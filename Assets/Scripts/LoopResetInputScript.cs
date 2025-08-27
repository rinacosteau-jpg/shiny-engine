using Articy.World_Of_Red_Moon.GlobalVariables;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoopResetInputScript : MonoBehaviour {
    [SerializeField] private InputActionReference resetActionReference; // ����� �������� ������ � ������ �� InputSystem.actions
    private InputAction resetAction;

    // --- ��������� ---
    private static int s_lastResetFrame = -1;
    private static float s_lastResetTime = -1f;
    private const float kResetDebounceSeconds = 0.05f; // 50 ��, ����� �������� ������� �������� ������� ����������

    private void Awake() {
        // ���������, ���� � ����� ��������� ����� �����������
        var all = FindObjectsOfType<LoopResetInputScript>(true);
        if (all.Length > 1) {
            Debug.LogWarning($"[LoopReset] In scene there are {all.Length} LoopResetInputScript instances. Debounce will prevent double reset.");
        }
    }

    private void OnEnable() {
        resetAction = resetActionReference != null ? resetActionReference.action : InputSystem.actions?.FindAction("Reset");
        resetAction?.Enable();
    }

    private void OnDisable() {
        resetAction?.Disable();
    }

    private void Update() {
        // ��������� ����� ���� ��� �� �������
        if (resetAction != null && resetAction.WasPressedThisFrame()/*&& GlobalVariables.Instance.player.hasGun*/) {
            TryLoopReset();
        }
    }

    // ������� ��� ������ ������� LoopReset() � �� ������� ����, ����� �� ��� ����� ���� ��������������
    public static void TryLoopReset() {
        // �� ���� ������ ���� �� ����
        if (Time.frameCount == s_lastResetFrame) return;
        // � � �������� ��������� (�� ������ ���� ������ �����������/������)
        if (s_lastResetTime >= 0f && (Time.unscaledTime - s_lastResetTime) < kResetDebounceSeconds) return;

        s_lastResetFrame = Time.frameCount;
        s_lastResetTime = Time.unscaledTime;

        DoLoopReset();
    }

    // ����� ���� ������ � ����������� ����� � ��� ����� �������� (������, �����, ��������) ��� �� ����� ������
    private static void DoLoopReset() {
        Debug.Log("[LoopReset] start");

        QuestManager.ResetTemporary();

        GameTime.Instance.Hours = 12;
        GameTime.Instance.Minutes = 12;

        ArticyReset.ResetArticySet("RQUE");
        ArticyReset.ResetArticySet("EVT");
        ArticyReset.ResetArticySet("RFLG");

        bool hasArtefactNow =
            (GlobalVariables.Instance != null && GlobalVariables.Instance.player.hasArtifact)
            || InventoryStorage.Contains("InventoryArtefact");

        Debug.Log($"[LoopReset] GV.Instance={(GlobalVariables.Instance != null)} hasArtifact={GlobalVariables.Instance?.player.hasArtifact} fallbackContains={InventoryStorage.Contains("InventoryArtefact")} -> hasArtefactNow={hasArtefactNow}");

        if (!hasArtefactNow) {
            Debug.Log("[LoopReset] clearing inventory (no artefact)");
            InventoryStorage.Clear();
        } else {
            Debug.Log("[LoopReset] preserving inventory (artefact present)");
        }

        var monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in monoBehaviours) {
            if (mb is ILoopResettable resettable) {
                try {
                    resettable.OnLoopReset();
                } catch (System.Exception e) {
                    Debug.LogWarning($"[LoopReset] OnLoopReset error on {mb.name}: {e}");
                }
            }
        }

        Debug.Log("[LoopReset] end");
        Debug.Log("Articy loopcount: " + ArticyGlobalVariables.Default.PS.loopCounter);
    }

    public void LoopReset() => TryLoopReset(); // ������� ��� �� �������

}
