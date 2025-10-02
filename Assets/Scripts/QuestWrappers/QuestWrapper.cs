using System;
using System.Collections.Generic;

public abstract class QuestWrapper
{
    private readonly Dictionary<int, string> stageDescriptions = new();
    private readonly HashSet<int> stagesToAdvanceOnLoopReset = new();
    private readonly Dictionary<int, QuestState> stageStateOverrides = new();

    protected QuestWrapper(string questName)
    {
        QuestName = questName;
    }

    public string QuestName { get; }

    protected void SetStageDescription(int stage, string description)
    {
        if (stage <= 0)
            return;

        stageDescriptions[stage] = description ?? string.Empty;
    }

    protected void MarkStageAsCompleted(params int[] stages)
    {
        MarkStageWithState(QuestState.Completed, stages);
    }

    protected void MarkStageAsFailed(params int[] stages)
    {
        MarkStageWithState(QuestState.Failed, stages);
    }

    private void MarkStageWithState(QuestState state, params int[] stages)
    {
        if (stages == null || stages.Length == 0)
            return;

        if (state != QuestState.Completed && state != QuestState.Failed)
            return;

        foreach (var stage in stages)
        {
            if (stage > 0)
                stageStateOverrides[stage] = state;
        }
    }

    private int GetMaxDefinedStage()
    {
        int max = 0;
        foreach (var key in stageDescriptions.Keys)
            if (key > max)
                max = key;
        return max;
    }

    public virtual void EnsureStageCapacity(QuestManager.Quest quest, int requiredStageCount)
    {
        if (quest == null)
            return;

        int maxStage = Math.Max(1, Math.Max(requiredStageCount, GetMaxDefinedStage()));

        quest.StageDescriptions.Clear();

        for (int stage = 1; stage <= maxStage; stage++)
        {
            if (stageDescriptions.TryGetValue(stage, out var desc))
                quest.StageDescriptions.Add(desc ?? string.Empty);
            else
                quest.StageDescriptions.Add(string.Empty);
        }
    }

    public virtual QuestState ProcessStateFromArticy(QuestManager.Quest quest, QuestState state) => state;

    public virtual int ProcessStageFromArticy(QuestManager.Quest quest, int stage) => stage;

    public virtual int ProcessResultFromArticy(QuestManager.Quest quest, int result) => result;

    protected void AddStagesToAdvanceOnLoopReset(params int[] stages)
    {
        if (stages == null)
            return;

        foreach (var stage in stages)
        {
            if (stage > 0)
                stagesToAdvanceOnLoopReset.Add(stage);
        }
    }

    public virtual void OnLoopReset(QuestManager.Quest quest)
    {
        if (quest == null)
            return;

        if (quest.State == QuestState.NotStarted)
            return;

        if (stagesToAdvanceOnLoopReset.Contains(quest.Stage))
            quest.Stage += 1;

        ApplyStageStateForCurrentStage(quest);
    }

    internal void ApplyStageStateForCurrentStage(QuestManager.Quest quest)
    {
        if (quest == null)
            return;

        if (stageStateOverrides.TryGetValue(quest.Stage, out var state))
            quest.State = state;
    }
}
