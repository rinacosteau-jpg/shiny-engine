using UnityEngine;
using Articy.Unity;

/// <summary>
/// Base interactable that starts a dialogue when interacted with.
/// Handles the connection to <see cref="DialogueUI"/>.
/// </summary>
public abstract class DialogueInteractable : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [Tooltip("Articy start node for the dialogue")]
    public ArticyRef dialogueStart;

    private DialogueUI cachedDialogueUI;

    protected virtual void Awake()
    {
        CacheDialogueUI();
    }

    /// <summary>
    /// Interact with this object and start the dialogue.
    /// </summary>
    public void Interact()
    {
        CacheDialogueUI();

        if (cachedDialogueUI == null)
        {
            // Debug.LogWarning($"[{nameof(DialogueInteractable)}] DialogueUI not found.");
            return;
        }

        if (dialogueStart == null)
        {
            // Debug.LogWarning($"[{nameof(DialogueInteractable)}] dialogueStart is not assigned for {name}.");
            return;
        }

        cachedDialogueUI.StartDialogue(dialogueStart);
    }

    private void CacheDialogueUI()
    {
        if (cachedDialogueUI == null)
            cachedDialogueUI = FindObjectOfType<DialogueUI>();
    }
}
