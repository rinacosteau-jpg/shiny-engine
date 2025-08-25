// QuestManager.cs
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon; // замени на свой, если Articy сгенерил другой namespace
using Articy.World_Of_Red_Moon.GlobalVariables;

public enum QuestState { NotStarted = 0, Active = 1, Completed = 2, Failed = 3 }

public static class QuestManager {
    // ======== Модель ========
    public class Objective {
        public string Id;                 // напр. "A", "B", "C"
        public QuestState State;
        public bool Optional;
    }

    public class Quest {
        public string Name;               // это и есть ID квеста
        public QuestState State;
        public int Stage;                 // линейный прогресс/чекпоинт
        public bool IsTemporary;          // RQUE = true, NQUE = false
        public readonly Dictionary<string, Objective> Objectives = new();
    }

    // ======== События ========
    public static event Action<Quest> OnQuestChanged;

    // ======== Хранилище ========
    private static readonly Dictionary<string, Quest> quests = new();

    // Глушилка для PushToArticy во время Sync
    private static bool _mutePush;

    // Быстрые ссылки на наборы глобалок Articy
    private static object RQUE => ArticyGlobalVariables.Default.RQUE;
    private static object NQUE => ArticyGlobalVariables.Default.NQUE;

    // ======== Хелперы ========
    // isTemp: null — не трогаем; true — временный (RQUE); false — постоянный (NQUE).
    private static Quest Ensure(string name, bool? isTemp = null) {
        if (!quests.TryGetValue(name, out var q)) {
            q = new Quest {
                Name = name,
                IsTemporary = isTemp ?? false, // по умолчанию считаем персистентным
                State = QuestState.NotStarted,
                Stage = 0
            };
            quests[name] = q;
        } else if (isTemp.HasValue) {
            // Если сведения пришли из NQUE — даём приоритет "не временным".
            if (isTemp.Value == false && q.IsTemporary)
                q.IsTemporary = false;
            // Если уже non-temp, не переводим обратно в temp.
            // Если был temp и пришло temp — оставляем temp.
        }
        return q;
    }

    private static void EnsureObjective(Quest q, string objId, QuestState state, bool optional = false) {
        if (!q.Objectives.TryGetValue(objId, out var obj))
            q.Objectives[objId] = obj = new Objective { Id = objId, Optional = optional, State = QuestState.NotStarted };
        obj.State = state;
    }

    private static void RaiseQuestChanged(Quest q) => OnQuestChanged?.Invoke(q);

    // ======== Публичный API ========
    public static bool Has(string name) => quests.ContainsKey(name);
    public static Quest Get(string name) => quests.TryGetValue(name, out var q) ? q : null;
    public static bool IsActive(string name) => quests.TryGetValue(name, out var q) && q.State == QuestState.Active;
    public static bool IsCompleted(string name) => quests.TryGetValue(name, out var q) && q.State == QuestState.Completed;

    /// <summary>Запусти постоянный квест: Start(name, isTemp:false) или временный: Start(name, isTemp:true)</summary>
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
        var q = Start(name); // тип (temp/non-temp) уже должен быть определён ранее или по умолчанию non-temp
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
            // 1) обнуляем зеркальные переменные именно в RQUE (только по префиксу квеста)
            ClearQuestInArticy(name, RQUE);

            // 2) убираем из локального реестра
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

    // ======== Синхронизация с Articy ========
    // Пуш в Articy по соглашению имён:
    // <Quest>_State (int), <Quest>_Stage (int), <Quest>_Obj_<Id> (int)
    // Дополнительно: <Quest>_ObjectivesCompleted (int) если свойство существует.
    private static void PushToArticy(Quest q) {
        if (_mutePush) return; // не устраиваем пинг-понг во время Sync

        try {
            var gv = q.IsTemporary ? RQUE : NQUE;

            SetInt(gv, $"{q.Name}_State", (int)q.State);
            SetInt(gv, $"{q.Name}_Stage", q.Stage);

            int completedObjectives = 0;
            foreach (var obj in q.Objectives.Values) {
                SetInt(gv, $"{q.Name}_Obj_{obj.Id}", (int)obj.State);
                if (obj.State == QuestState.Completed) completedObjectives++;
            }

            // Общее имя счётчика (если есть — поставим):
            SetIntIfExists(gv, $"{q.Name}_ObjectivesCompleted", completedObjectives);
            // Совместимость с «advertise_TalkedCount», если такая переменная есть:
            SetIntIfExists(gv, $"{q.Name}_TalkedCount", completedObjectives);
        } catch (Exception e) {
            Debug.LogWarning($"QuestManager.PushToArticy: {e.Message}");
        }
    }

    // Обнулить все int/bool переменные квеста по префиксу "<questName>_" внутри указанного набора (RQUE или NQUE).
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

    // Подтяжка состояния из Articy в локальное хранилище.
    // Теперь читаем и RQUE (isTemp=true), и NQUE (isTemp=false).
    // Старые bool (например *_Started) конвертируем в Active один раз.
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
                    // Легаси: *_Started == true -> активируем квест
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

    // ======== Рефлексия-помощники ========
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
