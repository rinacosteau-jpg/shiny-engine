using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIPanelStyle : MonoBehaviour {
    public UITheme theme;
    public enum Level { BG0, BG1, BG2 }
    public Level level = Level.BG1;

    [Header("Images")]
    public Image background;      // если пусто Ч возьмЄм Image на этом объекте
    public Image optionalBorder;  // не об€зателен

    void Reset() { background = GetComponent<Image>(); }
    void OnEnable() { Apply(); }
    void OnValidate() { if (Application.isEditor && isActiveAndEnabled) Apply(); }

    public void Apply() {
        if (!theme) return;

        if (!background) background = GetComponent<Image>();
        if (background) {
            background.color = level switch {
                Level.BG0 => theme.BG0,
                Level.BG1 => theme.BG1,
                Level.BG2 => theme.BG2,
                _ => theme.BG1
            };
        }

        if (optionalBorder)
            optionalBorder.color = theme.Stroke;
    }
}
