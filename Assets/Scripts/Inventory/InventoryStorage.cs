using System.Collections.Generic;

/// <summary>
/// Stores item entries currently held by the player.
/// Provides methods to add, remove and clear stored items.
/// </summary>
public static class InventoryStorage
{
    // Internal list that holds the player's items.
    private static readonly List<Item> _items = new List<Item>();

    // Static constructor to seed the inventory with test items.
    static InventoryStorage()
    {
        for (int i = 0; i < 5; i++)
        {
            _items.Add(new Item($"TestItem_{i}"));
        }
    }

    /// <summary>
    /// Adds a new item to the inventory. If an item with the same technical name
    /// already exists, increases its count instead of adding a new entry.
    /// </summary>
    /// <param name="item">The item to store.</param>
    public static void Add(Item item)
    {
        if (item == null || string.IsNullOrEmpty(item.TechnicalName))
            return;

        var existing = _items.Find(i => i.TechnicalName == item.TechnicalName);
        if (existing != null)
        {
            existing.ItemCount += item.ItemCount;
        }
        else
        {
            _items.Add(item);
        }
    }

    /// <summary>
    /// Removes a certain amount of an item from the inventory based on its technical name.
    /// If the count drops to zero or below, the item is removed entirely.
    /// </summary>
    /// <param name="technicalName">The identifier of the item to remove.</param>
    /// <param name="count">How many of the item to remove.</param>
    public static void Remove(string technicalName, int count = 1)
    {
        var existing = _items.Find(i => i.TechnicalName == technicalName);
        if (existing == null)
            return;

        existing.ItemCount -= count;
        if (existing.ItemCount <= 0)
            _items.Remove(existing);
    }

    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    public static void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Returns a read-only list of the items currently stored.
    /// </summary>
    public static IReadOnlyList<Item> Items => _items.AsReadOnly();
}
