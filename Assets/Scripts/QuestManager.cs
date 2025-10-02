// QuestManager.cs
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon; // çàìåíè íà ñâîé, åñëè Articy ñãåíåðèë äðóãîé namespace
using Articy.World_Of_Red_Moon.GlobalVariables;

public enum QuestState { NotStarted = 0, Active = 1, Completed = 2, Failed = 3 }

public static class QuestManager {
    static QuestManager()
    {
        RegisterWrapper(new StealFromRuQuestWrapper());
        RegisterWrapper(new BackgroundCheckQuestWrapper());
        RegisterWrapper(new FindMemoriesQuestWrapper());
        RegisterWrapper(new AdvertiseQuestWrapper());
        RegisterWrapper(new GetArtefactQuestWrapper());
        RegisterWrapper(new PreventMurderAttemptQuestWrapper());
        RegisterWrapper(new GetGunQuestWrapper());
    }

    public static IEnumerable<Quest> GetAllQuests() => quests.Values;

    public static string DescribeQuest(Quest quest)
    {
        if (quest == null)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine(quest.Name);
        sb.AppendLine($"State: {quest.State}");
        sb.AppendLine($"Stage: {quest.Stage}");
        sb.AppendLine($"Result: {quest.Result}");
        sb.AppendLine($"Current Description: {quest.CurrentDescription}");
        sb.AppendLine($"Stage Count: {quest.StageCount}");
        if (quest.StageDescriptions.Count > 0)
        {
            sb.AppendLine("Stage Descriptions:");
            for (int i = 0; i < quest.StageDescriptions.Count; i++)
                sb.AppendLine($"  Stage {i + 1}: {quest.StageDescriptions[i]}");
        }

        if (quest.Objectives.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Objectives:");
            foreach (var obj in quest.Objectives.Values)
            {
                sb.Append("- ");
                sb.Append(obj.Id);
                sb.Append(": ");
                sb.Append(obj.State);
                if (obj.Optional)
                    sb.Append(" (optional)");
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    // ======== Ìîäåëü ========
    public class Objective {
        public string Id;                 // íàïð. "A", "B", "C"
        public QuestState State;
        public bool Optional;
    }

    public class Quest {
        public string Name;               // ýòî è åñòü ID êâåñòà
        public QuestState State;
        public int Stage;                 // ëèíåéíûé ïðîãðåññ/÷åêïîèíò
        public int Result;                // òåêóùèé èòîã êâåñòà
        public readonly List<string> StageDescriptions = new();
        public readonly Dictionary<string, Objective> Objectives = new();
        public bool LoopChanged;

        public int StageCount => StageDescriptions.Count;

        public string CurrentDescription
        {
            get
            {
                return GetStageDescription(Stage);
            }
        }

        public string GetStageDescription(int stageIndex)
        {
            if (StageDescriptions.Count == 0)
                return string.Empty;

            if (stageIndex <= 0)
                stageIndex = 1;

            if (stageIndex > StageDescriptions.Count)
                stageIndex = StageDescriptions.Count;

            return StageDescriptions[stageIndex - 1] ?? string.Empty;
        }
    }

    // ======== Ñîáûòèÿ ========
    public static event Action<Quest> OnQuestChanged;

    // ======== Õðàíèëèùå ========
    private static readonly Dictionary<string, Quest> quests = new();
    private static readonly Dictionary<string, QuestWrapper> wrappers = new(StringComparer.Ordinal);

    public static void RegisterWrapper(QuestWrapper wrapper)
    {
        if (wrapper == null || string.IsNullOrEmpty(wrapper.QuestName))
            return;

        wrappers[wrapper.QuestName] = wrapper;
    }

    private static bool TryGetWrapper(string questName, out QuestWrapper wrapper)
    {
        if (string.IsNullOrEmpty(questName))
        {
            wrapper = null;
            return false;
        }

        return wrappers.TryGetValue(questName, out wrapper);
    }

    // Ãëóøèëêà äëÿ PushToArticy âî âðåìÿ Sync
    private static bool _mutePush;

    // Áûñòðûå ññûëêè íà íàáîðû ãëîáàëîê Articy
    private static Articy.World_Of_Red_Moon.GlobalVariables.QUEST QuestVariables => ArticyGlobalVariables.Default.QUEST;

    private static void EnsureStageCapacity(Quest q, int requiredStageCount)
    {
        if (q == null)
            return;

        if (requiredStageCount <= 0)
            requiredStageCount = 1;

        if (TryGetWrapper(q.Name, out var wrapper))
        {
            wrapper.EnsureStageCapacity(q, requiredStageCount);
            return;
        }

        while (q.StageDescriptions.Count < requiredStageCount)
        {
            q.StageDescriptions.Add(string.Empty);
        }
    }

    private static void ApplyWrapperStageState(Quest quest)
    {
        if (quest == null)
            return;

        if (TryGetWrapper(quest.Name, out var wrapper))
            wrapper.ApplyStageStateForCurrentStage(quest);
    }

    // ======== Õåëïåðû ========
    // isTemp: null — íå òðîãàåì; true — âðåìåííûé (RQUE); false — ïîñòîÿííûé (NQUE).
    private static Quest Ensure(string name) {
        if (!quests.TryGetValue(name, out var q)) {
            q = new Quest {
                Name = name,
                State = QuestState.NotStarted,
                Stage = 0,
                Result = 0,
                LoopChanged = false
            };
            quests[name] = q;
        }
        if (TryGetWrapper(name, out var wrapper))
            wrapper.EnsureStageCapacity(q, Math.Max(q.Stage, 1));
        return q;
    }

    private static void EnsureObjective(Quest q, string objId, QuestState state, bool optional = false) {
        if (!q.Objectives.TryGetValue(objId, out var obj))
            q.Objectives[objId] = obj = new Objective { Id = objId, Optional = optional, State = QuestState.NotStarted };
        obj.State = state;
    }

    private static void RaiseQuestChanged(Quest q) => OnQuestChanged?.Invoke(q);

    // ======== Ïóáëè÷íûé API ========
    public static bool Has(string name) => quests.ContainsKey(name);
    public static Quest Get(string name) => quests.TryGetValue(name, out var q) ? q : null;
    public static bool IsActive(string name) => quests.TryGetValue(name, out var q) && q.State == QuestState.Active;
    public static bool IsCompleted(string name) => quests.TryGetValue(name, out var q) && q.State == QuestState.Completed;

    /// <summary>Çàïóñòè ïîñòîÿííûé êâåñò: Start(name, isTemp:false) èëè âðåìåííûé: Start(name, isTemp:true)</summary>
    public static Quest Start(string name) {
        var q = Ensure(name);
        q.State = QuestState.Active;
        if (q.Stage == 0) q.Stage = 1;
        EnsureStageCapacity(q, q.Stage);
        PushToArticy(q);
        RaiseQuestChanged(q);
        return q;
    }

    public static void Fail(string name) {
        if (!quests.TryGetValue(name, out var q)) return;
        q.State = QuestState.Failed;
        EnsureStageCapacity(q, q.Stage);
        PushToArticy(q);
        RaiseQuestChanged(q);
    }

    public static void Complete(string name) {
        if (!quests.TryGetValue(name, out var q)) return;
        q.State = QuestState.Completed;
        EnsureStageCapacity(q, q.Stage);
        PushToArticy(q);
        RaiseQuestChanged(q);
    }

    public static void SetStage(string name, int stage) {
        var q = Start(name);
        q.Stage = stage;
        if (q.State == QuestState.NotStarted) q.State = QuestState.Active;
        ApplyWrapperStageState(q);
        EnsureStageCapacity(q, stage);
        PushToArticy(q);
        RaiseQuestChanged(q);
    }

    public static void SetObjectiveState(string quest, string objId, QuestState state, bool optional = false) {
        var q = Start(quest);
        EnsureObjective(q, objId, state, optional);
        PushToArticy(q);
        RaiseQuestChanged(q);
    }

    public static QuestState GetObjectiveState(string quest, string objId) {
        return quests.TryGetValue(quest, out var q) && q.Objectives.TryGetValue(objId, out var obj)
            ? obj.State
            : QuestState.NotStarted;
    }

    public static void OnLoopReset()
    {
        foreach (var quest in quests.Values)
        {
            quest.LoopChanged = false;

            int previousStage = quest.Stage;

            if (TryGetWrapper(quest.Name, out var wrapper))
            {
                wrapper.OnLoopReset(quest);
                wrapper.EnsureStageCapacity(quest, Math.Max(quest.Stage, 1));
                wrapper.ApplyStageStateForCurrentStage(quest);
            }
            else if ((quest.Stage & 1) == 1)
            {
                quest.Stage += 1;
                EnsureStageCapacity(quest, Math.Max(quest.Stage, 1));
            }

            if (quest.Stage != previousStage)
            {
                quest.LoopChanged = true;
                PushToArticy(quest);
                RaiseQuestChanged(quest);
            }
        }
    }

    public static void ResetAll() => quests.Clear();

    public static string DisplayQuests() {
        var sb = new StringBuilder();
        foreach (var q in quests.Values) {
            sb.Append($"{q.Name} [State:{q.State}, Stage:{q.Stage}, Result:{q.Result}]");
            if (q.Objectives.Count > 0) {
                sb.Append(" {");
                bool first = true;
                foreach (var obj in q.Objectives.Values) {
                    if (!first) sb.Append(", ");
                    sb.Append($"{obj.Id}:{obj.State}");
                    first = false;
                }
                sb.Append("}");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    // ======== Ñèíõðîíèçàöèÿ ñ Articy ========
    // Ïóø â Articy ïî ñîãëàøåíèþ èì¸í:
    // <Quest>_State (int), <Quest>_Stage (int), <Quest>_Obj_<Id> (int)
    // Äîïîëíèòåëüíî: <Quest>_ObjectivesCompleted (int) åñëè ñâîéñòâî ñóùåñòâóåò.
    private static void PushToArticy(Quest q) {
        if (_mutePush) return; // íå óñòðàèâàåì ïèíã-ïîíã âî âðåìÿ Sync

        try {
            var gv = QuestVariables;

            SetInt(gv, $"{q.Name}_State", (int)q.State);
            SetInt(gv, $"{q.Name}_Result", q.Result);
            SetInt(gv, $"{q.Name}_Stage", q.Stage);

            int completedObjectives = 0;
            foreach (var obj in q.Objectives.Values) {
                SetInt(gv, $"{q.Name}_Obj_{obj.Id}", (int)obj.State);
                if (obj.State == QuestState.Completed) completedObjectives++;
            }

            // Îáùåå èìÿ ñ÷¸ò÷èêà (åñëè åñòü — ïîñòàâèì):
            SetIntIfExists(gv, $"{q.Name}_ObjectivesCompleted", completedObjectives);
            // Ñîâìåñòèìîñòü ñ «advertise_TalkedCount», åñëè òàêàÿ ïåðåìåííàÿ åñòü:
            SetIntIfExists(gv, $"{q.Name}_TalkedCount", completedObjectives);
        } catch (Exception e) {
            Debug.LogWarning($"QuestManager.PushToArticy: {e.Message}");
        }
    }

    // Îáíóëèòü âñå int/bool ïåðåìåííûå êâåñòà ïî ïðåôèêñó "<questName>_" âíóòðè óêàçàííîãî íàáîðà (RQUE èëè NQUE).
    // Ïîäòÿæêà ñîñòîÿíèÿ èç Articy â ëîêàëüíîå õðàíèëèùå.
    // ÷èòàåì íàáîð QUEST è îáíîâëÿåì ëîêàëüíûå äàííûå êâåñòîâ.
    public static void SyncFromArticy() {
        var questsToPush = new HashSet<string>(StringComparer.Ordinal);

        _mutePush = true;
        try {
            ProcessQuestVariables(QuestVariables, questsToPush);
        } finally {
            _mutePush = false;
        }

        foreach (var questName in questsToPush) {
            if (quests.TryGetValue(questName, out var quest))
                PushToArticy(quest);
        }
    }

    private static void ProcessQuestVariables(object gv, ISet<string> questsToPush) {
        try {
            var props = gv.GetType().GetProperties();

            foreach (var p in props) {
                if (p.PropertyType == typeof(int)) {
                    var key = p.Name;
                    int val = (int)p.GetValue(gv);

                    if (key.EndsWith("_State")) {
                        var questName = key.Substring(0, key.Length - "_State".Length);
                        var q = Ensure(questName);
                        var newState = (QuestState)val;
                        if (TryGetWrapper(questName, out var stateWrapper))
                            newState = stateWrapper.ProcessStateFromArticy(q, newState);
                        q.State = newState;
                        ApplyWrapperStageState(q);
                        if (q.State != (QuestState)val)
                            questsToPush?.Add(questName);
                        EnsureStageCapacity(q, q.Stage);
                        RaiseQuestChanged(q);
                    } else if (key.EndsWith("_Stage")) {
                        var questName = key.Substring(0, key.Length - "_Stage".Length);
                        var q = Ensure(questName);
                        int newStage = val;
                        if (TryGetWrapper(questName, out var stageWrapper))
                            newStage = stageWrapper.ProcessStageFromArticy(q, newStage);
                        q.Stage = newStage;
                        if (q.State == QuestState.NotStarted && newStage > 0) q.State = QuestState.Active;
                        ApplyWrapperStageState(q);
                        if (q.Stage != val)
                            questsToPush?.Add(questName);
                        EnsureStageCapacity(q, newStage);
                        RaiseQuestChanged(q);
                    } else if (key.EndsWith("_Result")) {
                        var questName = key.Substring(0, key.Length - "_Result".Length);
                        var q = Ensure(questName);
                        int newResult = val;
                        if (TryGetWrapper(questName, out var resultWrapper))
                            newResult = resultWrapper.ProcessResultFromArticy(q, newResult);
                        q.Result = newResult;
                        if (q.Result != val)
                            questsToPush?.Add(questName);
                        EnsureStageCapacity(q, q.Stage);
                        RaiseQuestChanged(q);
                    } else if (key.Contains("_Obj_")) {
                        var parts = key.Split(new[] { "_Obj_" }, StringSplitOptions.None);
                        if (parts.Length == 2) {
                            var questName = parts[0];
                            var objId = parts[1];
                            var q = Ensure(questName);
                            EnsureObjective(q, objId, (QuestState)val);
                            RaiseQuestChanged(q);
                        }
                    }
                } else if (p.PropertyType == typeof(bool)) {
                    // Ëåãàñè: *_Started == true -> àêòèâèðóåì êâåñò
                    bool started = (bool)p.GetValue(gv);
                    if (started && p.Name.EndsWith("_Started")) {
                        string questName = p.Name.Substring(0, p.Name.Length - "_Started".Length);
                        var q = Ensure(questName);
                        if (q.State == QuestState.NotStarted) {
                            q.State = QuestState.Active;
                            if (q.Stage == 0) q.Stage = 1;
                            RaiseQuestChanged(q);
                        }
                    }
                }
            }
        } catch (Exception e) {
            Debug.LogWarning($"QuestManager.ProcessQuestVariables: {e.Message}");
        }
    }

    // ======== Ðåôëåêñèÿ-ïîìîùíèêè ========
    private static void SetInt(object gv, string propName, int value) {
        var p = gv.GetType().GetProperty(propName);
        if (p != null && p.PropertyType == typeof(int)) p.SetValue(gv, value);
    }

    private static void SetIntIfExists(object gv, string propName, int value) {
        var p = gv.GetType().GetProperty(propName);
        if (p != null && p.PropertyType == typeof(int)) p.SetValue(gv, value);
    }

    private static void SetBool(object gv, string propName, bool value) {
        var p = gv.GetType().GetProperty(propName);
        if (p != null && p.PropertyType == typeof(bool)) p.SetValue(gv, value);
    }
}
