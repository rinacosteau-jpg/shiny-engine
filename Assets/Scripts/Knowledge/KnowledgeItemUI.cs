using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnowledgeItemUI : MonoBehaviour
{
    private KnowledgeManager.Knowledge knowledge;
    private KnowledgeUI knowledgeUI;

    public void Initialize(KnowledgeManager.Knowledge knowledge, KnowledgeUI ui)
    {
        this.knowledge = knowledge;
        knowledgeUI = ui;

        UpdateLabel();

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }
    }

    private void UpdateLabel()
    {
        var text = GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = KnowledgeUI.FormatKnowledgeName(knowledge?.Name ?? string.Empty);
    }

    private void OnClick()
    {
        if (knowledge != null)
            knowledgeUI?.SelectKnowledge(knowledge);
    }
}
