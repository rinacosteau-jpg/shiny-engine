using System;
using System.Collections.Generic;
using System.Linq;

public static class InventoryStorage {
    private static readonly Dictionary<string, HashSet<string>> _items =
        new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

    // Tracks which item types have been identified by the player.
    private static readonly HashSet<string> _identifiedItems =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static event Action<string, int> OnItemCountChanged;
    public static event Action OnInventoryCleared;

    private static readonly HashSet<string> UniqueItemIds =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ItemIds.InventoryArtefact,
            ItemIds.HarmonicRow,
            ItemIds.SonoceramicShard,
            ItemIds.SonusGuideTube,
            ItemIds.ReceiptWhisperer,
            ItemIds.WaxStoppers,
            ItemIds.MaintScrollHum,
            ItemIds.VentFiddle,
            ItemIds.EarPressureReports
        };

    static InventoryStorage() {
        ArticyInventorySync.PushAllCountsToArticy();
    }

    public static void Add(string technicalName, int count = 1, string instanceId = null) {
        if (string.IsNullOrEmpty(technicalName)) return;

        if (!_items.TryGetValue(technicalName, out var set)) {
            set = new HashSet<string>();
            _items[technicalName] = set;
        }

        if (UniqueItemIds.Contains(technicalName)) {
            set.Clear();
            set.Add(instanceId ?? Guid.NewGuid().ToString());
        } else if (instanceId != null) {
            set.Add(instanceId);
        } else {
            for (int i = 0; i < count; i++)
                set.Add(Guid.NewGuid().ToString());
        }

        ArticyInventorySync.PushAllCountsToArticy();
        Notify(technicalName);
        if (ArticyClueSync.TryGetClueValue(technicalName, out _))
            ArticyClueSync.PushToArticy(technicalName, true);
    }

    public static void Remove(string technicalName, int count = 1, string instanceId = null) {
        if (!_items.TryGetValue(technicalName, out var set)) return;

        if (instanceId != null) {
            set.Remove(instanceId);
        } else {
            while (count-- > 0 && set.Count > 0) {
                var id = set.First();
                set.Remove(id);
            }
        }

        if (set.Count == 0) {
            _items.Remove(technicalName);
            _identifiedItems.Remove(technicalName);
        }

        ArticyInventorySync.PushAllCountsToArticy();
        Notify(technicalName);
        if (ArticyClueSync.TryGetClueValue(technicalName, out _))
            ArticyClueSync.PushToArticy(technicalName, Contains(technicalName));
    }

    public static void Clear() {
        _items.Clear();
        _identifiedItems.Clear();
        ArticyInventorySync.PushAllCountsToArticy();
        foreach (var clue in ArticyClueSync.ClueValues.Keys)
            ArticyClueSync.PushToArticy(clue, false);
        OnInventoryCleared?.Invoke();
    }

    /// <summary>Marks all currently stored item types as identified.</summary>
    public static void IdentifyAll() {
        foreach (var id in _items.Keys)
            _identifiedItems.Add(id);
    }

    public static IReadOnlyList<Item> Items =>
        _items.Select(kvp => {
            int value;
            bool isClue = ArticyClueSync.TryGetClueValue(kvp.Key, out value);
            bool isIdentified = _identifiedItems.Contains(kvp.Key);
            return new Item(kvp.Key, kvp.Value.Count, isClue: isClue, isIdentified: isIdentified, clueScore: value);
        }).ToList();

    public static int GetCount(string technicalName) =>
        _items.TryGetValue(technicalName, out var set) ? set.Count : 0;

    public static bool Contains(string technicalName) => GetCount(technicalName) > 0;

    public static bool ContainsInstance(string technicalName, string instanceId) =>
        _items.TryGetValue(technicalName, out var set) && set.Contains(instanceId);

    private static void Notify(string id) =>
        OnItemCountChanged?.Invoke(id, GetCount(id));
}
