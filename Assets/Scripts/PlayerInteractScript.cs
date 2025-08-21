using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractScript : MonoBehaviour {
    InputAction interactAction;

    void Start() {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    void Update() {
        if (interactAction != null && interactAction.triggered) {
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
