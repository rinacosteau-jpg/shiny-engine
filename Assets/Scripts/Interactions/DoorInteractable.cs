using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable, ILoopResettable
{
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;
    [SerializeField] private GameObject doorObject;

    private bool startIsOpen;

    private void Awake()
    {
        if (doorObject == null && transform.childCount > 0)
            doorObject = transform.GetChild(0).gameObject;

        startIsOpen = isOpen;
        ApplyState(isOpen);
    }

    public void Interact()
    {
        if (isLocked)
            return;

        ApplyState(!isOpen);
    }

    public void OnLoopReset()
    {
        ApplyState(startIsOpen);
    }

    public void ForceOpen()
    {
        ApplyState(true);
    }

    private void ApplyState(bool open)
    {
        isOpen = open;

        if (doorObject != null)
            doorObject.SetActive(!open);
    }
}
