using Articy.World_Of_Red_Moon.GlobalVariables;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player input that triggers a time-loop reset and coordinates the reset sequence.
/// Debounces inputs to avoid multiple resets within the same frame or a very short interval.
/// </summary>
public class LoopResetInputScript : MonoBehaviour {
    [SerializeField] private InputActionReference resetActionReference; // Input action for loop reset; falls back to action named "Reset".
    private InputAction resetAction;

    // Debounce state: prevent duplicate resets triggered too close together.
    private static int s_lastResetFrame = -1;
    private static float s_lastResetTime = -1f;
    private const float kResetDebounceSeconds = 0.05f; // 50 ms debounce window.

    private void Awake() {
        // Warn if the scene has more than one instance; debounce still prevents double reset.
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
        // Listen for the reset input press this frame, then attempt reset.
        if (resetAction != null && resetAction.WasPressedThisFrame()) {
            TryLoopReset();
        }
    }

    /// <summary>
    /// Public entry point for initiating the loop reset with debounce protection.
    /// Safe to call from code or UnityEvents/UI.
    /// </summary>
    public static void TryLoopReset() {
        // Guard 1: already reset this frame.
        if (Time.frameCount == s_lastResetFrame) return;
        // Guard 2: within debounce window.
        if (s_lastResetTime >= 0f && (Time.unscaledTime - s_lastResetTime) < kResetDebounceSeconds) return;

        s_lastResetFrame = Time.frameCount;
        s_lastResetTime = Time.unscaledTime;

        DoLoopReset();
    }

    /// <summary>
    /// Performs the actual reset: resets quests, time-of-day, Articy state, and inventory,
    /// then broadcasts OnLoopReset to all objects implementing ILoopResettable.
    /// </summary>
    private static void DoLoopReset() {
        Debug.Log("[LoopReset] start");

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
            Debug.Log("[LoopReset] clearing inventory of non-clues (no artefact)");
            InventoryStorage.Clear(removeClues: false);
        } else {
            Debug.Log("[LoopReset] preserving inventory (artefact present)");
        }

        QuestManager.OnLoopReset();

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

    /// <summary>
    /// Inspector/UnityEvent hook to trigger a reset.
    /// </summary>
    public void LoopReset() => TryLoopReset();

}

