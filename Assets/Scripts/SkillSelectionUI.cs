using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSelectionUI : MonoBehaviour {
    [Header("UI refs")]
    [SerializeField] private TMP_Text pointsLeftText;
    [SerializeField] private Button okButton;
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private RectTransform slotTemplate;

    [Header("Config")]
    [SerializeField] private int startPoints = 10;
    [SerializeField] private bool forceTopCanvas = true;     // поднять на отдельный Canvas поверх
    [SerializeField] private int topSortingOrder = 5000;
    [SerializeField] private bool activateParentsIfInactive = true; // включать родителей, если выключены

    private class RuntimeSlot {
        public string fieldName;
        public Skill skill;
        public int value;
        public TMP_Text nameText;
        public TMP_Text valueText;
        public Button plusButton;
        public Button minusButton;
        public GameObject go;
    }

    private readonly List<RuntimeSlot> _slots = new();
    private int pointsLeft;
    private bool _setupDone;
    private bool _okWired;
    private CanvasGroup _cg;

    private void Awake() {
        EnsureSetup();
        // ВАЖНО: остаёмся активными, прячем визуально
        HideImmediate();
    }

    private void OnEnable() { Debug.Log("[SkillSelectionUI] OnEnable"); }
    private void OnDisable() { Debug.Log("[SkillSelectionUI] OnDisable"); }

    private void EnsureSetup() {
        if (_setupDone) return;

        if (!slotContainer) {
            var vg = GetComponentsInChildren<VerticalLayoutGroup>(true).FirstOrDefault();
            if (vg) slotContainer = vg.GetComponent<RectTransform>();
        }

        if (!slotTemplate && slotContainer) {
            slotTemplate = TryFindTemplateFrom(slotContainer);
        }

        if (slotTemplate && slotTemplate.gameObject.activeSelf)
            slotTemplate.gameObject.SetActive(false);

        if (okButton && !_okWired) {
            okButton.onClick.AddListener(Confirm);
            _okWired = true;
        }

        if (!_cg) _cg = GetComponent<CanvasGroup>();
        if (!_cg) _cg = gameObject.AddComponent<CanvasGroup>();

        if (forceTopCanvas) {
            var cv = GetComponent<Canvas>();
            if (!cv) cv = gameObject.AddComponent<Canvas>();
            cv.overrideSorting = true;
            cv.sortingOrder = topSortingOrder;
            if (!GetComponent<GraphicRaycaster>()) gameObject.AddComponent<GraphicRaycaster>();
        }

        _setupDone = slotContainer && slotTemplate;
        Debug.Log($"[SkillSelectionUI] Setup: container={(slotContainer ? slotContainer.name : "<null>")}, template={(slotTemplate ? slotTemplate.name : "<null>")}, ok={(okButton ? okButton.name : "<null>")}, ready={_setupDone}");
    }

    private RectTransform TryFindTemplateFrom(RectTransform container) {
        for (int i = 0; i < container.childCount; i++) {
            var child = container.GetChild(i) as RectTransform;
            if (!child) continue;
            bool hasAll = child.Find("Name") && child.Find("Minus") && child.Find("Value") && child.Find("Plus");
            if (hasAll) return child;
        }
        for (int i = 0; i < container.childCount; i++) {
            var child = container.GetChild(i) as RectTransform;
            if (child && !child.gameObject.activeSelf) return child;
        }
        return container.childCount > 0 ? container.GetChild(0) as RectTransform : null;
    }

    /// Показать окно и сгенерировать слоты.
    public void Open(PlayerState player, int? pointsOverride = null) {
        EnsureSetup();
        if (!_setupDone) {
            Debug.LogError("[SkillSelectionUI] slotContainer/slotTemplate не заданы.");
            return;
        }

        // Активируем родительскую цепочку при необходимости
        if (activateParentsIfInactive) ActivateParentsChain();

        // Очистка
        foreach (Transform child in slotContainer) {
            if (child == slotTemplate) continue;
            Destroy(child.gameObject);
        }
        _slots.Clear();

        pointsLeft = pointsOverride ?? startPoints;

        var fields = typeof(PlayerState)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(Skill))
            .OrderBy(f => f.Name)
            .ToList();

        foreach (var f in fields) {
            var skillObj = f.GetValue(player) as Skill;
            if (skillObj == null) {
                Debug.LogWarning($"[SkillSelectionUI] Поле {f.Name} = null");
                continue;
            }
            var displayName = MakeDisplayName(f.Name);
            var slot = CreateSlot(displayName, skillObj);
            _slots.Add(slot);
        }

        UpdateUI();
        ShowImmediate();                    // ← только CanvasGroup
        transform.SetAsLastSibling();       // поверх соседей
        Canvas.ForceUpdateCanvases();

        Debug.Log($"[SkillSelectionUI] Open: slots={_slots.Count}, pointsLeft={pointsLeft}, activeSelf={gameObject.activeSelf}, inHierarchy={gameObject.activeInHierarchy}");
    }

    private void ShowImmediate() {
        _cg.alpha = 1f; _cg.interactable = true; _cg.blocksRaycasts = true;
    }

    private void HideImmediate() {
        _cg.alpha = 0f; _cg.interactable = false; _cg.blocksRaycasts = false;
    }

    private void ActivateParentsChain() {
        var t = transform;
        while (t) {
            if (!t.gameObject.activeSelf) {
                t.gameObject.SetActive(true);
                Debug.Log($"[SkillSelectionUI] Activated parent: {t.name}");
            }
            t = t.parent;
        }
    }

    private RuntimeSlot CreateSlot(string displayName, Skill skill) {
        var go = Instantiate(slotTemplate, slotContainer);
        go.gameObject.name = $"Slot_{displayName}";
        go.gameObject.SetActive(true);

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
        if (pointsLeft != 0) return;
        foreach (var s in _slots) {
            if (s.skill != null)
                s.skill.Value = s.value; // или += s.value
        }
        HideImmediate(); // только прячем, не выключаем GO
        Debug.Log("[SkillSelectionUI] Confirm → apply & hide (CG)");
    }

    private static string MakeDisplayName(string fieldName) {
        const string prefix = "skill";
        if (fieldName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
            var tail = fieldName.Substring(prefix.Length);
            if (tail.Length == 0) return fieldName;
            return char.ToUpperInvariant(tail[0]) + tail.Substring(1);
        }
        return SplitCamel(fieldName);
    }

    private static string SplitCamel(string s) {
        if (string.IsNullOrEmpty(s)) return s;
        var sb = new System.Text.StringBuilder(s.Length + 8);
        sb.Append(char.ToUpperInvariant(s[0]));
        for (int i = 1; i < s.Length; i++) {
            var ch = s[i];
            if (char.IsUpper(ch)) sb.Append(' ');
            sb.Append(ch);
        }
        return sb.ToString();
    }
}
