using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractScript : MonoBehaviour {
    InputAction interactAction;
    private DialogueUI dialogueUI;

    void Start() {
        interactAction = InputSystem.actions.FindAction("Interact");
        dialogueUI = FindObjectOfType<DialogueUI>();
    }

    void Update() {
        if (dialogueUI != null && dialogueUI.IsDialogueOpen)
            return;

        if (interactAction != null && interactAction.triggered) {
            Debug.Log("called");
            float interactRange = 2f;
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray) {
                var interactable = collider.GetComponent(typeof(IInteractable)) as IInteractable;

                if (interactable != null) {
                    
                    interactable.Interact();
                    
                }
            }
        }
    }
}
