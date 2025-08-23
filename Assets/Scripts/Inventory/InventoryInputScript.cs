using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInputScript : MonoBehaviour {
    private InputAction inventoryAction;
    private InputAction escapeAction;
    private InventoryUI inventoryUI;

    void Start() {
        inventoryAction = InputSystem.actions.FindAction("Inventory");
        escapeAction = InputSystem.actions.FindAction("Escape");
        inventoryUI = FindObjectOfType<InventoryUI>();
    }

    void Update() {
        if (inventoryUI == null)
            return;

        if (inventoryAction != null && inventoryAction.triggered) {
            Debug.Log("opened inv");
            inventoryUI.Show();
        }
            

        if (escapeAction != null && escapeAction.triggered) {
            Debug.Log("close inv");
            inventoryUI.Hide();
        }
            
    }
}
