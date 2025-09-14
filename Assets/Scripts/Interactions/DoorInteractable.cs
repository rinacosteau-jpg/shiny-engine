using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable, ILoopResettable
{
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;
    [SerializeField] private GameObject doorObject;
    [SerializeField] private Collider interactionTrigger;

    private MeshRenderer doorRenderer;
    private Collider doorCollider;
    private bool startIsOpen;
    private bool startDoorColliderEnabled;
    private bool startInteractionTriggerEnabled;

    private void Awake()
    {
        if (doorObject == null && transform.childCount > 0)
            doorObject = transform.GetChild(0).gameObject;

        if (doorObject != null)
        {
            doorRenderer = doorObject.GetComponent<MeshRenderer>();
            doorCollider = doorObject.GetComponent<Collider>();
        }

        if (interactionTrigger != null)
        {
            interactionTrigger.gameObject.SetActive(true);
            interactionTrigger.isTrigger = true;
        }

        startIsOpen = isOpen;
        ApplyState(isOpen);

        startDoorColliderEnabled = doorCollider != null && doorCollider.enabled;
        startInteractionTriggerEnabled = interactionTrigger != null && interactionTrigger.enabled;
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

        if (doorCollider != null)
            doorCollider.enabled = startDoorColliderEnabled;

        if (interactionTrigger != null)
        {
            interactionTrigger.enabled = startInteractionTriggerEnabled;
            interactionTrigger.isTrigger = true;
        }
    }

    private void ApplyState(bool open)
    {
        isOpen = open;

        if (doorRenderer != null)
            doorRenderer.enabled = !open;

        if (doorCollider != null)
            doorCollider.enabled = !open;
    }
}
