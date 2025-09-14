// Assets/Scripts/UI/Theme/UIButtonStyle.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIButtonStyle : MonoBehaviour {
    public UITheme theme;
    public enum Kind { Primary, Secondary, Ghost }
    public Kind kind = Kind.Primary;

    [Header("Optional refs")]
    public TMP_Text label;
    public Image border; // для Secondary

    void Reset() { label = GetComponentInChildren<TMP_Text>(true); }
    void OnEnable() { Apply(); }
    void OnValidate() { if (Application.isEditor && theme) Apply(); }

    void Apply() {
        var btn = GetComponent<Button>();
        if (!btn || theme == null) return;

        switch (kind) {
            case Kind.Primary: UIThemeApplier.ApplyPrimary(btn, theme, label); break;
            case Kind.Secondary: UIThemeApplier.ApplySecondary(btn, theme, border, label); break;
            case Kind.Ghost: UIThemeApplier.ApplyGhost(btn, theme, label); break;
        }
    }
}
