using UnityEngine;
using Unity.Cinemachine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CinemachineCamera))]
public class OrthoWheelZoom : MonoBehaviour {
    public float minSize = 6f;
    public float maxSize = 14f;
    public float sensitivity = 1.0f;   // 1.0�2.0 ������ �������
    public float smooth = 0.08f;        // ��� �� �������� �������

    CinemachineCamera cam;
    float target;

    void Awake() {
        cam = GetComponent<CinemachineCamera>();
        target = cam.Lens.OrthographicSize;
    }

    void Update() {
        float scroll = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) scroll = Mouse.current.scroll.ReadValue().y; // �������
#else
        scroll = Input.mouseScrollDelta.y;
#endif

        if (Mathf.Abs(scroll) > 0.01f)
            target = Mathf.Clamp(target - scroll * sensitivity * 0.05f, minSize, maxSize);

        var lens = cam.Lens; // LensSettings � struct, ����� ������� �������
        // ����������� ��� ������
        float t = 1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(0.0001f, smooth));
        lens.OrthographicSize = Mathf.Lerp(lens.OrthographicSize, target, t);
        cam.Lens = lens;
    }
}
