using UnityEngine;
using Articy.Unity;
using Articy.Unity.Interfaces;

public class WatchedDialogueSequence : MonoBehaviour
{
    [Header("Dialogue References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private ArticyRef watchDialogue;
    [SerializeField] private ArticyRef guardDialogue;

    [Header("World References")]
    [SerializeField] private DoorInteractable doorToOpen;
    [SerializeField] private Transform characterToMove;
    [SerializeField] private Transform targetTransform;

    private IFlowObject watchedFlowObject;
    private bool watchedDialogueActive;
    private bool suppressNextStartCheck;

    private void Reset()
    {
        dialogueUI = FindObjectOfType<DialogueUI>();
    }

    private void Awake()
    {
        CacheWatchedFlowObject();
    }

    private void OnValidate()
    {
        CacheWatchedFlowObject();
    }

    private void OnEnable()
    {
        CacheWatchedFlowObject();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>();

        if (dialogueUI == null)
        {
            Debug.LogWarning($"[{nameof(WatchedDialogueSequence)}] DialogueUI not assigned.");
            return;
        }

        dialogueUI.DialogueStarted += OnDialogueStarted;
        dialogueUI.DialogueClosed += OnDialogueClosed;
    }

    private void Unsubscribe()
    {
        if (dialogueUI == null)
            return;

        dialogueUI.DialogueStarted -= OnDialogueStarted;
        dialogueUI.DialogueClosed -= OnDialogueClosed;
    }

    private void CacheWatchedFlowObject()
    {
        if (watchDialogue == null)
        {
            watchedFlowObject = null;
            return;
        }

        var articyObject = watchDialogue.GetObject();
        var flowObject = articyObject as IFlowObject;
        if (articyObject != null && flowObject == null)
        {
            Debug.LogWarning($"[{nameof(WatchedDialogueSequence)}] watchDialogue does not reference an IFlowObject.");
        }

        watchedFlowObject = flowObject;
    }

    private void OnDialogueStarted(DialogueUI ui)
    {
        if (ui != dialogueUI)
            return;

        if (suppressNextStartCheck)
        {
            suppressNextStartCheck = false;
            watchedDialogueActive = false;
            return;
        }

        if (watchedFlowObject == null && watchDialogue != null)
            CacheWatchedFlowObject();

        var currentStart = ui.CurrentStartObject;
        watchedDialogueActive = currentStart != null && watchedFlowObject != null && currentStart == watchedFlowObject;
    }

    private void OnDialogueClosed(DialogueUI ui)
    {
        if (ui != dialogueUI)
            return;

        if (!watchedDialogueActive)
            return;

        watchedDialogueActive = false;

        if (doorToOpen != null)
            doorToOpen.ForceOpen();

        if (characterToMove != null && targetTransform != null)
            characterToMove.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);

        if (dialogueUI != null && guardDialogue != null)
        {
            suppressNextStartCheck = true;
            dialogueUI.StartDialogue(guardDialogue);
        }
    }
}
