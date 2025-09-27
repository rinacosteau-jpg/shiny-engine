using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class JournalUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private Transform questItemsParent;
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private bool test = true;

    private readonly List<GameObject> spawnedQuestItems = new();
    private QuestManager.Quest selectedQuest;

    private void OnEnable()
    {
        QuestManager.OnQuestChanged += HandleQuestChanged;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestChanged -= HandleQuestChanged;
    }

    private void Start()
    {
        if (questItemPrefab != null)
            questItemPrefab.SetActive(false);

        Hide();
        Refresh();
    }

    public void Show()
    {
        Refresh();
        if (journalPanel != null)
            journalPanel.SetActive(true);
    }

    public void Hide()
    {
        if (journalPanel != null)
            journalPanel.SetActive(false);
    }

    public void Toggle()
    {
        if (journalPanel == null)
            return;

        if (journalPanel.activeSelf)
            Hide();
        else
            Show();
    }

    public void SelectQuest(QuestManager.Quest quest)
    {
        selectedQuest = quest;
        DisplayQuest(quest);
    }

    private void DisplayQuest(QuestManager.Quest quest)
    {
        if (descriptionText == null)
            return;

        if (quest == null)
        {
            descriptionText.text = string.Empty;
            return;
        }

        descriptionText.text = test
            ? QuestManager.DescribeQuest(quest)
            : quest.CurrentDescription ?? string.Empty;
    }

    private void HandleQuestChanged(QuestManager.Quest _)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (questItemsParent == null || questItemPrefab == null)
        {
            DisplayQuest(null);
            return;
        }

        string previousSelectionName = selectedQuest?.Name;

        foreach (var go in spawnedQuestItems)
        {
            if (go != null)
                Destroy(go);
        }
        spawnedQuestItems.Clear();

        var quests = QuestManager
            .GetAllQuests()
            .Where(q => test || q.State != QuestState.NotStarted)
            .OrderBy(q => q.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        selectedQuest = null;

        foreach (var quest in quests)
        {
            var instance = Instantiate(questItemPrefab, questItemsParent);
            instance.SetActive(true);

            var questItemUI = instance.GetComponent<JournalQuestItemUI>() ?? instance.AddComponent<JournalQuestItemUI>();
            questItemUI.Initialize(quest, this);
            spawnedQuestItems.Add(instance);

            if (!string.IsNullOrEmpty(previousSelectionName) && string.Equals(quest.Name, previousSelectionName, StringComparison.Ordinal))
                selectedQuest = quest;
        }

        if (selectedQuest == null && quests.Count > 0)
            selectedQuest = quests[0];

        DisplayQuest(selectedQuest);
    }
}
