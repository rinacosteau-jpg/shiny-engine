using UnityEngine;
using UnityEngine.InputSystem;

public class LoopResetInputScript : MonoBehaviour {
    private InputAction resetAction;

    void Start() {
        resetAction = InputSystem.actions.FindAction("Reset");

    }

    public void LoopReset() {
        Debug.Log("[LoopReset] start");

        QuestManager.ResetTemporary();

        GameTime.Instance.Hours = 12;
        GameTime.Instance.Minutes = 12;

        // Надёжная проверка наличия артефакта: сначала из GlobalVariables, если он уже жив,
        // иначе — прямо из InventoryStorage (фоллбек).
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

        var monoBehaviours = FindObjectsOfType<MonoBehaviour>(true);
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
    }

    void Update() {
        if (resetAction != null && resetAction.triggered) {
            LoopReset();
        }
    }
}
