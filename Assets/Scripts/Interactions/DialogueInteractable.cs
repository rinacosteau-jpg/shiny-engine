using System.Collections;
using Articy.Unity;
using TMPro;
using UnityEngine;

/// <summary>
/// Universal interactable that starts a dialogue when interacted with.
/// Can be attached to any object with a collider.
/// Handles the connection to <see cref="DialogueUI"/>.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [Tooltip("Articy start node for the dialogue")]
    public ArticyRef dialogueStart;

    [Header("Repeat")]
    [SerializeField] private bool repeatable = true;
    [SerializeField] private TMP_Text repeatBlockedLabel;
    [SerializeField] private CanvasGroup repeatBlockedCanvasGroup;
    [SerializeField] private float repeatBlockedFadeDuration = 1f;

    private DialogueUI cachedDialogueUI;
    private Coroutine repeatBlockedRoutine;
    private bool hasInteracted;

    protected virtual void Awake()
    {
        CacheDialogueUI();
        InitializeRepeatBlockedMessage();
    }

    /// <summary>
    /// Interact with this object and start the dialogue.
    /// </summary>
    public void Interact()
    {
        if (!repeatable && hasInteracted)
        {
            ShowRepeatBlockedMessage();
            return;
        }

        CacheDialogueUI();

        if (cachedDialogueUI == null)
        {
            Debug.LogWarning($"[{nameof(DialogueInteractable)}] DialogueUI not found.");
            return;
        }

        if (dialogueStart == null)
        {
            Debug.LogWarning($"[{nameof(DialogueInteractable)}] dialogueStart is not assigned for {name}.");
            return;
        }

        cachedDialogueUI.StartDialogue(dialogueStart);

        if (!repeatable)
            hasInteracted = true;
    }

    protected virtual void OnDisable()
    {
        if (repeatBlockedRoutine != null)
        {
            StopCoroutine(repeatBlockedRoutine);
            repeatBlockedRoutine = null;
        }

        if (repeatBlockedCanvasGroup != null)
            repeatBlockedCanvasGroup.alpha = 0f;

        if (repeatBlockedLabel != null)
            repeatBlockedLabel.text = string.Empty;
    }

    private void CacheDialogueUI()
    {
        if (cachedDialogueUI == null)
            cachedDialogueUI = FindObjectOfType<DialogueUI>();
    }

    private void InitializeRepeatBlockedMessage()
    {
        if (repeatBlockedCanvasGroup != null)
            repeatBlockedCanvasGroup.alpha = 0f;

        if (repeatBlockedLabel != null)
        {
            repeatBlockedLabel.text = string.Empty;
            repeatBlockedLabel.color = Color.white;
        }
    }

    private void ShowRepeatBlockedMessage()
    {
        if (repeatBlockedLabel == null || repeatBlockedCanvasGroup == null)
            return;

        repeatBlockedLabel.text = "Nothing interesting here";
        repeatBlockedLabel.color = Color.white;

        if (repeatBlockedRoutine != null)
            StopCoroutine(repeatBlockedRoutine);

        repeatBlockedRoutine = StartCoroutine(FadeRepeatBlockedMessage());
    }

    private IEnumerator FadeRepeatBlockedMessage()
    {
        if (repeatBlockedCanvasGroup == null)
            yield break;

        float duration = Mathf.Max(repeatBlockedFadeDuration, Mathf.Epsilon);
        repeatBlockedCanvasGroup.alpha = 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            repeatBlockedCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        repeatBlockedCanvasGroup.alpha = 0f;
        repeatBlockedRoutine = null;
    }
}
