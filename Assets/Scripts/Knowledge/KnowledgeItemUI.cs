using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KnowledgeItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private KnowledgeManager.Knowledge knowledge;
    private KnowledgeUI knowledgeUI;
    private CanvasGroup canvasGroup;
    private static KnowledgeItemUI currentlyDragging;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(KnowledgeManager.Knowledge knowledge, KnowledgeUI ui)
    {
        this.knowledge = knowledge;
        knowledgeUI = ui;

        UpdateLabel();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (knowledge == null)
            return;

        currentlyDragging = this;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentlyDragging == this)
            currentlyDragging = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (currentlyDragging == null || currentlyDragging == this)
            return;

        var sourceKnowledgeName = currentlyDragging.knowledge?.Name;
        var targetKnowledgeName = knowledge?.Name;

        if (KnowledgeManager.TryCombineKnowledge(sourceKnowledgeName, targetKnowledgeName, out var resultKnowledge))
            knowledgeUI?.RequestSelectKnowledge(resultKnowledge);
    }

    private void UpdateLabel()
    {
        var text = GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = KnowledgeUI.FormatKnowledgeName(knowledge?.Name ?? string.Empty);
    }

    private void OnClick()
    {
        if (knowledge != null)
            knowledgeUI?.SelectKnowledge(knowledge);
    }
}
