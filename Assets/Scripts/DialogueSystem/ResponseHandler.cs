using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Articy.Unity;
using Articy.Unity.Interfaces;
using System.Reflection;
using Articy.World_Of_Red_Moon;

public class ResponseHandler : MonoBehaviour {
    [Header("UI Elements")]
    [SerializeField] private RectTransform responseBox;
    [SerializeField] private RectTransform responseButtonTemplate;
    [SerializeField] private RectTransform responseContainer;

    private List<GameObject> tempResponseButtons = new List<GameObject>();

    // Кол-во текущих ответов (для клавишного подтверждения)
    public int ResponsesCount => tempResponseButtons.Count;

    // Программно "кликнуть" по первой кнопке (клавишный confirm)
    public void ClickFirstResponse() {
        if (tempResponseButtons.Count == 0) return;
        var btn = tempResponseButtons[0].GetComponent<Button>();
        if (btn != null) btn.onClick.Invoke();
        Debug.Log("[ResponseHandler] ClickFirstResponse called");
    }

    /// <summary>
    /// Показывает кнопки для веток.
    /// Если playerEntity != null — показываются только ветки, чей target.speaker == playerEntity.
    /// Если playerEntity == null — показываются все ветки.
    /// </summary>
    public void ShowResponses(IList<Branch> branches, ArticyFlowPlayer flowPlayer, Entity playerEntity = null) {
        if (branches == null || flowPlayer == null) return;

        ClearResponses();

        foreach (var branch in branches) {
            if (branch == null || branch.Target == null) continue;

            // Фильтрация по спикеру (если задан playerEntity)
            if (playerEntity != null) {
                var targetSpeaker = GetSpeakerEntity(branch.Target);
                if (targetSpeaker == null || !ReferenceEquals(targetSpeaker, playerEntity))
                    continue; // не ответ игрока — пропускаем
            }

            CreateButtonForBranch(branch, flowPlayer);
        }

        responseBox.gameObject.SetActive(tempResponseButtons.Count > 0);
    }

    /// <summary>
    /// Создаёт одну кнопку-ответ с текстом. Если передан branch, при клике будет выполнен flowPlayer.Play(branch).
    /// Удобно для кнопки "Далее".
    /// </summary>
    public void CreateSingleResponse(string text, Branch branch, ArticyFlowPlayer flowPlayer) {
        if (string.IsNullOrEmpty(text) || flowPlayer == null) return;

        ClearResponses();

        var buttonObj = Instantiate(responseButtonTemplate.gameObject, responseContainer);
        buttonObj.SetActive(true);

        var tmp = buttonObj.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.text = text;

        var btnComponent = buttonObj.GetComponent<Button>();
        if (btnComponent != null) {
            btnComponent.onClick.RemoveAllListeners();
            if (branch != null)
                btnComponent.onClick.AddListener(() => flowPlayer.Play(branch));
            else
                btnComponent.onClick.AddListener(() => flowPlayer.Play());
        }

        tempResponseButtons.Add(buttonObj);
        responseBox.gameObject.SetActive(true);
        Debug.Log("[ResponseHandler] Single button created: " + text);
    }

    /// <summary>
    /// Удаляет все текущие кнопки и прячет контейнер.
    /// </summary>
    public void ClearResponses() {
        foreach (var btn in tempResponseButtons)
            Destroy(btn);
        tempResponseButtons.Clear();

        if (responseBox != null)
            responseBox.gameObject.SetActive(false);
    }

    // --- Внутренние вспомогательные методы ---

    private void CreateButtonForBranch(Branch branch, ArticyFlowPlayer flowPlayer) {
        if (branch == null || branch.Target == null || flowPlayer == null) return;

        GameObject buttonObj = Instantiate(responseButtonTemplate.gameObject, responseContainer);
        buttonObj.SetActive(true);

        // Для кнопки сначала пробуем MenuText, затем Text, затем fallback
        string buttonText = GetMenuTextFromFlowObject(branch.Target);
        if (string.IsNullOrEmpty(buttonText))
            buttonText = GetTextFromFlowObject(branch.Target);
        if (string.IsNullOrEmpty(buttonText))
            buttonText = branch.Target.GetType().Name;

        TMP_Text tmpText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (tmpText != null) tmpText.text = buttonText;

        var localBranch = branch; // фиксируем для замыкания
        Button btnComponent = buttonObj.GetComponent<Button>();
        if (btnComponent != null) {
            btnComponent.onClick.RemoveAllListeners();
            btnComponent.onClick.AddListener(() => {
                OnPickedResponse(localBranch, flowPlayer);
            });
        }

        tempResponseButtons.Add(buttonObj);
    }

    private void OnPickedResponse(Branch branch, ArticyFlowPlayer flowPlayer) {
        ClearResponses();

        if (flowPlayer != null && branch != null)
            flowPlayer.Play(branch);
        Debug.Log("picked");
    }

    /// <summary>
    /// Получает MenuText из FlowObject (IObjectWithMenuText или через reflection).
    /// </summary>
    private string GetMenuTextFromFlowObject(IFlowObject obj) {
        if (obj == null) return null;

        var objWithMenu = obj as IObjectWithMenuText;
        if (objWithMenu != null && !string.IsNullOrEmpty(objWithMenu.MenuText))
            return objWithMenu.MenuText;

        try {
            var type = obj.GetType();
            var menuProp = type.GetProperty("MenuText", BindingFlags.Public | BindingFlags.Instance);
            if (menuProp != null) {
                var val = menuProp.GetValue(obj);
                if (val != null) return val.ToString();
            }
        } catch { }

        return null;
    }

    /// <summary>
    /// Безопасно вытягивает текст из FlowObject — интерфейс, property Text или Properties.Text
    /// </summary>
    private string GetTextFromFlowObject(IFlowObject obj) {
        if (obj == null) return null;

        var objWithText = obj as IObjectWithText;
        if (objWithText != null && !string.IsNullOrEmpty(objWithText.Text))
            return objWithText.Text;

        try {
            var type = obj.GetType();
            var textProp = type.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
            if (textProp != null) {
                var val = textProp.GetValue(obj);
                if (val != null) return val.ToString();
            }

            var propsProp = type.GetProperty("Properties");
            if (propsProp != null) {
                var props = propsProp.GetValue(obj);
                if (props != null) {
                    var txtProp = props.GetType().GetProperty("Text");
                    if (txtProp != null) {
                        var val = txtProp.GetValue(props);
                        if (val != null) return val.ToString();
                    }
                }
            }
        } catch { }

        return null;
    }

    /// <summary>
    /// Пытается достать Entity-спикера из FlowObject.
    /// </summary>
    private Entity GetSpeakerEntity(IFlowObject obj) {
        if (obj == null) return null;

        if (obj is IObjectWithSpeaker withSpeaker && withSpeaker.Speaker is Entity ent1)
            return ent1;

        try {
            var prop = obj.GetType().GetProperty("Speaker", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) {
                var sp = prop.GetValue(obj);
                if (sp is Entity ent2) return ent2;
            }
        } catch { }

        return null;
    }
}
