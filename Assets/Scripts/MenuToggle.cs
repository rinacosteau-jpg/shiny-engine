using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggle : MonoBehaviour {
    [SerializeField] private GameObject menuPanel;
    private InputAction escapeAction;

    private void Start() {
        escapeAction = InputSystem.actions.FindAction("Escape");
        if (menuPanel == null) {
            var found = GameObject.Find("Menu");
            if (found != null) {
                menuPanel = found;
            }
        }
    }

    private void Update() {
        if (escapeAction == null)
            return;

        if (escapeAction.triggered) {
            ToggleMenu();
        }
    }

    private void ToggleMenu() {
        if (menuPanel == null)
            return;

        bool newState = !menuPanel.activeSelf;
        menuPanel.SetActive(newState);
        if (newState) {
            menuPanel.transform.SetAsLastSibling();
        }
    }
}
