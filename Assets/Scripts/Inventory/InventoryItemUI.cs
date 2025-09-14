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
    private const string DefaultImagePath = "Images/Black";

    /// <summary>Initializes the item UI with a specific inventory item.</summary>
    public void Initialize(Item item, InventoryUI inventoryUI)
    {
        _item = item;
        _inventoryUI = inventoryUI;
        var text = GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = item.ItemCount.ToString();

        var image = GetComponent<Image>();
        if (image != null)
        {
            Sprite sprite = null;
            if (!string.IsNullOrEmpty(item.ImagePath))
                sprite = Resources.Load<Sprite>(item.ImagePath);
            if (sprite == null)
                sprite = Resources.Load<Sprite>(DefaultImagePath);
            image.sprite = sprite;
        }

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
