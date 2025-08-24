using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class KnowledgeManager {
    public class Knowledge {
        public string Name;
    }

    private static readonly Dictionary<string, Knowledge> knowledges = new();

    public static void AddKnowledge(string name) {
        knowledges[name] = new Knowledge { Name = name };
        Debug.Log($"[Knowledge] Received: {name}");
    }

    public static string DisplayKnowledges() {
        var sb = new StringBuilder();
        foreach (var q in knowledges.Values)
            sb.AppendLine($"{q.Name} | ");
        return sb.ToString();
    }

    public static bool RemoveKnowledge(string name) => knowledges.Remove(name);

    public static void ResetKnowledges() => knowledges.Clear();

    public static bool HasKnowledge(string name) => knowledges.ContainsKey(name);
}
