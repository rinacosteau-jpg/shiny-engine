using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractScript : MonoBehaviour {
    [SerializeField] private float interactRange = 2f;

    private InputAction interactAction;
    private DialogueUI dialogueUI;
    private bool interactionsBlocked;

    private readonly List<IInteractable> interactablesInRange = new List<IInteractable>();
    private readonly HashSet<MonoBehaviour> interactableBehavioursInRange = new HashSet<MonoBehaviour>();
    private readonly Dictionary<MonoBehaviour, InteractableOutline> highlightedInteractables = new Dictionary<MonoBehaviour, InteractableOutline>();
    private readonly List<MonoBehaviour> highlightRemovalBuffer = new List<MonoBehaviour>();

    public event Action InteractionWhileBlocked;

    private void Start() {
        interactAction = InputSystem.actions.FindAction("Interact");
        dialogueUI = FindObjectOfType<DialogueUI>();
    }

    private void Update() {
        Collider[] colliderArray = Physics.OverlapSphere(
            transform.position,
            interactRange,
            ~0,
            QueryTriggerInteraction.Collide);

        CollectInteractables(colliderArray);

        if (dialogueUI != null && dialogueUI.IsDialogueOpen)
            return;

        if (interactAction != null && interactAction.triggered) {
            if (interactionsBlocked) {
                if (interactablesInRange.Count > 0)
                    InteractionWhileBlocked?.Invoke();
                return;
            }

            foreach (IInteractable interactable in interactablesInRange)
                interactable.Interact();
        }
    }

    private void CollectInteractables(Collider[] colliders) {
        interactablesInRange.Clear();
        interactableBehavioursInRange.Clear();

        if (colliders == null)
            return;

        foreach (Collider collider in colliders) {
            if (collider == null)
                continue;

            IInteractable interactable = collider.GetComponentInParent<IInteractable>() ??
                                         collider.GetComponentInChildren<IInteractable>();

            if (interactable == null)
                continue;

            interactablesInRange.Add(interactable);

            if (interactable is MonoBehaviour behaviour)
                interactableBehavioursInRange.Add(behaviour);
        }

        UpdateHighlightedInteractables();
    }

    private void UpdateHighlightedInteractables() {
        foreach (MonoBehaviour behaviour in interactableBehavioursInRange) {
            if (behaviour == null || highlightedInteractables.ContainsKey(behaviour))
                continue;

            InteractableOutline outline = behaviour.GetComponentInChildren<InteractableOutline>();
            if (outline == null)
                outline = behaviour.GetComponentInParent<InteractableOutline>();

            if (outline == null)
                continue;

            outline.SetHighlighted(true);
            highlightedInteractables[behaviour] = outline;
        }

        highlightRemovalBuffer.Clear();
        foreach (KeyValuePair<MonoBehaviour, InteractableOutline> pair in highlightedInteractables) {
            if (pair.Key != null && interactableBehavioursInRange.Contains(pair.Key))
                continue;

            if (pair.Value != null)
                pair.Value.SetHighlighted(false);

            highlightRemovalBuffer.Add(pair.Key);
        }

        foreach (MonoBehaviour behaviour in highlightRemovalBuffer)
            highlightedInteractables.Remove(behaviour);
    }

    public void SetInteractionsBlocked(bool blocked)
    {
        interactionsBlocked = blocked;
    }
}
