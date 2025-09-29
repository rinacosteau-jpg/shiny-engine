using System.Collections;
using UnityEngine;
using Articy.Unity;
using Articy.Unity.Interfaces;

public class WatchedDialogueSequence : MonoBehaviour
{
    [Header("Dialogue References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private ArticyRef watchDialogue;
    [SerializeField] private ArticyRef guardDialogue;

    [Header("Guard Dialogue")]
    [SerializeField] private GameObject guardDialogueBackground;
    [SerializeField] private float guardDialogueDelay = 3f;

    [Header("World References")]
    [SerializeField] private DoorInteractable doorToOpen;
    [SerializeField] private Transform characterToMove;
    [SerializeField] private Transform targetTransform;

    private IFlowObject watchedFlowObject;
    private bool watchedDialogueActive;
    private bool suppressNextStartCheck;
    private bool guardDialogueActive;
    private Coroutine guardDialogueRoutine;
    private Vector3 originalCharacterPosition;
    private Quaternion originalCharacterRotation;
    private bool hasOriginalCharacterTransform;

    private void Reset()
    {
        dialogueUI = FindObjectOfType<DialogueUI>();
    }

    private void Awake()
    {
        CacheWatchedFlowObject();
        CacheOriginalCharacterTransform();
        HideGuardDialogueBackground();
    }

    private void OnValidate()
    {
        CacheWatchedFlowObject();
        if (!Application.isPlaying)
            CacheOriginalCharacterTransform();
    }

    private void OnEnable()
    {
        CacheWatchedFlowObject();
        EnsureOriginalCharacterTransformCached();
        HideGuardDialogueBackground();
        Subscribe();
    }

    private void OnDisable()
    {
        CancelGuardDialogueRoutine();
        HideGuardDialogueBackground();
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

    private void CacheOriginalCharacterTransform()
    {
        if (characterToMove == null)
        {
            hasOriginalCharacterTransform = false;
            return;
        }

        originalCharacterPosition = characterToMove.position;
        originalCharacterRotation = characterToMove.rotation;
        hasOriginalCharacterTransform = true;
    }

    private void EnsureOriginalCharacterTransformCached()
    {
        if (hasOriginalCharacterTransform || characterToMove == null)
            return;

        CacheOriginalCharacterTransform();
    }

    private void RestoreCharacterToOriginal()
    {
        if (!hasOriginalCharacterTransform || characterToMove == null)
            return;

        characterToMove.SetPositionAndRotation(originalCharacterPosition, originalCharacterRotation);
    }

    private void HideGuardDialogueBackground()
    {
        if (guardDialogueBackground == null)
            return;

        guardDialogueBackground.SetActive(false);
    }

    private void CancelGuardDialogueRoutine()
    {
        if (guardDialogueRoutine == null)
            return;

        StopCoroutine(guardDialogueRoutine);
        guardDialogueRoutine = null;
        HideGuardDialogueBackground();
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

        if (guardDialogueActive)
        {
            guardDialogueActive = false;
            RestoreCharacterToOriginal();
            return;
        }

        if (!watchedDialogueActive)
            return;

        watchedDialogueActive = false;

        if (doorToOpen != null)
            doorToOpen.ForceOpen();

        if (characterToMove != null && targetTransform != null)
            characterToMove.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);

        CancelGuardDialogueRoutine();

        if (dialogueUI != null && guardDialogue != null)
            guardDialogueRoutine = StartCoroutine(BeginGuardDialogueSequence());
    }

    private IEnumerator BeginGuardDialogueSequence()
    {
        if (guardDialogueBackground != null)
            guardDialogueBackground.SetActive(true);

        float delay = Mathf.Max(guardDialogueDelay, 0f);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (guardDialogueBackground != null)
            guardDialogueBackground.SetActive(false);

        if (dialogueUI != null && guardDialogue != null)
        {
            suppressNextStartCheck = true;
            guardDialogueActive = true;
            dialogueUI.StartDialogue(guardDialogue);
        }

        guardDialogueRoutine = null;
    }
}
