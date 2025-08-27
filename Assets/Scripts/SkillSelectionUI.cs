using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSelectionUI : MonoBehaviour {
    [Header("UI refs")]
    [SerializeField] private TMP_Text pointsLeftText;     // "���������� Skillpoints"
    [SerializeField] private Button okButton;             // ������� ������ ����� pointsLeft == 0
    [SerializeField] private RectTransform slotContainer; // ��������� �� VerticalLayoutGroup
    [SerializeField] private RectTransform slotTemplate;  // ������������ ������ ����� (�������������)

    [Header("Config")]
    [SerializeField] private int startPoints = 10;

    // ���������� ������ �����
    private class RuntimeSlot {
        public string fieldName;
        public Skill skill;
        public int value; // �������� ������������� ���� (� ����)

        // ui
        public TMP_Text nameText;
        public TMP_Text valueText;
        public Button plusButton;
        public Button minusButton;
        public GameObject go;
    }

    private readonly List<RuntimeSlot> _slots = new();
    private int pointsLeft;

    private void Awake() {
        // ��������� ����������/������� ���� �� ��������� ������
        if (!slotContainer) {
            var vg = GetComponentInChildren<VerticalLayoutGroup>(true);
            if (vg) slotContainer = vg.GetComponent<RectTransform>();
        }
        if (!slotTemplate && slotContainer && slotContainer.childCount > 0) {
            // ������������, ��� ������/������������ ������ � ������
            slotTemplate = slotContainer.GetChild(0) as RectTransform;
        }

        if (okButton) okButton.onClick.AddListener(Confirm);

        // ���� ������ �� ���������
        gameObject.SetActive(false);

        // �� ������ � ��������, ��� ������ ��������
        if (slotTemplate) slotTemplate.gameObject.SetActive(false);
    }

    /// <summary>
    /// ������� ����, ������������� ����� ��� ��� Skill-���� �� PlayerState.
    /// </summary>
    public void Open(PlayerState player, int? pointsOverride = null) {
        if (!slotContainer || !slotTemplate) {
            Debug.LogError("[SkillSelectionUI] �� ����� slotContainer ��� slotTemplate.");
            return;
        }

        // �����
        foreach (Transform child in slotContainer) {
            if (child == slotTemplate) continue;
            Destroy(child.gameObject);
        }
        _slots.Clear();

        pointsLeft = pointsOverride ?? startPoints;

        // ������� ��� public instance-���� ���� Skill
        var fields = typeof(PlayerState)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(Skill))
            .OrderBy(f => f.Name) // ���������� �������
            .ToList();

        foreach (var f in fields) {
            var skillObj = f.GetValue(player) as Skill;
            if (skillObj == null) continue;

            var displayName = MakeDisplayName(f.Name); // �������� skillPerseption -> Perseption
            var slot = CreateSlot(displayName, skillObj);
            _slots.Add(slot);
        }

        UpdateUI();
        gameObject.SetActive(true);
    }

    private RuntimeSlot CreateSlot(string displayName, Skill skill) {
        var go = Instantiate(slotTemplate, slotContainer);
        go.gameObject.name = $"Slot_{displayName}";
        go.gameObject.SetActive(true);

        // ���� �������� �� ������������� ������
        TMP_Text nameText = FindIn<TMP_Text>(go, "Name");
        TMP_Text valueText = FindIn<TMP_Text>(go, "Value");
        Button btnPlus = FindIn<Button>(go, "Plus");
        Button btnMinus = FindIn<Button>(go, "Minus");

        if (nameText) nameText.text = displayName;
        if (valueText) valueText.text = "0";

        var slot = new RuntimeSlot {
            fieldName = displayName,
            skill = skill,
            value = 0,
            nameText = nameText,
            valueText = valueText,
            plusButton = btnPlus,
            minusButton = btnMinus,
            go = go.gameObject
        };

        if (btnPlus) btnPlus.onClick.AddListener(() => Change(slot, +1));
        if (btnMinus) btnMinus.onClick.AddListener(() => Change(slot, -1));

        return slot;
    }

    private static T FindIn<T>(RectTransform root, string childName) where T : Component {
        var t = root.Find(childName);
        if (t) return t.GetComponent<T>();
        // �������� ������� � ����� �� ���� ��������
        return root.GetComponentsInChildren<T>(true)
                   .FirstOrDefault(c => string.Equals(c.gameObject.name, childName, StringComparison.Ordinal));
    }

    private void Change(RuntimeSlot s, int delta) {
        if (delta > 0 && pointsLeft == 0) return;
        if (delta < 0 && s.value == 0) return;

        s.value += delta;
        pointsLeft -= delta;

        if (s.valueText) s.valueText.text = s.value.ToString();
        UpdateUI();
    }

    private void UpdateUI() {
        if (pointsLeftText) pointsLeftText.text = pointsLeft.ToString();
        if (okButton) okButton.interactable = (pointsLeft == 0);

        foreach (var s in _slots) {
            if (s.plusButton) s.plusButton.interactable = pointsLeft > 0;
            if (s.minusButton) s.minusButton.interactable = s.value > 0;
        }
    }

    public void Confirm() {
        if (pointsLeft != 0) return; // �� ������

        foreach (var s in _slots) {
            if (s.skill != null) {
                // ������� ������: ������� = ������������� ����
                // ���� ������ ���������� � ��������, ������ ��: s.skill.Value += s.value;
                s.skill.Value = s.value;
            }
        }

        gameObject.SetActive(false);
    }

    // "skillPerseption" -> "Perseption", "skillPersuasion" -> "Persuasion"
    private static string MakeDisplayName(string fieldName) {
        const string prefix = "skill";
        if (fieldName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
            var tail = fieldName.Substring(prefix.Length);
            if (tail.Length == 0) return fieldName;
            // ������ ������ ����� ���������
            return char.ToUpperInvariant(tail[0]) + tail.Substring(1);
        }
        // ��������� CamelCase �� ����� (���� �����)
        return SplitCamel(fieldName);
    }

    private static string SplitCamel(string s) {
        if (string.IsNullOrEmpty(s)) return s;
        var result = new System.Text.StringBuilder(s.Length + 8);
        result.Append(char.ToUpperInvariant(s[0]));
        for (int i = 1; i < s.Length; i++) {
            var ch = s[i];
            if (char.IsUpper(ch)) result.Append(' ');
            result.Append(ch);
        }
        return result.ToString();
    }
}
