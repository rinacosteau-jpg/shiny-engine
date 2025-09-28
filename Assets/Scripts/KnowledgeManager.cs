using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class KnowledgeManager
{
    public class Knowledge
    {
        public string Name;
    }

    private static readonly Dictionary<string, Knowledge> knowledges = new();

    public static event Action KnowledgeChanged;

    public static void AddKnowledge(string name)
    {
        knowledges[name] = new Knowledge { Name = name };
        NotifyKnowledgeChanged();
    }

    public static string DisplayKnowledges()
    {
        var sb = new StringBuilder();
        foreach (var knowledge in GetAllKnowledges())
            sb.AppendLine($"{knowledge.Name} | ");
        return sb.ToString();
    }

    public static bool RemoveKnowledge(string name)
    {
        bool removed = knowledges.Remove(name);
        if (removed)
            NotifyKnowledgeChanged();
        return removed;
    }

    public static void ResetKnowledges()
    {
        if (knowledges.Count == 0)
            return;

        knowledges.Clear();
        NotifyKnowledgeChanged();
    }

    //
    public static bool HasKnowledge(string name) => knowledges.ContainsKey(name);

    public static IReadOnlyList<Knowledge> GetAllKnowledges() => knowledges
        .Values
        .OrderBy(k => k.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static void NotifyKnowledgeChanged() => KnowledgeChanged?.Invoke();
}
