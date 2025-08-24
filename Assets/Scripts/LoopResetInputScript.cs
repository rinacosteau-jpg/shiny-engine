using UnityEngine;
using UnityEngine.InputSystem;

public class LoopResetInputScript : MonoBehaviour {
    private InputAction resetAction;

    void Start() {
        resetAction = InputSystem.actions.FindAction("Reset");
    }

    public void LoopReset() {
        QuestManager.ResetTemporary();

        GameTime.Instance.Hours = 12;
        GameTime.Instance.Minutes = 12;

        ArticyReset.ResetRQUE();
        ArticyReset.ResetEVT();

        bool hasArtefactNow =
            (GlobalVariables.Instance != null && GlobalVariables.Instance.player.hasArtifact)
            || InventoryStorage.Contains("InventoryArtefact");

        if (!hasArtefactNow) {
            InventoryStorage.Clear();
        }

        var monoBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in monoBehaviours) {
            if (mb is ILoopResettable resettable) {
                try {
                    resettable.OnLoopReset();
                } catch (System.Exception e) {
                    // Debug.LogWarning($"[LoopReset] OnLoopReset error on {mb.name}: {e}");
                }
            }
        }
    }

    void Update() {
        if (resetAction != null && resetAction.triggered) {
            LoopReset();
        }
    }
}
