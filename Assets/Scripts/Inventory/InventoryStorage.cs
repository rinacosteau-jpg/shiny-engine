using System;
using System.Collections.Generic;
using System.Linq;

public static class InventoryStorage {
    private static readonly List<Item> _items = new List<Item>();

    // события
    public static event Action<string, int> OnItemCountChanged;
    public static event Action OnInventoryCleared;

    // какие предметы считаем уникальными (count ограничен 1)
    private static readonly HashSet<string> UniqueItemIds =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ItemIds.InventoryArtefact
        };

    static InventoryStorage() {
        // Удали сиды в проде; оставлю пустым чтобы не мешало логике флагов
        //_items.Add(new Item("TestItem_0"));
        ArticyInventorySync.PushAllCountsToArticy();
    }

    public static void Add(Item item) {
        if (item == null || string.IsNullOrEmpty(item.TechnicalName)) return;

        var existing = _items.Find(i => i.TechnicalName.Equals(item.TechnicalName, StringComparison.OrdinalIgnoreCase));
        if (existing != null) {
            if (UniqueItemIds.Contains(existing.TechnicalName))
                existing.ItemCount = 1; // уникальный — максимум 1
            else
                existing.ItemCount += item.ItemCount;
        } else {
            if (UniqueItemIds.Contains(item.TechnicalName) && item.ItemCount > 1)
                item = new Item(item.TechnicalName, 1);
            _items.Add(item);
        }

        ArticyInventorySync.PushAllCountsToArticy();
        Notify(item.TechnicalName);
    }

    public static void Remove(string technicalName, int count = 1) {
        var existing = _items.Find(i => i.TechnicalName.Equals(technicalName, StringComparison.OrdinalIgnoreCase));
        if (existing == null) return;

        existing.ItemCount -= count;
        if (existing.ItemCount <= 0) _items.Remove(existing);

        ArticyInventorySync.PushAllCountsToArticy();
        Notify(technicalName);
    }

    public static void Clear() {
        _items.Clear();
        ArticyInventorySync.PushAllCountsToArticy();
        OnInventoryCleared?.Invoke();
    }

    public static IReadOnlyList<Item> Items => _items.AsReadOnly();

    // утилиты
    public static int GetCount(string technicalName)
        => _items.FirstOrDefault(i => i.TechnicalName.Equals(technicalName, StringComparison.OrdinalIgnoreCase))?.ItemCount ?? 0;

    public static bool Contains(string technicalName) => GetCount(technicalName) > 0;

    private static void Notify(string id)
        => OnItemCountChanged?.Invoke(id, GetCount(id));
}
