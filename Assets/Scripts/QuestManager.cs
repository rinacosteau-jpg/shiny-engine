using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public class QuestManager {

    public static QuestManager Instance { get; private set; }

    void Awake() {
        Instance = this;
    }

    public class Quest {
        public string Name;
        public bool IsCompleted;
        public bool IsTemporary;
    }

    public static Dictionary<string, Quest> quests = new();

    public void AddQuest(string name, bool isTemporary) {
        Instance = this;
        quests[name] = new Quest { Name = name, IsTemporary = isTemporary, IsCompleted = false };
    }

    public string displayQuests () {

        Instance = this;

        string list = " ";

        foreach (var kvp in quests) {
            
                list += kvp.Value.Name + " Is Temporary:" + kvp.Value.IsTemporary as string + " Is Completed: " + kvp.Value.IsCompleted as string;
        }

        return list;
    }

    public bool RemoveQuest(string id) => quests.Remove(id);

    public void CompleteQuest(string id) {
        if (quests.TryGetValue(id, out var quest)) {
            quest.IsCompleted = true;
        }
    }

    public void ResetQuests() => quests.Clear();

    public void ResetTemporary() {
        var toRemove = new List<string>();
        foreach (var kvp in quests) {
            if (kvp.Value.IsTemporary) {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove) {
            quests.Remove(key);
        }
    }
}

