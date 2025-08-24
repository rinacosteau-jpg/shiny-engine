using System;
using UnityEngine;

public class ItemPickable : MonoBehaviour, IInteractable, ILoopResettable {
    [SerializeField] private string itemID;
    [SerializeField] private string instanceID;

    public bool isPicked = false;

    private void Awake() {
        if (string.IsNullOrEmpty(instanceID))
            instanceID = Guid.NewGuid().ToString();
    }

    public void Interact() {
        if (isPicked) return;
        InventoryStorage.Add(itemID, instanceId: instanceID);
        isPicked = true;
        gameObject.SetActive(false);
    }

    public void OnLoopReset() {
        if (GlobalVariables.Instance != null &&
            GlobalVariables.Instance.player.hasArtifact &&
            InventoryStorage.ContainsInstance(itemID, instanceID)) {
            // item already in inventory, do not respawn
            gameObject.SetActive(false);
            return;
        }

        isPicked = false;
        gameObject.SetActive(true);
    }
}
