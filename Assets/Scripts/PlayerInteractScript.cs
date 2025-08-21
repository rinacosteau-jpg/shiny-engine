using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player interaction with nearby objects.
/// </summary>
public class PlayerInteractScript : MonoBehaviour
{
    [SerializeField] private float interactRange = 2f;

    private InputAction interactAction;

    private void Awake()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void OnEnable()
    {
        interactAction?.Enable();
        if (interactAction != null)
            interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;
        interactAction?.Disable();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        var hits = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable))
                interactable.Interact();
        }
    }
}
