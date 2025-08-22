using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single item entry in the inventory UI.
/// </summary>
public class InventoryItemUI : MonoBehaviour
{
    private int _id;

    /// <summary>Initializes the item UI with a specific identifier.</summary>
    public void Initialize(int id)
    {
        _id = id;
        var text = GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = id.ToString();

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        Debug.Log($"Clicked inventory item with id {_id}");
    }
}
