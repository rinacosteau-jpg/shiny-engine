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
    [SerializeField] private Button continueButton;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private ScrollRect scrollrect;

    private List<GameObject> tempResponseButtons = new List<GameObject>();

    private void Awake() {
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
    }

    public void ShowResponses(IList<Branch> branches, ArticyFlowPlayer flowPlayer, Entity playerEntity = null) {
        if (branches == null || flowPlayer == null) return;

        ClearResponses();

        int responseIndex = 1;
        foreach (var branch in branches) {
            if (branch == null || branch.Target == null) continue;

            // Ôèëüòðàöèÿ ïî ñïèêåðó (åñëè çàäàí playerEntity)
            if (playerEntity != null) {
                var targetSpeaker = GetSpeakerEntity(branch.Target);
                if (targetSpeaker == null || !ReferenceEquals(targetSpeaker, playerEntity))
                    continue; // íå îòâåò èãðîêà — ïðîïóñêàåì
            }

            CreateButtonForBranch(branch, flowPlayer, responseIndex);
            responseIndex++;
        }
        
        responseBox.gameObject.SetActive(tempResponseButtons.Count > 0);
    }

    /// <summary>
    /// Ñîçäà¸ò îäíó êíîïêó-îòâåò ñ òåêñòîì. Åñëè ïåðåäàí branch, ïðè êëèêå áóäåò âûïîëíåí flowPlayer.Play(branch).
    /// Óäîáíî äëÿ êíîïêè "Äàëåå".
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
            btnComponent.onClick.AddListener(() => {
                if (branch != null)
                    flowPlayer.Play(branch);
                else
                    flowPlayer.Play();

                if (scrollbar != null) {
                    Canvas.ForceUpdateCanvases();
                    scrollrect.verticalNormalizedPosition = 0f;
                    scrollbar.value = 0f;
                    Debug.Log("scrollbar val changed");
                    Canvas.ForceUpdateCanvases();
                }
                    
            });
        }

        tempResponseButtons.Add(buttonObj);
        responseBox.gameObject.SetActive(true);
        Debug.Log("[ResponseHandler] Single button created: " + text);
    }

    /// <summary>
    /// Óäàëÿåò âñå òåêóùèå êíîïêè è ïðÿ÷åò êîíòåéíåð.
    /// </summary>
    public void ClearResponses() {
        foreach (var btn in tempResponseButtons)
            Destroy(btn);
        tempResponseButtons.Clear();
    }

    public void ShowContinueButton(Branch branch, ArticyFlowPlayer flowPlayer) {
        if (continueButton == null || flowPlayer == null) return;

        ClearResponses();

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() => {
            continueButton.gameObject.SetActive(false);
            if (branch != null)
                flowPlayer.Play(branch);
            else
                flowPlayer.Play();

            if (scrollbar != null) {
                Canvas.ForceUpdateCanvases();
                scrollrect.verticalNormalizedPosition = 0f;
                scrollbar.value = 0f;
                Debug.Log("scrollbar val changed");
                Canvas.ForceUpdateCanvases();
            }
        });
    }

    // --- Âíóòðåííèå âñïîìîãàòåëüíûå ìåòîäû ---

    private void CreateButtonForBranch(Branch branch, ArticyFlowPlayer flowPlayer, int responseIndex) {
        if (branch == null || branch.Target == null || flowPlayer == null) return;

        GameObject buttonObj = Instantiate(responseButtonTemplate.gameObject, responseContainer);
        buttonObj.SetActive(true);

        // Äëÿ êíîïêè ñíà÷àëà ïðîáóåì MenuText, çàòåì Text, çàòåì fallback
        string buttonText = GetMenuTextFromFlowObject(branch.Target);
        if (string.IsNullOrEmpty(buttonText))
            buttonText = GetTextFromFlowObject(branch.Target);
        if (string.IsNullOrEmpty(buttonText))
            buttonText = branch.Target.GetType().Name;

        TMP_Text tmpText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (tmpText != null) tmpText.text = $"{responseIndex}. {buttonText}";

        var localBranch = branch; // ôèêñèðóåì äëÿ çàìûêàíèÿ
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

        if (scrollbar != null) {
            Canvas.ForceUpdateCanvases();
            scrollrect.verticalNormalizedPosition = 0f;
            scrollbar.value = 0f;
            Debug.Log("scrollbar val changed");
            Canvas.ForceUpdateCanvases();
        }
            

        Debug.Log("picked");
    }

    /// <summary>
    /// Ïîëó÷àåò MenuText èç FlowObject (IObjectWithMenuText èëè ÷åðåç reflection).
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
    /// Áåçîïàñíî âûòÿãèâàåò òåêñò èç FlowObject — èíòåðôåéñ, property Text èëè Properties.Text
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
    /// Ïûòàåòñÿ äîñòàòü Entity-ñïèêåðà èç FlowObject.
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
