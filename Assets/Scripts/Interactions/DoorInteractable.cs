using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;
    [SerializeField] private GameObject doorObject;

    private void Awake()
    {
        if (doorObject == null)
            doorObject = gameObject;
    }

    public void Interact()
    {
        if (isLocked)
            return;

        if (!isOpen)
        {
            doorObject.SetActive(false);
            isOpen = true;
        }
        else
        {
            doorObject.SetActive(true);
            isOpen = false;
        }
    }
}
