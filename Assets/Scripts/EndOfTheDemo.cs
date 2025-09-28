using UnityEngine;
using Articy.Unity;
using Articy.Unity.Interfaces;

public class EndOfTheDemo : MonoBehaviour {
    [Header("Dialogue References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private ArticyRef dialogueFragment;

    private IFlowObject targetFlowObject;
    private bool isWatching;
    private IFlowObject watchedFlowObject;
    private ArticyId? watchedFragmentId;
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

        if (isWatching)
            return;

        var currentStart = ui.CurrentStartObject;
        if (!FlowObjectsMatch(currentStart, targetFlowObject))
            return;

        isWatching = true;
        watchedFlowObject = currentStart;
        watchedFragmentId = GetArticyId(currentStart) ?? GetArticyId(targetFlowObject);
    }

    private void OnDialogueClosed(DialogueUI ui) {
        if (hasTriggered || ui != dialogueUI)
            return;

        var wasTargetDialogue = false;
        var wasWatching = isWatching;

        if (wasWatching) {
            if (watchedFragmentId.HasValue)
                wasTargetDialogue = FlowObjectMatchesId(targetFlowObject, watchedFragmentId.Value);

            if (!wasTargetDialogue)
                wasTargetDialogue = FlowObjectsMatch(watchedFlowObject, targetFlowObject);
        }

        if (!wasTargetDialogue)
            return;

        TriggerEndOfDemo();

        if (wasWatching) {
            isWatching = false;
            watchedFlowObject = null;
            watchedFragmentId = null;
        }
    }

    private static bool FlowObjectsMatch(IFlowObject currentStart, IFlowObject target) {
        if (ReferenceEquals(currentStart, target))
            return true;

        if (currentStart == null || target == null)
            return false;

        if (currentStart.Equals(target))
            return true;

        if (currentStart is ArticyObject currentArticy && target is ArticyObject targetArticy)
            return Equals(currentArticy.Id, targetArticy.Id);

        return false;
    }

    private static bool FlowObjectMatchesId(IFlowObject flowObject, ArticyId id) {
        if (flowObject is ArticyObject articyObject)
            return Equals(articyObject.Id, id);

        return false;
    }

    private static ArticyId? GetArticyId(IFlowObject flowObject) {
        if (flowObject is ArticyObject articyObject)
            return articyObject.Id;

        return null;
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
