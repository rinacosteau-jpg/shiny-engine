using System;
using UnityEngine;

public class ItemPickable : MonoBehaviour, IInteractable, ILoopResettable {
    [SerializeField] private string itemID;
    [SerializeField] private bool dontRespawnIfHasArtefact; // <- добавь

    public bool isPicked = false;

    public void Interact() {
        if (isPicked) return;
        InventoryStorage.Add(new Item(itemID));
        isPicked = true;
        gameObject.SetActive(false);
    }

    public void OnLoopReset() {
        if (dontRespawnIfHasArtefact &&
            GlobalVariables.Instance != null &&
            GlobalVariables.Instance.player.hasArtifact &&
            itemID.Equals(ItemIds.InventoryArtefact, StringComparison.OrdinalIgnoreCase)) {
            // не респавним, игрок уже владеет артефактом
            gameObject.SetActive(false);
            return;
        }

        isPicked = false;
        gameObject.SetActive(true);
    }
}
