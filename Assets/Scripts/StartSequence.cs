using UnityEngine;
using Articy.Unity;

/// <summary>
/// Controls the opening sequence: dialogue A → skill selection → dialogue B.
/// Ensures the background panel stays visible for the entire sequence.
/// </summary>
public class StartSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private SkillSelectionUI skillSelectionUI;

    [Header("Dialogue Starts")]
    [SerializeField] private ArticyRef dialogueStartA;
    [SerializeField] private ArticyRef dialogueStartB;

    [Header("UI")]
    [SerializeField] private GameObject backgroundPanel;

    private enum SequenceStep
    {
        None,
        DialogueA,
        SkillSelection,
        DialogueB,
        Completed
    }

    private SequenceStep currentStep = SequenceStep.None;

    private void Awake()
    {
        if (backgroundPanel != null)
            backgroundPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (dialogueUI != null)
            dialogueUI.DialogueClosed += HandleDialogueClosed;
        if (skillSelectionUI != null)
            skillSelectionUI.Confirmed += HandleSkillsConfirmed;
    }

    private void OnDisable()
    {
        if (dialogueUI != null)
            dialogueUI.DialogueClosed -= HandleDialogueClosed;
        if (skillSelectionUI != null)
            skillSelectionUI.Confirmed -= HandleSkillsConfirmed;
    }

    private void Start()
    {
        BeginSequence();
    }

    private void BeginSequence()
    {
        if (currentStep != SequenceStep.None)
            return;

        if (backgroundPanel != null)
            backgroundPanel.SetActive(true);

        if (dialogueUI == null)
        {
            Debug.LogError("[StartSequence] DialogueUI reference is missing.");
            FinishSequence();
            return;
        }

        if (dialogueStartA == null)
        {
            Debug.LogError("[StartSequence] Dialogue start A is not assigned.");
            FinishSequence();
            return;
        }

        currentStep = SequenceStep.DialogueA;
        dialogueUI.StartDialogue(dialogueStartA);
    }

    private void HandleDialogueClosed(DialogueUI ui)
    {
        if (ui != dialogueUI)
            return;

        switch (currentStep)
        {
            case SequenceStep.DialogueA:
                StartSkillSelection();
                break;
            case SequenceStep.DialogueB:
                FinishSequence();
                break;
        }
    }

    private void StartSkillSelection()
    {
        currentStep = SequenceStep.SkillSelection;

        if (skillSelectionUI == null)
        {
            Debug.LogError("[StartSequence] SkillSelectionUI reference is missing.");
            StartDialogueB();
            return;
        }

        skillSelectionUI.Open();
    }

    private void HandleSkillsConfirmed()
    {
        if (currentStep != SequenceStep.SkillSelection)
            return;

        StartDialogueB();
    }

    private void StartDialogueB()
    {
        currentStep = SequenceStep.DialogueB;

        if (dialogueUI == null)
        {
            Debug.LogError("[StartSequence] DialogueUI reference is missing when starting dialogue B.");
            FinishSequence();
            return;
        }

        if (dialogueStartB == null)
        {
            Debug.LogError("[StartSequence] Dialogue start B is not assigned.");
            FinishSequence();
            return;
        }

        dialogueUI.StartDialogue(dialogueStartB);
    }

    private void FinishSequence()
    {
        if (currentStep == SequenceStep.Completed)
            return;

        currentStep = SequenceStep.Completed;

        if (backgroundPanel != null)
            backgroundPanel.SetActive(false);

        if (dialogueUI != null)
            dialogueUI.DialogueClosed -= HandleDialogueClosed;
        if (skillSelectionUI != null)
            skillSelectionUI.Confirmed -= HandleSkillsConfirmed;
    }
}
