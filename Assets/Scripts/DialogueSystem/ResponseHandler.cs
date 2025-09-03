using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResponseHandler : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform responseBox;
    [SerializeField] private RectTransform responseButtonTemplate;
    [SerializeField] private RectTransform responseContainer;

    private readonly List<GameObject> tempResponseButtons = new List<GameObject>();

    public void ShowResponses(IList<ChoiceData> choices, DialogueController controller)
    {
        ClearResponses();
        foreach (var choice in choices)
        {
            CreateButton(choice, controller);
        }
        responseBox.gameObject.SetActive(tempResponseButtons.Count > 0);
    }

    public void DisableAll()
    {
        foreach (var go in tempResponseButtons)
        {
            var btn = go.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
    }

    public void ClearResponses()
    {
        foreach (var btn in tempResponseButtons)
            Destroy(btn);
        tempResponseButtons.Clear();
        if (responseBox != null)
            responseBox.gameObject.SetActive(false);
    }

    private void CreateButton(ChoiceData choice, DialogueController controller)
    {
        var buttonObj = Instantiate(responseButtonTemplate.gameObject, responseContainer);
        buttonObj.SetActive(true);
        var tmp = buttonObj.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.text = choice.Text;
        var btn = buttonObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => controller.SelectChoice(choice));
        }
        tempResponseButtons.Add(buttonObj);
    }
}
