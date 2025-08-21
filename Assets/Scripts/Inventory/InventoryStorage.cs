using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores references to the objects currently held by the player.
/// Provides methods to add, remove and clear stored objects.
/// </summary>
public static class InventoryStorage
{
    // Internal list that holds the player's objects.
    private static readonly List<GameObject> _items = new List<GameObject>();

    /// <summary>
    /// Adds a new object to the inventory if it is not null and not already stored.
    /// </summary>
    /// <param name="item">The object to store.</param>
    public static void Add(GameObject item)
    {
        if (item != null && !_items.Contains(item))
        {
            _items.Add(item);
        }
    }

    /// <summary>
    /// Removes an object from the inventory if it exists.
    /// </summary>
    /// <param name="item">The object to remove.</param>
    public static void Remove(GameObject item)
    {
        if (item != null)
        {
            _items.Remove(item);
        }
    }

    /// <summary>
    /// Clears all objects from the inventory.
    /// </summary>
    public static void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Returns a read-only list of the items currently stored.
    /// </summary>
    public static IReadOnlyList<GameObject> Items => _items.AsReadOnly();
}
