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
        sb.AppendLine($"Temporary: {quest.IsTemporary}");
        sb.AppendLine($"Description: {quest.Description}");
        sb.AppendLine($"Current Description: {quest.CurrentDescription}");
        sb.AppendLine($"Failed Description: {quest.FailedDescription}");
        sb.AppendLine($"Stage Count: {quest.StageCount}");
        if (quest.StageDescriptions.Count > 0)
        {
            sb.AppendLine("Stage Descriptions:");
            for (int i = 0; i < quest.StageDescriptions.Count; i++)
                sb.AppendLine($"  Stage {i + 1}: {quest.StageDescriptions[i]}");
        }
        sb.AppendLine($"Result Count: {quest.ResultCount}");
        if (quest.ResultDescriptions.Count > 0)
        {
            sb.AppendLine("Result Descriptions:");
            for (int i = 0; i < quest.ResultDescriptions.Count; i++)
                sb.AppendLine($"  Result {i + 1}: {quest.ResultDescriptions[i]}");
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
        public bool IsTemporary;          // RQUE = true, NQUE = false
        public string Description;
        public string FailedDescription;
        public readonly List<string> StageDescriptions = new();
        public readonly List<string> ResultDescriptions = new();
        public readonly Dictionary<string, Objective> Objectives = new();

        public int StageCount => StageDescriptions.Count;
        public int ResultCount => ResultDescriptions.Count;

        public string CurrentDescription
        {
            get
            {
                return State switch
                {
                    QuestState.Active => GetStageDescription(Stage),
                    QuestState.Completed => GetResultDescription(Result),
                    QuestState.Failed => FailedDescription ?? string.Empty,
                    _ => Description ?? string.Empty,
                };
            }
        }

        public string GetStageDescription(int stageIndex)
        {
            if (StageDescriptions.Count == 0)
                return Description ?? string.Empty;

            if (stageIndex <= 0)
                stageIndex = 1;

            if (stageIndex > StageDescriptions.Count)
                stageIndex = StageDescriptions.Count;

            return StageDescriptions[stageIndex - 1];
        }

        public string GetResultDescription(int resultIndex)
        {
            if (ResultDescriptions.Count == 0)
                return Description ?? string.Empty;

            if (resultIndex <= 0)
                resultIndex = 1;

            if (resultIndex > ResultDescriptions.Count)
                resultIndex = ResultDescriptions.Count;

            return ResultDescriptions[resultIndex - 1];
        }
    }

    // ======== Ñîáûòèÿ ========
    public static event Action<Quest> OnQuestChanged;

    // ======== Õðàíèëèùå ========
    private static readonly Dictionary<string, Quest> quests = new();

    // Ãëóøèëêà äëÿ PushToArticy âî âðåìÿ Sync
    private static bool _mutePush;

    // Áûñòðûå ññûëêè íà íàáîðû ãëîáàëîê Articy
    private static object RQUE => ArticyGlobalVariables.Default.RQUE;
    private static object NQUE => ArticyGlobalVariables.Default.NQUE;

    private static string GenerateQuestDescription(string questName) => $"Quest {questName} overview {Guid.NewGuid():N}";
    private static string GenerateFailedDescription(string questName) => $"Quest {questName} failure {Guid.NewGuid():N}";
    private static string GenerateStageDescription(string questName, int index) => $"Quest {questName} stage {index} description {Guid.NewGuid():N}";
    private static string GenerateResultDescription(string questName, int index) => $"Quest {questName} result {index} description {Guid.NewGuid():N}";

    private static void EnsureQuestMetadata(Quest q)
    {
        if (q == null)
            return;

        if (string.IsNullOrEmpty(q.Description))
            q.Description = GenerateQuestDescription(q.Name);

        if (string.IsNullOrEmpty(q.FailedDescription))
            q.FailedDescription = GenerateFailedDescription(q.Name);

        if (q.StageDescriptions.Count == 0)
            q.StageDescriptions.Add(GenerateStageDescription(q.Name, 1));

        if (q.ResultDescriptions.Count == 0)
            q.ResultDescriptions.Add(GenerateResultDescription(q.Name, 1));
    }

    private static void EnsureStageCapacity(Quest q, int requiredStageCount)
    {
        if (q == null)
            return;

        EnsureQuestMetadata(q);

        if (requiredStageCount <= 0)
            requiredStageCount = 1;

        while (q.StageDescriptions.Count < requiredStageCount)
        {
            int nextIndex = q.StageDescriptions.Count + 1;
            q.StageDescriptions.Add(GenerateStageDescription(q.Name, nextIndex));
        }
    }

    private static void EnsureResultCapacity(Quest q, int requiredResultCount)
    {
        if (q == null)
            return;

        EnsureQuestMetadata(q);

        if (requiredResultCount <= 0)
            requiredResultCount = 1;

        while (q.ResultDescriptions.Count < requiredResultCount)
        {
            int nextIndex = q.ResultDescriptions.Count + 1;
            q.ResultDescriptions.Add(GenerateResultDescription(q.Name, nextIndex));
        }
    }

    // ======== Õåëïåðû ========
    // isTemp: null — íå òðîãàåì; true — âðåìåííûé (RQUE); false — ïîñòîÿííûé (NQUE).
    private static Quest Ensure(string name, bool? isTemp = null) {
        if (!quests.TryGetValue(name, out var q)) {
            q = new Quest {
                Name = name,
                IsTemporary = isTemp ?? false, // ïî óìîë÷àíèþ ñ÷èòàåì ïåðñèñòåíòíûì
                State = QuestState.NotStarted,
                Stage = 0,
                Result = 0
            };
            quests[name] = q;
        } else if (isTemp.HasValue) {
            // Åñëè ñâåäåíèÿ ïðèøëè èç NQUE — äà¸ì ïðèîðèòåò "íå âðåìåííûì".
            if (isTemp.Value == false && q.IsTemporary)
                q.IsTemporary = false;
            // Åñëè óæå non-temp, íå ïåðåâîäèì îáðàòíî â temp.
            // Åñëè áûë temp è ïðèøëî temp — îñòàâëÿåì temp.
        }
        EnsureQuestMetadata(q);
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
    public static Quest Start(string name, bool isTemp = false) {
        var q = Ensure(name, isTemp);
        q.State = QuestState.Active;
        if (q.Stage == 0) q.Stage = 1;
        EnsureStageCapacity(q, q.Stage);
        EnsureResultCapacity(q, q.Result);
        PushToArticy(q);
        RaiseQuestChanged(q);
        return q;
    }

    public static void Fail(string name) {
        if (!quests.TryGetValue(name, out var q)) return;
        q.State = QuestState.Failed;
        EnsureStageCapacity(q, q.Stage);
        EnsureResultCapacity(q, q.Result);
        PushToArticy(q);
        RaiseQuestChanged(q);
    }

    public static void Complete(string name) {
        if (!quests.TryGetValue(name, out var q)) return;
        q.State = QuestState.Completed;
        EnsureStageCapacity(q, q.Stage);
        EnsureResultCapacity(q, q.Result);
        PushToArticy(q);
        RaiseQuestChanged(q);
    }

    public static void SetStage(string name, int stage) {
        var q = Start(name); // òèï (temp/non-temp) óæå äîëæåí áûòü îïðåäåë¸í ðàíåå èëè ïî óìîë÷àíèþ non-temp
        q.Stage = stage;
        if (q.State == QuestState.NotStarted) q.State = QuestState.Active;
        EnsureStageCapacity(q, stage);
        EnsureResultCapacity(q, q.Result);
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

    public static void ResetTemporary() {
        var toRemove = new List<string>();
        foreach (var kv in quests)
            if (kv.Value.IsTemporary) toRemove.Add(kv.Key);

        foreach (var name in toRemove) {
            // 1) îáíóëÿåì çåðêàëüíûå ïåðåìåííûå èìåííî â RQUE (òîëüêî ïî ïðåôèêñó êâåñòà)
            ClearQuestInArticy(name, RQUE);

            // 2) óáèðàåì èç ëîêàëüíîãî ðååñòðà
            quests.Remove(name);
        }
    }

    public static void ResetAll() => quests.Clear();

    public static string DisplayQuests() {
        var sb = new StringBuilder();
        foreach (var q in quests.Values) {
            sb.Append($"{q.Name} [State:{q.State}, Stage:{q.Stage}, Result:{q.Result}, Temp:{q.IsTemporary}]");
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
            var gv = q.IsTemporary ? RQUE : NQUE;

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
    private static void ClearQuestInArticy(string questName, object gvset) {
        try {
            var type = gvset.GetType();
            foreach (var p in type.GetProperties()) {
                if (!p.Name.StartsWith(questName + "_")) continue;

                if (p.PropertyType == typeof(int))
                    p.SetValue(gvset, 0);
                else if (p.PropertyType == typeof(bool))
                    p.SetValue(gvset, false);
            }
        } catch (System.Exception e) {
            UnityEngine.Debug.LogWarning($"ClearQuestInArticy({questName}): {e.Message}");
        }
    }

    // Ïîäòÿæêà ñîñòîÿíèÿ èç Articy â ëîêàëüíîå õðàíèëèùå.
    // Òåïåðü ÷èòàåì è RQUE (isTemp=true), è NQUE (isTemp=false).
    // Ñòàðûå bool (íàïðèìåð *_Started) êîíâåðòèðóåì â Active îäèí ðàç.
    public static void SyncFromArticy() {
        _mutePush = true;
        try {
            ProcessGVSet(RQUE, isTemp: true);
            ProcessGVSet(NQUE, isTemp: false);
        } finally {
            _mutePush = false;
        }
    }

    private static void ProcessGVSet(object gv, bool isTemp) {
        try {
            var props = gv.GetType().GetProperties();

            foreach (var p in props) {
                if (p.PropertyType == typeof(int)) {
                    var key = p.Name;
                    int val = (int)p.GetValue(gv);

                    if (key.EndsWith("_State")) {
                        var questName = key.Substring(0, key.Length - "_State".Length);
                        var q = Ensure(questName, isTemp);
                        q.State = (QuestState)val;
                        EnsureStageCapacity(q, q.Stage);
                        EnsureResultCapacity(q, q.Result);
                        RaiseQuestChanged(q);
                    } else if (key.EndsWith("_Stage")) {
                        var questName = key.Substring(0, key.Length - "_Stage".Length);
                        var q = Ensure(questName, isTemp);
                        q.Stage = val;
                        if (q.State == QuestState.NotStarted && val > 0) q.State = QuestState.Active;
                        EnsureStageCapacity(q, val);
                        EnsureResultCapacity(q, q.Result);
                        RaiseQuestChanged(q);
                    } else if (key.EndsWith("_Result")) {
                        var questName = key.Substring(0, key.Length - "_Result".Length);
                        var q = Ensure(questName, isTemp);
                        q.Result = val;
                        EnsureResultCapacity(q, val);
                        EnsureStageCapacity(q, q.Stage);
                        RaiseQuestChanged(q);
                    } else if (key.Contains("_Obj_")) {
                        var parts = key.Split(new[] { "_Obj_" }, StringSplitOptions.None);
                        if (parts.Length == 2) {
                            var questName = parts[0];
                            var objId = parts[1];
                            var q = Ensure(questName, isTemp);
                            EnsureObjective(q, objId, (QuestState)val);
                            RaiseQuestChanged(q);
                        }
                    }
                } else if (p.PropertyType == typeof(bool)) {
                    // Ëåãàñè: *_Started == true -> àêòèâèðóåì êâåñò
                    bool started = (bool)p.GetValue(gv);
                    if (started && p.Name.EndsWith("_Started")) {
                        string questName = p.Name.Substring(0, p.Name.Length - "_Started".Length);
                        var q = Ensure(questName, isTemp);
                        if (q.State == QuestState.NotStarted) {
                            q.State = QuestState.Active;
                            if (q.Stage == 0) q.Stage = 1;
                            RaiseQuestChanged(q);
                        }
                    }
                }
            }
        } catch (Exception e) {
            Debug.LogWarning($"QuestManager.ProcessGVSet({(isTemp ? "RQUE" : "NQUE")}): {e.Message}");
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
