using System;

/// <summary>
/// Represents an item stored in the inventory.
/// </summary>
public class Item
{
    /// <summary>Unique technical identifier for the item.</summary>
    public string TechnicalName { get; }

    /// <summary>Quantity of this item in the inventory.</summary>
    public int ItemCount { get; set; }

    public float ClueScore { get; set; }

    public bool IsClue { get; set; }
    public bool IsIdentified { get; set; }

    public string Description { get; set; }

    public string ImagePath { get; set; }


    /// <summary>Creates a new item with the given identifier and count.</summary>
    public Item(string technicalName, int itemCount = 1, bool isClue=false, bool isIdentified=false, float clueScore = 0)
    {
        if (string.IsNullOrEmpty(technicalName))
            throw new ArgumentException("technicalName cannot be null or empty", nameof(technicalName));
        TechnicalName = technicalName;
        ItemCount = itemCount;
        ClueScore = clueScore;
        IsClue = isClue;
        IsIdentified = isIdentified;
    }
}
