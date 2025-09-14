// Assets/Scripts/UI/Theme/UIThemeApplier.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class UIThemeApplier {
    // Primary (красная) кнопка
    public static void ApplyPrimary(Button btn, UITheme t, TMP_Text label = null) {
        if (!btn || t == null) return;

        var cg = btn.targetGraphic as Graphic;
        if (cg) cg.color = t.Red500;

        var cb = btn.colors;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        cb.normalColor = t.Red500;
        cb.highlightedColor = t.Red400;
        cb.pressedColor = t.Red600;
        cb.selectedColor = t.Red500;
        cb.disabledColor = new Color(0.165f, 0.078f, 0.086f, 1f); // #2A1416
        btn.colors = cb;

        if (label == null) label = btn.GetComponentInChildren<TMP_Text>(true);
        if (label) label.color = t.TextOnRed;
    }

    // Secondary (тёмная с бордером)
    public static void ApplySecondary(Button btn, UITheme t, Image border = null, TMP_Text label = null) {
        if (!btn || t == null) return;

        var cg = btn.targetGraphic as Graphic;
        if (cg) cg.color = t.BG1;

        var cb = btn.colors;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        cb.normalColor = t.BG1;
        cb.highlightedColor = t.BG2;
        cb.pressedColor = new Color(0.10f, 0.11f, 0.13f, 1f); // #1A1C22
        cb.selectedColor = t.BG1;
        cb.disabledColor = t.BG1;
        btn.colors = cb;

        if (border) border.color = t.Stroke;
        if (label == null) label = btn.GetComponentInChildren<TMP_Text>(true);
        if (label) label.color = t.TextHigh;
    }

    // Ghost (только текст/прозрачная подложка)
    public static void ApplyGhost(Button btn, UITheme t, TMP_Text label = null) {
        if (!btn || t == null) return;

        var cg = btn.targetGraphic as Graphic;
        if (cg) cg.color = new Color(0, 0, 0, 0);

        var cb = btn.colors;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        cb.normalColor = new Color(0, 0, 0, 0);
        cb.highlightedColor = t.BG2;
        cb.pressedColor = new Color(0.10f, 0.11f, 0.13f, 1f);
        cb.selectedColor = new Color(0, 0, 0, 0);
        cb.disabledColor = new Color(0, 0, 0, 0);
        btn.colors = cb;

        if (label == null) label = btn.GetComponentInChildren<TMP_Text>(true);
        if (label) label.color = t.TextHigh;
    }

    // Текстовые элементы
    public static void ApplyTextHigh(TMP_Text txt, UITheme t) { if (txt) txt.color = t.TextHigh; }
    public static void ApplyTextMid(TMP_Text txt, UITheme t) { if (txt) txt.color = t.TextMid; }
    public static void ApplyTextLow(TMP_Text txt, UITheme t) { if (txt) txt.color = t.TextLow; }

    // Поверхности/панели
    public static void ApplySurface(Image img, UITheme t, int level = 1) {
        if (!img || t == null) return;
        img.color = (level == 0) ? t.BG0 : (level == 2 ? t.BG2 : t.BG1);
    }
}
