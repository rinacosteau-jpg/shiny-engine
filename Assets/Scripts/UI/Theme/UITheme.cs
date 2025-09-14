// Assets/Scripts/UI/Theme/UITheme.cs
using UnityEngine;

[CreateAssetMenu(fileName = "UITheme_Red", menuName = "Theme/UI Theme (Red)")]
public class UITheme : ScriptableObject {
    [Header("Neutrals")]
    public Color BG0 = Hex("#0D0E10");
    public Color BG1 = Hex("#15171B");
    public Color BG2 = Hex("#1E2026");
    public Color Stroke = Hex("#2E323A");
    public Color TextHigh = Hex("#F2F3F5");
    public Color TextMid = Hex("#C4C9D1");
    public Color TextLow = Hex("#8B9098");

    [Header("Primary Red")]
    public Color Red700 = Hex("#7E0E14");
    public Color Red600 = Hex("#9E1118");
    public Color Red500 = Hex("#B5161E");
    public Color Red400 = Hex("#C91F27");
    public Color Red300 = Hex("#E04B51");
    public Color Red100 = Hex("#F6D2D4");
    public Color TextOnRed = Hex("#FFF4F4");

    [Header("Statuses")]
    public Color Success = Hex("#2BBEA8");
    public Color Warning = Hex("#E6B450");
    public Color Info = Hex("#3A7BD5");

    [Header("Faction Violet (sparingly)")]
    public Color Violet700 = Hex("#4E2C9A");
    public Color Violet500 = Hex("#6E4ACB");
    public Color Violet300 = Hex("#9A7EF0");

    static Color Hex(string hex) {
        Color c; ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }
}
