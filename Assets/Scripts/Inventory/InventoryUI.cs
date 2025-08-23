using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controls the inventory interface and populates it based on items in <see cref="InventoryStorage"/>.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private Transform itemsParent;

    private readonly List<GameObject> _spawnedItems = new List<GameObject>();

    private void Start()
    {
        Hide();
        Refresh();
    }

    /// <summary>Shows the inventory interface.</summary>
    public void Show()
    {
        Refresh();
        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);
    }

    /// <summary>Hides the inventory interface.</summary>
    public void Hide()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    /// <summary>Toggles the visibility of the inventory interface.</summary>
    public void Toggle()
    {
        if (inventoryPanel == null) return;

        if (inventoryPanel.activeSelf)
            Hide();
        else
            Show();
    }

    /// <summary>Rebuilds the list of UI items based on <see cref="InventoryStorage.Items"/>.</summary>
    public void Refresh()
    {
        foreach (var go in _spawnedItems)
        {
            if (go != null)
                Destroy(go);
        }
        _spawnedItems.Clear();

        var items = InventoryStorage.Items;
        for (int i = 0; i < items.Count; i++)
        {
            var obj = Instantiate(itemButtonPrefab, itemsParent);
            _spawnedItems.Add(obj);
            var itemUi = obj.GetComponent<InventoryItemUI>();
            if (itemUi == null)
                itemUi = obj.AddComponent<InventoryItemUI>();
            itemUi.Initialize(i);
        }
    }
}
