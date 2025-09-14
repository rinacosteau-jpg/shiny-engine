using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class UITextStyle : MonoBehaviour {
    public UITheme theme;
    public enum Kind { High, Mid, Low, OnRed }
    public Kind kind = Kind.High;

    [Header("Optional")]
    public TMP_Text target;

    void Reset() { target = GetComponent<TMP_Text>(); }
    void OnEnable() { Apply(); }
    void OnValidate() { if (Application.isEditor && isActiveAndEnabled) Apply(); }

    public void Apply() {
        if (!theme) return;
        if (!target) target = GetComponent<TMP_Text>();
        if (!target) return;

        switch (kind) {
            case Kind.High: target.color = theme.TextHigh; break;
            case Kind.Mid: target.color = theme.TextMid; break;
            case Kind.Low: target.color = theme.TextLow; break;
            case Kind.OnRed: target.color = theme.TextOnRed; break;
        }
    }
}
