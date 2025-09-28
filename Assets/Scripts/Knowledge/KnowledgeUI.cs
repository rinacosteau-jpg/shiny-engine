using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class KnowledgeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject knowledgePanel;
    [SerializeField] private Transform knowledgeItemsParent;
    [SerializeField] private GameObject knowledgeItemPrefab;
    [SerializeField] private TMP_Text descriptionText;

    private readonly List<GameObject> spawnedKnowledgeItems = new();
    private KnowledgeManager.Knowledge selectedKnowledge;
    private string pendingSelectionName;

    private void OnEnable()
    {
        KnowledgeManager.KnowledgeChanged += HandleKnowledgeChanged;
    }

    private void OnDisable()
    {
        KnowledgeManager.KnowledgeChanged -= HandleKnowledgeChanged;
    }

    private void Start()
    {
        if (knowledgeItemPrefab != null)
            knowledgeItemPrefab.SetActive(false);

        Hide();
        Refresh();
    }

    public void Show()
    {
        Refresh();
        if (knowledgePanel != null)
            knowledgePanel.SetActive(true);
    }

    public void Hide()
    {
        if (knowledgePanel != null)
            knowledgePanel.SetActive(false);
    }

    public void Toggle()
    {
        if (knowledgePanel == null)
            return;

        if (knowledgePanel.activeSelf)
            Hide();
        else
            Show();
    }

    public void SelectKnowledge(KnowledgeManager.Knowledge knowledge)
    {
        selectedKnowledge = knowledge;
        DisplayKnowledge(knowledge);
    }

    public void Refresh()
    {
        if (knowledgeItemsParent == null || knowledgeItemPrefab == null)
        {
            DisplayKnowledge(null);
            return;
        }

        string previousSelectionName = !string.IsNullOrEmpty(pendingSelectionName)
            ? pendingSelectionName
            : selectedKnowledge?.Name;
        pendingSelectionName = null;

        foreach (var instance in spawnedKnowledgeItems)
        {
            if (instance != null)
                Destroy(instance);
        }
        spawnedKnowledgeItems.Clear();

        var knowledges = KnowledgeManager.GetAllKnowledges().ToList();

        selectedKnowledge = null;

        foreach (var knowledge in knowledges)
        {
            var instance = Instantiate(knowledgeItemPrefab, knowledgeItemsParent);
            instance.SetActive(true);

            var itemUI = instance.GetComponent<KnowledgeItemUI>() ?? instance.AddComponent<KnowledgeItemUI>();
            itemUI.Initialize(knowledge, this);
            spawnedKnowledgeItems.Add(instance);

            if (!string.IsNullOrEmpty(previousSelectionName) && string.Equals(knowledge.Name, previousSelectionName, StringComparison.Ordinal))
                selectedKnowledge = knowledge;
        }

        if (selectedKnowledge == null && knowledges.Count > 0)
            selectedKnowledge = knowledges[0];

        DisplayKnowledge(selectedKnowledge);
    }

    private void HandleKnowledgeChanged()
    {
        Refresh();
    }

    private void DisplayKnowledge(KnowledgeManager.Knowledge knowledge)
    {
        if (descriptionText == null)
            return;

        if (knowledge == null)
        {
            descriptionText.text = string.Empty;
            return;
        }

        descriptionText.text = FormatKnowledgeName(knowledge.Name);
    }

    internal void RequestSelectKnowledge(string knowledgeName)
    {
        if (!string.IsNullOrWhiteSpace(knowledgeName))
            pendingSelectionName = knowledgeName;
    }

    internal static string FormatKnowledgeName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return string.Empty;

        var builder = new StringBuilder(rawName.Length * 2);
        bool lastAppendedSpace = false;

        foreach (char character in rawName)
        {
            if (character == '_' || character == '-')
            {
                if (!lastAppendedSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                    lastAppendedSpace = true;
                }

                continue;
            }

            bool shouldInsertSpace = char.IsUpper(character)
                && builder.Length > 0
                && !lastAppendedSpace
                && char.IsLetterOrDigit(builder[builder.Length - 1])
                && char.IsLower(builder[builder.Length - 1]);

            if (shouldInsertSpace)
            {
                builder.Append(' ');
            }

            builder.Append(character);
            lastAppendedSpace = false;
        }

        return builder.ToString().Trim();
    }
}
