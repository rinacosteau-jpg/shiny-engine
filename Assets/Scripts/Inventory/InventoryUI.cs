using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controls the inventory interface and populates it based on items in <see cref="InventoryStorage"/>.
/// </summary>
public class InventoryUI : MonoBehaviour, ILoopResettable
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private TMP_Text itemDescription;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private Image itemPicture;
    [SerializeField] private Image itemPictureSmall;
    [SerializeField] private Transform itemsParent;

    private readonly List<GameObject> _spawnedItems = new List<GameObject>();
    private const string DefaultImagePath = "Images/Black";

    private void Start()
    {
        Hide();
        Refresh();
    }

    /// <summary>Shows the inventory interface.</summary>
    public void Show()
    {
        Refresh();
        DisplayItem(null);
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
    public void Refresh() {
        // 1) подтянуть награды/штрафы из диалога
        ArticyInventorySync.ApplyItemDeltasFromArticy();
        ArticyClueSync.SyncFromArticy();

        // 2) дальше как было
        foreach (var go in _spawnedItems)
            if (go != null) Destroy(go);
        _spawnedItems.Clear();

        var items = InventoryStorage.Items;
        for (int i = 0; i < items.Count; i++) {
            var obj = Instantiate(itemButtonPrefab, itemsParent);
            _spawnedItems.Add(obj);
            var itemUi = obj.GetComponent<InventoryItemUI>() ?? obj.AddComponent<InventoryItemUI>();
            itemUi.Initialize(items[i], this);
            obj.SetActive(true);
        }
    }


    public void OnLoopReset()
    {
        Hide();
        Refresh();
    }

    public void DisplayItem(Item item)
    {
        if (itemName != null)
            itemName.text = item?.TechnicalName ?? string.Empty;
        if (itemDescription != null)
            itemDescription.text = item?.Description ?? string.Empty;

        Sprite sprite = null;
        if (item != null && !string.IsNullOrEmpty(item.ImagePath))
            sprite = Resources.Load<Sprite>(item.ImagePath);
        if (sprite == null)
            sprite = Resources.Load<Sprite>(DefaultImagePath);

        if (itemPicture != null)
            itemPicture.sprite = sprite;
        if (itemPictureSmall != null)
            itemPictureSmall.sprite = sprite;
    }
}
