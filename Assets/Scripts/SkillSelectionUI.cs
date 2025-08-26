using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSelectionUI : MonoBehaviour {
    [Header("�����")]
    [SerializeField] private TMP_Text pointsLeftText;   // "���������� Skillpoints"
    [SerializeField] private Button okButton;           // ������� ������ ����� pointsLeft == 0
    [SerializeField] private int startPoints = 10;      // ������� ����� ����� �� �������������

    [Serializable]
    private class SkillSlotUI {
        [Header("UI")]
        public string displayName;          // ������� � UI (��������, "Persuasion")
        public TMP_Text nameText;           // ����� ��������
        public TMP_Text valueText;          // ����� �������� �������� (������������� �����)
        public Button plusButton;           // "+"
        public Button minusButton;          // "-"

        [Header("������")]
        public Skill skill;                 // ������ �� ������ Skill �� PlayerState
        [HideInInspector] public int value; // ������� ����� �������� �������� (�� �������)
    }

    [Header("����� #1 (Persuasion)")]
    [SerializeField] private SkillSlotUI skill1;

    [Header("����� #2 (Perseption)")]
    [SerializeField] private SkillSlotUI skill2;

    private int pointsLeft;

    private void Awake() {
        gameObject.SetActive(false);

        WireSlot(skill1);
        WireSlot(skill2);

        if (okButton)
            okButton.onClick.AddListener(Confirm);
    }

    /// <summary>
    /// ������� ���� � ��������� � ������� �� PlayerState.
    /// �������� � UI �������� � ���� (������������ ������ �������������� ����),
    /// ��� Confirm() ����� ������� � Skill.Value = ��������_�������������.
    /// </summary>
    public void Open(PlayerState player, int? pointsOverride = null) {
        // ����������� ������ �� �������� Skill �� ���������� PlayerState
        skill1.skill = player.skillPersuasion;
        skill2.skill = player.skillPerseption;

        pointsLeft = pointsOverride ?? startPoints;

        ResetSlot(skill1);
        ResetSlot(skill2);

        // �������
        if (skill1.nameText && !string.IsNullOrEmpty(skill1.displayName))
            skill1.nameText.text = skill1.displayName;
        if (skill2.nameText && !string.IsNullOrEmpty(skill2.displayName))
            skill2.nameText.text = skill2.displayName;

        UpdateUI();
        gameObject.SetActive(true);
    }

    private void WireSlot(SkillSlotUI s) {
        if (s == null) return;

        if (s.valueText) s.valueText.text = "0";
        if (s.plusButton) s.plusButton.onClick.AddListener(() => Change(s, +1));
        if (s.minusButton) s.minusButton.onClick.AddListener(() => Change(s, -1));
    }

    private void ResetSlot(SkillSlotUI s) {
        if (s == null) return;
        s.value = 0;
        if (s.valueText) s.valueText.text = "0";
    }

    private void Change(SkillSlotUI s, int delta) {
        if (s == null) return;
        if (delta > 0 && pointsLeft == 0) return; // ��� ����� � �� ���������
        if (delta < 0 && s.value == 0) return;     // �� ������ ���� ���� �� ���������� �������������

        s.value += delta;
        pointsLeft -= delta;

        if (s.valueText)
            s.valueText.text = s.value.ToString();

        UpdateUI();
    }

    private void UpdateUI() {
        if (pointsLeftText)
            pointsLeftText.text = pointsLeft.ToString();

        if (okButton)
            okButton.interactable = (pointsLeft == 0);

        UpdateSlotInteractable(skill1);
        UpdateSlotInteractable(skill2);
    }

    private void UpdateSlotInteractable(SkillSlotUI s) {
        if (s == null) return;
        if (s.plusButton) s.plusButton.interactable = pointsLeft > 0;
        if (s.minusButton) s.minusButton.interactable = s.value > 0;
    }

    public void Confirm() {
        // �� ������ ������ ��������
        if (pointsLeft != 0) return;

        ApplySlot(skill1);
        ApplySlot(skill2);

        gameObject.SetActive(false);
    }

    private void ApplySlot(SkillSlotUI s) {
        if (s?.skill != null) {
            // ������� ������� ������: ����� ������� = ������������� ����
            // ���� �������� ������ "������� + �������������", ������ ��:
            // s.skill.Value = s.skill.Value + s.value;
            s.skill.Value = s.value;
        }
    }
}
