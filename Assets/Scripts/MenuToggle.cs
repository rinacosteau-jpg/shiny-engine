using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggle : MonoBehaviour {
    [SerializeField] private GameObject menuPanel;
    private InputAction escapeAction;
    private CanvasGroup menuCanvasGroup;
    private bool isMenuVisible;

    private void Start() {
        escapeAction = InputSystem.actions.FindAction("Escape");
        if (menuPanel == null) {
            var found = GameObject.Find("Menu");
            if (found != null) {
                menuPanel = found;
            }
        }

        if (menuPanel != null) {
            menuCanvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null) {
                menuCanvasGroup = menuPanel.AddComponent<CanvasGroup>();
            }

            isMenuVisible = menuPanel.activeSelf;
            menuPanel.SetActive(true);
            UpdateMenuVisibility();
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

        isMenuVisible = !isMenuVisible;
        UpdateMenuVisibility();
        if (isMenuVisible) {
            menuPanel.transform.SetAsLastSibling();
        }
    }

    public void HideMenu() {
        if (menuPanel == null)
            return;

        isMenuVisible = false;

        if (menuCanvasGroup == null)
            menuCanvasGroup = menuPanel.GetComponent<CanvasGroup>();

        if (menuCanvasGroup == null) {
            menuPanel.SetActive(false);
            return;
        }

        if (!menuPanel.activeSelf)
            menuPanel.SetActive(true);

        UpdateMenuVisibility();
    }

    private void UpdateMenuVisibility() {
        if (menuCanvasGroup == null)
            return;

        menuCanvasGroup.alpha = isMenuVisible ? 1f : 0f;
        menuCanvasGroup.interactable = isMenuVisible;
        menuCanvasGroup.blocksRaycasts = isMenuVisible;
    }
}
