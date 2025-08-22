using UnityEngine;
using UnityEngine.UI;

public class ScrollbarToScrollRect : MonoBehaviour {
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Scrollbar scrollbar;

    void Awake() {
        // ������ � ��� �������:
        scrollbar.onValueChanged.AddListener(OnScrollbarChanged);
        scrollRect.onValueChanged.AddListener(_ => OnScrollRectChanged());
        // �������������: ��������� �������
        OnScrollRectChanged();
    }

    private void OnScrollbarChanged(float v) {
        // ScrollRect ���������� 1 = ����, 0 = ��� (��� Bottom To Top ������ ���������)
        scrollRect.verticalNormalizedPosition = v;
    }

    private void OnScrollRectChanged() {
        // ������ ���������, ���� ���� ����������� �������
        scrollbar.value = scrollRect.verticalNormalizedPosition;
    }
}
