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
        public bool IsTemporary;          // RQUE = true, NQUE = false
        public readonly Dictionary<string, Objective> Objectives = new();
    }

    // ======== ������� ========
    public static event Action<Quest> OnQuestChanged;

    // ======== ��������� ========
    private static readonly Dictionary<string, Quest> quests = new();

    // �������� ��� PushToArticy �� ����� Sync
    private static bool _mutePush;

    // ������� ������ �� ������ �������� Articy
    private static object RQUE => ArticyGlobalVariables.Default.RQUE;
    private static object NQUE => ArticyGlobalVariables.Default.NQUE;

    // ======== ������� ========
    // isTemp: null � �� �������; true � ��������� (RQUE); false � ���������� (NQUE).
    private static Quest Ensure(string name, bool? isTemp = null) {
        if (!quests.TryGetValue(name, out var q)) {
            q = new Quest {
                Name = name,
                IsTemporary = isTemp ?? false, // �� ��������� ������� �������������
                State = QuestState.NotStarted,
                Stage = 0
            };
            quests[name] = q;
        } else if (isTemp.HasValue) {
            // ���� �������� ������ �� NQUE � ��� ��������� "�� ���������".
            if (isTemp.Value == false && q.IsTemporary)
                q.IsTemporary = false;
            // ���� ��� non-temp, �� ��������� ������� � temp.
            // ���� ��� temp � ������ temp � ��������� temp.
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

    /// <summary>������� ���������� �����: Start(name, isTemp:false) ��� ���������: Start(name, isTemp:true)</summary>
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
        var q = Start(name); // ��� (temp/non-temp) ��� ������ ���� �������� ����� ��� �� ��������� non-temp
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
        foreach (var kv in quests)
            if (kv.Value.IsTemporary) toRemove.Add(kv.Key);

        foreach (var name in toRemove) {
            // 1) �������� ���������� ���������� ������ � RQUE (������ �� �������� ������)
            ClearQuestInArticy(name, RQUE);

            // 2) ������� �� ���������� �������
            quests.Remove(name);
        }
    }

    public static void ResetAll() => quests.Clear();

    public static string DisplayQuests() {
        var sb = new StringBuilder();
        foreach (var q in quests.Values) {
            sb.Append($"{q.Name} [State:{q.State}, Stage:{q.Stage}, Temp:{q.IsTemporary}]");
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
            var gv = q.IsTemporary ? RQUE : NQUE;

            SetInt(gv, $"{q.Name}_State", (int)q.State);
            SetInt(gv, $"{q.Name}_Stage", q.Stage);

            int completedObjectives = 0;
            foreach (var obj in q.Objectives.Values) {
                SetInt(gv, $"{q.Name}_Obj_{obj.Id}", (int)obj.State);
                if (obj.State == QuestState.Completed) completedObjectives++;
            }

            // ����� ��� �������� (���� ���� � ��������):
            SetIntIfExists(gv, $"{q.Name}_ObjectivesCompleted", completedObjectives);
            // ������������� � �advertise_TalkedCount�, ���� ����� ���������� ����:
            SetIntIfExists(gv, $"{q.Name}_TalkedCount", completedObjectives);
        } catch (Exception e) {
            Debug.LogWarning($"QuestManager.PushToArticy: {e.Message}");
        }
    }

    // �������� ��� int/bool ���������� ������ �� �������� "<questName>_" ������ ���������� ������ (RQUE ��� NQUE).
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

    // �������� ��������� �� Articy � ��������� ���������.
    // ������ ������ � RQUE (isTemp=true), � NQUE (isTemp=false).
    // ������ bool (�������� *_Started) ������������ � Active ���� ���.
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
                        RaiseQuestChanged(q);
                    } else if (key.EndsWith("_Stage")) {
                        var questName = key.Substring(0, key.Length - "_Stage".Length);
                        var q = Ensure(questName, isTemp);
                        q.Stage = val;
                        if (q.State == QuestState.NotStarted && val > 0) q.State = QuestState.Active;
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
                    // ������: *_Started == true -> ���������� �����
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

    // ======== ���������-��������� ========
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
