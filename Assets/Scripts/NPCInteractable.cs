using UnityEngine;
using Articy.Unity;

public class NPCInteractable : MonoBehaviour {
    [Header("Dialogue")]
    [Tooltip("��������� ���� ������� � Articy ��� ����� NPC")]
    public ArticyRef dialogueStart;

    private DialogueUI cachedDialogueUI;

    private void Awake() {
        // �������� ������������ DialogueUI �� �����
        cachedDialogueUI = FindObjectOfType<DialogueUI>();
        if (cachedDialogueUI == null) {
            Debug.LogWarning($"[{nameof(NPCInteractable)}] �� ����� �� ������ DialogueUI.");
        }
    }

    public void Interact() {
        if (cachedDialogueUI == null)
            cachedDialogueUI = FindObjectOfType<DialogueUI>();

        if (cachedDialogueUI == null) {
            Debug.LogWarning($"[{nameof(NPCInteractable)}] ��� ������� � DialogueUI.");
            return;
        }

        if (dialogueStart == null) {
            Debug.LogWarning($"[{nameof(NPCInteractable)}] �� ����� dialogueStart � {name}.");
            return;
        }

        cachedDialogueUI.StartDialogue(dialogueStart);
    }
}
