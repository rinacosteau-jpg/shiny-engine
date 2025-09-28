using UnityEngine;

public static class GameOver {
    private static bool isActive;

    public static void Trigger() {
        if (isActive)
            return;

        isActive = true;

        CloseOpenWindows();
        DisablePlayerControls();
        ActivateGameOverPanel();

        Debug.Log("[GameOver] Triggered");
    }

    public static void ResetState() {
        isActive = false;
    }

    private static void CloseOpenWindows() {
        GlobalVariables.Instance?.ForceCloseDialogue();

        foreach (var inventory in Object.FindObjectsOfType<InventoryUI>(true))
            inventory.Hide();

        foreach (var journal in Object.FindObjectsOfType<JournalUI>(true))
            journal.Hide();

        foreach (var menuToggle in Object.FindObjectsOfType<MenuToggle>(true)) {
            menuToggle.HideMenu();
            menuToggle.enabled = false;
        }
    }

    private static void DisablePlayerControls() {
        foreach (var movement in Object.FindObjectsOfType<PlayerMovementScript>(true))
            movement.enabled = false;

        foreach (var interact in Object.FindObjectsOfType<PlayerInteractScript>(true))
            interact.enabled = false;
    }

    private static void ActivateGameOverPanel() {
        GameObject panel = null;
        var rects = Object.FindObjectsOfType<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++) {
            var rect = rects[i];
            if (rect != null && rect.gameObject != null && rect.gameObject.name == "GameOver") {
                panel = rect.gameObject;
                break;
            }
        }

        if (panel == null) {
            Debug.LogWarning("[GameOver] GameOver panel not found in scene.");
            return;
        }

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        var canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup != null) {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}

