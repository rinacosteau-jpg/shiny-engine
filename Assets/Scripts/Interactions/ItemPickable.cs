using UnityEngine;

public class ItemPickable : MonoBehaviour, IInteractable, ILoopResettable
{
    [SerializeField] private string itemID;
    public bool isPicked = false;

    public void Interact()
    {
        if (isPicked)
            return;

        InventoryStorage.Add(new Item(itemID));
        isPicked = true;
        gameObject.SetActive(false);
    }

    public void OnLoopReset()
    {
        isPicked = false;
        gameObject.SetActive(true);
    }
}
