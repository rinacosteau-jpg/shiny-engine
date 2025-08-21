using UnityEngine;
using Articy.Unity;

public class NPCInteractable : MonoBehaviour {
    [Header("Dialogue")]
    [Tooltip("Стартовый узел диалога в Articy для этого NPC")]
    public ArticyRef dialogueStart;

    private DialogueUI cachedDialogueUI;

    private void Awake() {
        // Кешируем единственный DialogueUI на сцене
        cachedDialogueUI = FindObjectOfType<DialogueUI>();
        if (cachedDialogueUI == null) {
            Debug.LogWarning($"[{nameof(NPCInteractable)}] На сцене не найден DialogueUI.");
        }
    }

    public void Interact() {
        if (cachedDialogueUI == null)
            cachedDialogueUI = FindObjectOfType<DialogueUI>();

        if (cachedDialogueUI == null) {
            Debug.LogWarning($"[{nameof(NPCInteractable)}] Нет доступа к DialogueUI.");
            return;
        }

        if (dialogueStart == null) {
            Debug.LogWarning($"[{nameof(NPCInteractable)}] Не задан dialogueStart у {name}.");
            return;
        }

        cachedDialogueUI.StartDialogue(dialogueStart);
    }
}
