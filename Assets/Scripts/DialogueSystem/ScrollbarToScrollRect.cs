using UnityEngine;
using UnityEngine.UI;

public class ScrollbarToScrollRect : MonoBehaviour {
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Scrollbar scrollbar;

    void Awake() {
        // Связка в обе стороны:
        scrollbar.onValueChanged.AddListener(OnScrollbarChanged);
        scrollRect.onValueChanged.AddListener(_ => OnScrollRectChanged());
        // Инициализация: выровнять позиции
        OnScrollRectChanged();
    }

    private void OnScrollbarChanged(float v) {
        // ScrollRect использует 1 = верх, 0 = низ (при Bottom To Top обычно совпадает)
        scrollRect.verticalNormalizedPosition = v;
    }

    private void OnScrollRectChanged() {
        // Держим синхронно, если юзер проскроллит колесом
        scrollbar.value = scrollRect.verticalNormalizedPosition;
    }
}
