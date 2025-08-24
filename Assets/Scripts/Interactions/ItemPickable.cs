using System;
using UnityEngine;

public class ItemPickable : MonoBehaviour, IInteractable, ILoopResettable {
    [SerializeField] private string itemID; 

    public bool isPicked = false;

    public void Interact() {
        if (isPicked) return;
        InventoryStorage.Add(new Item(itemID));
        isPicked = true;
        gameObject.SetActive(false);
    }

    public void OnLoopReset() {
        if (GlobalVariables.Instance != null &&
            GlobalVariables.Instance.player.hasArtifact &&
            InventoryStorage.Contains(itemID)) {
            // item already in inventory, do not respawn
            gameObject.SetActive(false);
            return;
        }

        isPicked = false;
        gameObject.SetActive(true);
    }
}
