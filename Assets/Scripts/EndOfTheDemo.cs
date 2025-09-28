using UnityEngine;
using Articy.Unity;
using Articy.Unity.Interfaces;

public class EndOfTheDemo : MonoBehaviour {
    [Header("Dialogue References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private ArticyRef dialogueFragment;

    private IFlowObject targetFlowObject;
    private bool watchedDialogueActive;
    private bool hasTriggered;

    private void Reset() {
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>();
    }

    private void Awake() {
        CacheTargetFlowObject();
    }

    private void OnValidate() {
        CacheTargetFlowObject();
    }

    private void OnEnable() {
        CacheTargetFlowObject();
        Subscribe();
    }

    private void OnDisable() {
        Unsubscribe();
    }

    private void Subscribe() {
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>();

        if (dialogueUI == null) {
            Debug.LogWarning($"[{nameof(EndOfTheDemo)}] DialogueUI not assigned.");
            return;
        }

        dialogueUI.DialogueStarted += OnDialogueStarted;
        dialogueUI.DialogueClosed += OnDialogueClosed;
    }

    private void Unsubscribe() {
        if (dialogueUI == null)
            return;

        dialogueUI.DialogueStarted -= OnDialogueStarted;
        dialogueUI.DialogueClosed -= OnDialogueClosed;
    }

    private void CacheTargetFlowObject() {
        if (dialogueFragment == null) {
            targetFlowObject = null;
            return;
        }

        var articyObject = dialogueFragment.GetObject();
        targetFlowObject = articyObject as IFlowObject;
        if (articyObject != null && targetFlowObject == null)
            Debug.LogWarning($"[{nameof(EndOfTheDemo)}] dialogueFragment does not reference an IFlowObject.");
    }

    private void OnDialogueStarted(DialogueUI ui) {
        if (hasTriggered || ui != dialogueUI)
            return;

        if (targetFlowObject == null && dialogueFragment != null)
            CacheTargetFlowObject();

        var currentStart = ui.CurrentStartObject;
        Debug.Log("start to watch");
        var matchesTarget = currentStart != null && targetFlowObject != null && currentStart == targetFlowObject;

        watchedDialogueActive = matchesTarget;
    }

    private void OnDialogueClosed(DialogueUI ui) {
        if (hasTriggered || ui != dialogueUI)
            return;

        if (!watchedDialogueActive)
            return;

        Debug.Log("dialogue not watched anymore");
        watchedDialogueActive = false;

        TriggerEndOfDemo();
    }

    private void TriggerEndOfDemo() {
        hasTriggered = true;
        Unsubscribe();

        CloseOpenWindows();
        DisablePlayerControls();
        ActivateEndOfDemoPanel();

        Debug.Log("[EndOfTheDemo] Triggered");
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

    private static void ActivateEndOfDemoPanel() {
        GameObject panel = null;
        var rects = Object.FindObjectsOfType<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++) {
            var rect = rects[i];
            if (rect != null && rect.gameObject != null && rect.gameObject.name == "EndOfTheDemo") {
                panel = rect.gameObject;
                break;
            }
        }

        if (panel == null) {
            Debug.LogWarning("[EndOfTheDemo] Panel not found in scene.");
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
