using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Articy.World_Of_Red_Moon.GlobalVariables;

public class PlayerStatsDisplay : MonoBehaviour {
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private Button showSkillsButton;
    [SerializeField] private TMP_Text showSkillsButtonLabel;

    private bool skillsVisible;

    private void Awake() {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();

        CacheShowSkillsButton();
        UpdateShowSkillsButtonLabel();
    }

    private void Update() {
        UpdateDisplay();
    }

    private void OnDestroy() {
        if (showSkillsButton != null)
            showSkillsButton.onClick.RemoveListener(ToggleSkillsVisibility);
    }

    private void UpdateDisplay() {
        if (targetText == null || GlobalVariables.Instance == null)
            return;

        var player = GlobalVariables.Instance.player;
        int loopState = ArticyGlobalVariables.Default.PS.loopCounter;

        string skillsSection = skillsVisible
            ? "Skills:\n" + BuildSkillsBlock()
            : "Skills: ";

        var displayBuilder = new StringBuilder();
        displayBuilder.AppendFormat("Moral: {0}/{1}\n", player.moralVal, player.moralCap);

        if (loopState >= 2)
            displayBuilder.AppendFormat("Loop: {0}\n", loopState);

        displayBuilder.Append(skillsSection);

        targetText.text = displayBuilder.ToString();
    }

    private void CacheShowSkillsButton() {
        if (showSkillsButton == null)
            showSkillsButton = GetComponentsInChildren<Button>(true)
                .FirstOrDefault(b => b.gameObject.name == "ShowSkills");

        if (showSkillsButton != null) {
            showSkillsButton.onClick.RemoveListener(ToggleSkillsVisibility);
            showSkillsButton.onClick.AddListener(ToggleSkillsVisibility);

            if (showSkillsButtonLabel == null)
                showSkillsButtonLabel = showSkillsButton.GetComponentInChildren<TMP_Text>();
        }
    }

    private void ToggleSkillsVisibility() {
        skillsVisible = !skillsVisible;
        UpdateShowSkillsButtonLabel();
        UpdateDisplay();
    }

    private void UpdateShowSkillsButtonLabel() {
        if (showSkillsButtonLabel != null)
            showSkillsButtonLabel.text = skillsVisible ? "-" : "+";
    }

    private static string BuildSkillsBlock() {
        var ps = ArticyGlobalVariables.Default?.PS;
        if (ps == null)
            return "  —";

        var properties = ps.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(int)
                        && p.GetIndexParameters().Length == 0
                        && p.Name.StartsWith("skill_", System.StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Name)
            .ToList();

        if (properties.Count == 0)
            return "  —";

        var sb = new StringBuilder();
        foreach (var property in properties) {
            int value = (int)property.GetValue(ps);
            sb.Append("  ");
            sb.Append(MakeDisplayName(property.Name));
            sb.Append(": ");
            sb.Append(value);
            sb.Append('\n');
        }

        return sb.ToString().TrimEnd('\n');
    }

    private static string MakeDisplayName(string propertyName) {
        const string prefix = "skill_";
        if (propertyName.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) {
            var tail = propertyName.Substring(prefix.Length);
            if (tail.Length == 0) return propertyName;
            return CapitalizeWords(tail.Replace('_', ' '));
        }
        return propertyName;
    }

    private static string CapitalizeWords(string text) {
        if (string.IsNullOrEmpty(text))
            return text;

        var parts = text.Split(' ');
        for (int i = 0; i < parts.Length; i++) {
            var part = parts[i];
            if (part.Length == 0) continue;
            if (part.Length == 1) {
                parts[i] = char.ToUpperInvariant(part[0]).ToString();
                continue;
            }
            parts[i] = char.ToUpperInvariant(part[0]) + part.Substring(1);
        }
        return string.Join(" ", parts);
    }
}

