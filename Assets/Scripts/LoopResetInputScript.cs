using Articy.World_Of_Red_Moon.GlobalVariables;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoopResetInputScript : MonoBehaviour {
    [SerializeField] private InputActionReference resetActionReference; // можно оставить пустым — возьмём из InputSystem.actions
    private InputAction resetAction;

    // --- антидубль ---
    private static int s_lastResetFrame = -1;
    private static float s_lastResetTime = -1f;
    private const float kResetDebounceSeconds = 0.05f; // 50 мс, чтобы пережить двойные дергания разными инстансами

    private void Awake() {
        // Подсказка, если в сцене несколько таких компонентов
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
        // Реагируем ровно один раз на нажатие
        if (resetAction != null && resetAction.WasPressedThisFrame()/*&& GlobalVariables.Instance.player.hasGun*/) {
            TryLoopReset();
        }
    }

    // Вызывай ЭТО вместо прямого LoopReset() и из таймера тоже, чтобы всё шло через один предохранитель
    public static void TryLoopReset() {
        // не чаще одного раза за кадр
        if (Time.frameCount == s_lastResetFrame) return;
        // и с коротким кулдауном (на случай двух разных компонентов/систем)
        if (s_lastResetTime >= 0f && (Time.unscaledTime - s_lastResetTime) < kResetDebounceSeconds) return;

        s_lastResetFrame = Time.frameCount;
        s_lastResetTime = Time.unscaledTime;

        DoLoopReset();
    }

    // Вынес тело ресета в статический метод — так любой источник (кнопка, время, триггеры) идёт по одной дороге
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

    public void LoopReset() => TryLoopReset(); // дергает тот же дебаунс

}
