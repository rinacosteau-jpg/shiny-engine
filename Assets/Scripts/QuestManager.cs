// QuestManager.cs
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon; // ������ �� ����, ���� Articy �������� ������ namespace
using Articy.World_Of_Red_Moon.GlobalVariables;

public enum QuestState { NotStarted = 0, Active = 1, Completed = 2, Failed = 3 }

public static class QuestManager {
    // ======== ������ ========
    public class Objective {
        public string Id;                 // ����. "A", "B", "C"
        public QuestState State;
        public bool Optional;
    }

    public class Quest {
        public string Name;               // ��� � ���� ID ������
        public QuestState State;
        public int Stage;                 // �������� ��������/��������
        public bool IsTemporary;          // ����� ���������� �� ������ �����
        public readonly Dictionary<string, Objective> Objectives = new();
    }

    // ======== ������� ========
    public static event Action<Quest> OnQuestChanged;

    // ======== ��������� ========
    private static readonly Dictionary<string, Quest> quests = new();

    // �������� ��� PushToArticy �� ����� Sync
    private static bool _mutePush;

    // ======== ������� ========
    private static Quest Ensure(string name, bool isTemp = false) {
        if (!quests.TryGetValue(name, out var q)) {
            q = new Quest { Name = name, IsTemporary = isTemp, State = QuestState.NotStarted, Stage = 0 };
            quests[name] = q;
        }
        return q;
    }

    private static void EnsureObjective(Quest q, string objId, QuestState state, bool optional = false) {
        if (!q.Objectives.TryGetValue(objId, out var obj))
            q.Objectives[objId] = obj = new Objective { Id = objId, Optional = optional, State = QuestState.NotStarted };
        obj.State = state;
    }

    private static void RaiseQuestChanged(Quest q) => OnQuestChanged?.Invoke(q);

    // ======== ��������� API ========
    public static bool Has(string name) => quests.ContainsKey(name);
    public static Quest Get(string name) => quests.TryGetValue(name, out var q) ? q : null;
    public static bool IsActive(string name) => quests.TryGetValue(name, out var q) && q.State == QuestState.Active;
    public static bool IsCompleted(string name) => quests.TryGetValue(name, out var q) && q.State == QuestState.Completed;

    public static Quest Start(string name, bool isTemp = false) {
        var q = Ensure(name, isTemp);
        q.State = QuestState.Active;
        if (q.Stage == 0) q.Stage = 1;
        PushToArticy(q);
        RaiseQuestChanged(q);
        return q;
    }

    public static void Fail(string name) {
        if (!quests.TryGetValue(name, out var q)) return;
        q.State = QuestState.Failed;
        PushToArticy(q);
        RaiseQuestChanged(q);
    }

    public static void Complete(string name) {
        if (!quests.TryGetValue(name, out var q)) return;
        q.State = QuestState.Completed;
        PushToArticy(q);
        RaiseQuestChanged(q);
    }

    public static void SetStage(string name, int stage) {
        var q = Start(name);
        q.Stage = stage;
        if (q.State == QuestState.NotStarted) q.State = QuestState.Active;
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
        foreach (var kv in quests) if (kv.Value.IsTemporary) toRemove.Add(kv.Key);
        foreach (var k in toRemove) quests.Remove(k);
    }

    public static void ResetAll() => quests.Clear();

    public static string DisplayQuests() {
        var sb = new StringBuilder();
        foreach (var q in quests.Values) {
            sb.Append($"{q.Name} [State:{q.State}, Stage:{q.Stage}]");
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

    // ======== ������������� � Articy ========
    // ��� � Articy �� ���������� ���:
    // <Quest>_State (int), <Quest>_Stage (int), <Quest>_Obj_<Id> (int)
    // �������������: <Quest>_ObjectivesCompleted (int) ���� �������� ����������.
    private static void PushToArticy(Quest q) {
        if (_mutePush) return; // �� ���������� ����-���� �� ����� Sync

        try {
            var rque = ArticyGlobalVariables.Default.RQUE;

            SetInt(rque, $"{q.Name}_State", (int)q.State);
            SetInt(rque, $"{q.Name}_Stage", q.Stage);

            int completedObjectives = 0;
            foreach (var obj in q.Objectives.Values) {
                SetInt(rque, $"{q.Name}_Obj_{obj.Id}", (int)obj.State);
                if (obj.State == QuestState.Completed) completedObjectives++;
            }

            // ����� ��� ��������:
            SetIntIfExists(rque, $"{q.Name}_ObjectivesCompleted", completedObjectives);
            // ������������� � �advertise_TalkedCount�, ���� ����� ���������� ����:
            SetIntIfExists(rque, $"{q.Name}_TalkedCount", completedObjectives);
        } catch (Exception e) {
            Debug.LogWarning($"QuestManager.PushToArticy: {e.Message}");
        }
    }

    // �������� ��������� �� Articy � ��������� ���������.
    // ��������� ��: *_State, *_Stage, *_Obj_*
    // ������ bool (�������� *_Started) ������������ � Active ���� ���.
    public static void SyncFromArticy() {
        _mutePush = true;
        try {
            var rque = ArticyGlobalVariables.Default.RQUE;
            var props = rque.GetType().GetProperties();

            foreach (var p in props) {
                if (p.PropertyType == typeof(int)) {
                    var key = p.Name;
                    int val = (int)p.GetValue(rque);

                    if (key.EndsWith("_State")) {
                        var questName = key.Substring(0, key.Length - "_State".Length);
                        var q = Ensure(questName);
                        q.State = (QuestState)val;
                        RaiseQuestChanged(q);
                    } else if (key.EndsWith("_Stage")) {
                        var questName = key.Substring(0, key.Length - "_Stage".Length);
                        var q = Ensure(questName);
                        q.Stage = val;
                        if (q.State == QuestState.NotStarted && val > 0) q.State = QuestState.Active;
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
                    // ������: *_Started == true -> ���������� �����
                    bool started = (bool)p.GetValue(rque);
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
        } finally {
            _mutePush = false;
        }
    }

    // ======== ���������-��������� ========
    private static void SetInt(object rque, string propName, int value) {
        var p = rque.GetType().GetProperty(propName);
        if (p != null && p.PropertyType == typeof(int)) p.SetValue(rque, value);
    }

    private static void SetIntIfExists(object rque, string propName, int value) {
        var p = rque.GetType().GetProperty(propName);
        if (p != null && p.PropertyType == typeof(int)) p.SetValue(rque, value);
    }

    private static void SetBool(object rque, string propName, bool value) {
        var p = rque.GetType().GetProperty(propName);
        if (p != null && p.PropertyType == typeof(bool)) p.SetValue(rque, value);
    }

    // ======================================================================
    //                    ����� "�������" (advertise)
    // ======================================================================
    public static class Advertise {
        public const string Name = "advertise";
        private const string A = "A";
        private const string B = "B";
        private const string C = "C";

        // 0 = ��� ����������, 1 = ������ A, 2 = ������ B, 3 = ������ C, 4 = ��� ��� ���
        public static int ResultCode { get; private set; }

        public static void Start(bool isTemp = true) => QuestManager.Start(Name, isTemp);
        public static void Fail() => QuestManager.Fail(Name);

        // �������� �� �������� � ����������� NPC, ����� �������������� ����������� �������:
        public static void TalkedToA() => QuestManager.SetObjectiveState(Name, A, QuestState.Completed);
        public static void TalkedToB() => QuestManager.SetObjectiveState(Name, B, QuestState.Completed);
        public static void TalkedToC() => QuestManager.SetObjectiveState(Name, C, QuestState.Completed);

        // ����� ������ � �����������; ���������� true ���� ��������
        public static bool TryCompleteAndReward() {
            if (!QuestManager.IsActive(Name)) return false;

            bool a = QuestManager.GetObjectiveState(Name, A) == QuestState.Completed;
            bool b = QuestManager.GetObjectiveState(Name, B) == QuestState.Completed;
            bool c = QuestManager.GetObjectiveState(Name, C) == QuestState.Completed;

            int count = (a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0);
            if (count == 0) return false; // ������ �� ������ � ����� ������� ��������

            // ������� ����������
            if (count >= 2) ResultCode = 4;
            else if (a) ResultCode = 1;
            else if (b) ResultCode = 2;
            else ResultCode = 3;

            QuestManager.Complete(Name);

            // ������� � ���������� ���������� ��� ������� � ������� �����
            try {
                var rque = ArticyGlobalVariables.Default.RQUE;
                SetInt(rque, $"{Name}_Result", ResultCode);     // 1/2/3/4
                SetBool(rque, "ratCanDistractGuard", true);     // ������-�������
            } catch (Exception e) {
                Debug.LogWarning($"Advertise.TryCompleteAndReward push error: {e.Message}");
            }

            return true;
        }
    }
}
