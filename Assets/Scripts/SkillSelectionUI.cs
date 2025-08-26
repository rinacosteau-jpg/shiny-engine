using System;
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

    [SerializeField] private SkillSlot[] slots;
    [SerializeField] private TMP_Text pointsLeftText;
    [SerializeField] private Button okButton;

    private int pointsLeft;
    private PlayerState player;

    private void Awake() {
        gameObject.SetActive(false);
    }

    public void Open(PlayerState player) {
        this.player = player;
        pointsLeft = 10;
        foreach (var slot in slots) {
            var field = typeof(PlayerState).GetField(slot.fieldName);
            if (field != null && field.FieldType == typeof(Skill)) {
                slot.skill = (Skill)field.GetValue(player);
                if (slot.nameText)
                    slot.nameText.text = slot.fieldName;
            }
            slot.value = 0;
            if (slot.valueText)
                slot.valueText.text = "0";
            if (slot.plusButton) {
                slot.plusButton.onClick.RemoveAllListeners();
                slot.plusButton.onClick.AddListener(() => Change(slot, 1));
            }
            if (slot.minusButton) {
                slot.minusButton.onClick.RemoveAllListeners();
                slot.minusButton.onClick.AddListener(() => Change(slot, -1));
            }
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
}
