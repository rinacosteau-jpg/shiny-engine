using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSelectionUI : MonoBehaviour {
    [Serializable]
    private class SkillSlot {
        public string fieldName;
        public TMP_Text nameText;
        public TMP_Text valueText;
        public Button plusButton;
        public Button minusButton;
        [HideInInspector] public Skill skill;
        [HideInInspector] public int value;
    }

    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private TMP_Text pointsLeftText;
    [SerializeField] private Button okButton;

    private readonly List<SkillSlot> slots = new();

    private int pointsLeft;
    private PlayerState player;

    private void Awake() {
        if (!slotContainer) {
            var go = new GameObject("Slots", typeof(RectTransform), typeof(VerticalLayoutGroup));
            slotContainer = go.GetComponent<RectTransform>();
            slotContainer.SetParent(transform, false);
            slotContainer.anchorMin = Vector2.zero;
            slotContainer.anchorMax = Vector2.one;
            slotContainer.offsetMin = Vector2.zero;
            slotContainer.offsetMax = Vector2.zero;
            slotContainer.SetSiblingIndex(0);
            if (transform.GetComponent<LayoutGroup>() && !slotContainer.GetComponent<LayoutElement>())
                slotContainer.gameObject.AddComponent<LayoutElement>();
            var layout = slotContainer.GetComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 5f;
        }
        gameObject.SetActive(false);
    }

    public void Open(PlayerState player) {
        this.player = player;
        pointsLeft = 10;

        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);
        slots.Clear();

        foreach (var field in typeof(PlayerState).GetFields()) {
            if (field.FieldType != typeof(Skill)) continue;
            var skill = (Skill)field.GetValue(player);
            slots.Add(CreateSlot(field.Name, skill));
        }

        UpdateUI();
        gameObject.SetActive(true);
    }

    private void Change(SkillSlot slot, int delta) {
        if (delta > 0 && pointsLeft == 0) return;
        if (delta < 0 && slot.value == 0) return;
        slot.value += delta;
        pointsLeft -= delta;
        if (slot.valueText)
            slot.valueText.text = slot.value.ToString();
        UpdateUI();
    }

    private void UpdateUI() {
        if (pointsLeftText)
            pointsLeftText.text = pointsLeft.ToString();
        if (okButton)
            okButton.interactable = pointsLeft == 0;
        foreach (var slot in slots) {
            if (slot.minusButton)
                slot.minusButton.interactable = slot.value > 0;
            if (slot.plusButton)
                slot.plusButton.interactable = pointsLeft > 0;
        }
    }

    public void Confirm() {
        if (pointsLeft != 0) return;
        foreach (var slot in slots) {
            if (slot.skill != null)
                slot.skill.Value = slot.value;
        }
        gameObject.SetActive(false);
    }

    private SkillSlot CreateSlot(string name, Skill skill) {
        var slotObj = new GameObject(name, typeof(RectTransform));
        slotObj.transform.SetParent(slotContainer, false);
        var layout = slotObj.AddComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 5f;

        var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(slotObj.transform, false);
        var nameText = nameObj.GetComponent<TextMeshProUGUI>();
        nameText.text = name;

        var minus = CreateButton("-", slotObj.transform);
        var valueObj = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
        valueObj.transform.SetParent(slotObj.transform, false);
        var valueText = valueObj.GetComponent<TextMeshProUGUI>();
        valueText.text = "0";
        var plus = CreateButton("+", slotObj.transform);

        var slot = new SkillSlot {
            fieldName = name,
            nameText = nameText,
            valueText = valueText,
            plusButton = plus,
            minusButton = minus,
            skill = skill,
            value = 0
        };

        plus.onClick.AddListener(() => Change(slot, 1));
        minus.onClick.AddListener(() => Change(slot, -1));
        return slot;
    }

    private Button CreateButton(string label, Transform parent) {
        var btnObj = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        var img = btnObj.GetComponent<Image>();
        var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = Color.gray;

        var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        var txt = textObj.GetComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.alignment = TextAlignmentOptions.Center;
        txt.raycastTarget = false;

        var rect = txt.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return btnObj.GetComponent<Button>();
    }
}
