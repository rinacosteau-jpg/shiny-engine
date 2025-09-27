using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalQuestItemUI : MonoBehaviour
{
    private QuestManager.Quest quest;
    private JournalUI journalUI;

    public void Initialize(QuestManager.Quest quest, JournalUI journal)
    {
        this.quest = quest;
        journalUI = journal;

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
            text.text = quest?.Name ?? string.Empty;
    }

    private void OnClick()
    {
        if (quest != null)
            journalUI?.SelectQuest(quest);
    }
}
