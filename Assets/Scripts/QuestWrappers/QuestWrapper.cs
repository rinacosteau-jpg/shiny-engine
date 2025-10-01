using System;
using System.Collections.Generic;

public abstract class QuestWrapper
{
    private readonly Dictionary<int, string> stageDescriptions = new();

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

    public virtual void OnLoopReset(QuestManager.Quest quest)
    {
        if (quest == null)
            return;

        if (quest.State == QuestState.NotStarted)
            return;

        if ((quest.Stage & 1) == 1)
            quest.Stage += 1;
    }
}
