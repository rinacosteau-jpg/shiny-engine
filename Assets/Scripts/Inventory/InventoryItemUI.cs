using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single item entry in the inventory UI.
/// </summary>
public class InventoryItemUI : MonoBehaviour
{
    private Item _item;
    private InventoryUI _inventoryUI;

    /// <summary>Initializes the item UI with a specific inventory item.</summary>
    public void Initialize(Item item, InventoryUI inventoryUI)
    {
        _item = item;
        _inventoryUI = inventoryUI;
        var text = GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = $"{item.TechnicalName} ({item.ItemCount})";

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (_item != null)
        {
            Debug.Log($"Clicked inventory item {_item.TechnicalName}");
            _inventoryUI?.DisplayItem(_item);
        }
    }
}
