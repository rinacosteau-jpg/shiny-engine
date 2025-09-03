using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Articy.Unity;
using Articy.Unity.Interfaces;

public class DialogueUI : MonoBehaviour, ILoopResettable
{
    [Header("Articy")]
    [SerializeField] private ArticyFlowPlayer flowPlayer;
    [SerializeField] private Entity playerEntity;

    [Header("UI")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private TMP_Text dialogueSpeaker;
    [SerializeField] private ResponseHandler responseHandler;

    private DialogueController controller;

    public bool IsDialogueOpen => controller != null && controller.IsActive;

    private void Awake()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    public void BindController(DialogueController ctrl)
    {
        controller = ctrl;
    }

    public void StartDialogue(ArticyRef startRef)
    {
        controller?.OpenDialogue(startRef, new DialogueContext());
    }

    public void StartDialogue(IFlowObject startObject)
    {
        if (startObject == null) return;
        var refId = new ArticyRef(startObject);
        controller?.OpenDialogue(refId, new DialogueContext());
    }

    public void CloseDialogue()
    {
        controller?.CloseDialogue();
    }

    public void OnLoopReset()
    {
        CloseDialogue();
    }

    public void DisplayNode(NodeData node)
    {
        if (node?.node == null) return;

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        if (dialogueSpeaker != null)
        {
            var ent = GetSpeakerEntity(node.node);
            dialogueSpeaker.text = ent != null ? ent.DisplayName : string.Empty;
        }

        if (textLabel != null)
        {
            string text = GetText(node.node);
            textLabel.text = text;
        }
    }

    public void DisplayChoices(List<ChoiceData> choices, DialogueController ctrl)
    {
        responseHandler?.ShowResponses(choices, ctrl);
    }

    public void DisableChoices()
    {
        responseHandler?.DisableAll();
    }

    public void RefreshBindings() { }

    private string GetText(IFlowObject obj)
    {
        if (obj is IObjectWithText wt && !string.IsNullOrEmpty(wt.Text))
            return wt.Text;
        return obj.ToString();
    }

    private Entity GetSpeakerEntity(IFlowObject obj)
    {
        if (obj is IObjectWithSpeaker ws && ws.Speaker is Entity ent)
            return ent;
        return null;
    }
}
