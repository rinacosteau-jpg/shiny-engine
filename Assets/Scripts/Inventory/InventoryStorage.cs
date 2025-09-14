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
            ItemIds.Gun,
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
        ArticyClueSync.PushTotalScoreToArticy();
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
        ArticyClueSync.PushTotalScoreToArticy();
    }

    public static void Clear(bool removeClues = true) {
        if (removeClues) {
            _items.Clear();
            _identifiedItems.Clear();
            ArticyInventorySync.PushAllCountsToArticy();
            foreach (var clue in ArticyClueSync.ClueValues.Keys)
                ArticyClueSync.PushToArticy(clue, false);
        } else {
            var keysToRemove = _items.Keys.Where(k => !ArticyClueSync.TryGetClueValue(k, out _)).ToList();
            foreach (var key in keysToRemove) {
                _items.Remove(key);
                _identifiedItems.Remove(key);
            }
            ArticyInventorySync.PushAllCountsToArticy();
            foreach (var clue in ArticyClueSync.ClueValues.Keys)
                ArticyClueSync.PushToArticy(clue, Contains(clue));
        }
        ArticyClueSync.PushTotalScoreToArticy();
        OnInventoryCleared?.Invoke();
    }

    /// <summary>Marks all currently stored item types as identified.</summary>
    public static void IdentifyAll() {
        foreach (var id in _items.Keys)
            _identifiedItems.Add(id);
        ArticyClueSync.PushTotalScoreToArticy();
    }

    public static IReadOnlyList<Item> Items =>
        _items.Select(kvp => {
            int value;
            bool isClue = ArticyClueSync.TryGetClueValue(kvp.Key, out value);
            bool isIdentified = _identifiedItems.Contains(kvp.Key);
            return new Item(kvp.Key, kvp.Value.Count, isClue: isClue, isIdentified: isIdentified, clueScore: value)
            {
                Description = ItemIds.Descriptions.TryGetValue(kvp.Key, out var desc) ? desc : string.Empty,
                ImagePath = ItemIds.ImagePaths.TryGetValue(kvp.Key, out var img) ? img : string.Empty
            };
        }).ToList();

    /// <summary>
    /// Calculates the total clue score based on items currently in the inventory.
    /// Unidentified clues are counted as 0.5 points.
    /// </summary>
    public static float ClueTotalScore {
        get {
            float total = 0f;
            foreach (var kvp in _items) {
                if (ArticyClueSync.TryGetClueValue(kvp.Key, out var value)) {
                    bool identified = _identifiedItems.Contains(kvp.Key);
                    total += identified ? value : 0.5f;
                }
            }
            return total;
        }
    }

    public static int GetCount(string technicalName) =>
        _items.TryGetValue(technicalName, out var set) ? set.Count : 0;

    public static bool Contains(string technicalName) => GetCount(technicalName) > 0;

    public static bool ContainsInstance(string technicalName, string instanceId) =>
        _items.TryGetValue(technicalName, out var set) && set.Contains(instanceId);

    private static void Notify(string id) =>
        OnItemCountChanged?.Invoke(id, GetCount(id));
}
