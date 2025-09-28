using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;

public static class KnowledgeManager
{
    public class Knowledge
    {
        public string Name;
    }

    private static readonly Dictionary<string, Knowledge> knowledges = new();
    private static readonly Dictionary<CombinationKey, string> combinationResults = new()
    {
        { CombinationKey.Create("testA", "testB"), "testD" },
        { CombinationKey.Create("testD", "testC"), "testE" }
    };

    public static event Action KnowledgeChanged;

    public static void AddKnowledge(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        knowledges[name] = new Knowledge { Name = name };
        SetArticyKnowledgeState(name, true);
        NotifyKnowledgeChanged();
    }

    public static bool TryCombineKnowledge(string firstKnowledge, string secondKnowledge, out string resultKnowledge)
    {
        resultKnowledge = null;

        if (string.IsNullOrWhiteSpace(firstKnowledge) || string.IsNullOrWhiteSpace(secondKnowledge))
            return false;

        var key = CombinationKey.Create(firstKnowledge, secondKnowledge);
        if (!combinationResults.TryGetValue(key, out var targetKnowledge))
            return false;

        if (!HasKnowledge(firstKnowledge) || !HasKnowledge(secondKnowledge))
            return false;

        resultKnowledge = targetKnowledge;
        AddKnowledge(targetKnowledge);
        return true;
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

    private static void SetArticyKnowledgeState(string knowledgeName, bool value)
    {
        var globals = ArticyGlobalVariables.Default;
        if (globals == null)
            return;

        var knowledgeContainer = globals.NKNW;
        if (knowledgeContainer == null)
            return;

        var property = typeof(NKNW).GetProperty(knowledgeName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property == null || property.PropertyType != typeof(bool) || property.GetIndexParameters().Length != 0 || !property.CanWrite)
            return;

        property.SetValue(knowledgeContainer, value);
    }

    private readonly struct CombinationKey : IEquatable<CombinationKey>
    {
        private readonly string first;
        private readonly string second;

        private CombinationKey(string first, string second)
        {
            this.first = first;
            this.second = second;
        }

        public static CombinationKey Create(string firstKnowledge, string secondKnowledge)
        {
            string normalizedFirst = Normalize(firstKnowledge);
            string normalizedSecond = Normalize(secondKnowledge);

            return string.Compare(normalizedFirst, normalizedSecond, StringComparison.Ordinal) <= 0
                ? new CombinationKey(normalizedFirst, normalizedSecond)
                : new CombinationKey(normalizedSecond, normalizedFirst);
        }

        public bool Equals(CombinationKey other) => first == other.first && second == other.second;

        public override bool Equals(object obj) => obj is CombinationKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(first, second);

        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }
}
