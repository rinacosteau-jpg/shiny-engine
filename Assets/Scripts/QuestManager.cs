using System.Collections.Generic;

public class QuestManager {
    private class Quest {
        public string Id;
        public bool IsCompleted;
        public bool IsTemporary;
    }

    private readonly Dictionary<string, Quest> quests = new();

    public void AddQuest(string id, bool isTemporary = false) {
        quests[id] = new Quest { Id = id, IsTemporary = isTemporary, IsCompleted = false };
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

