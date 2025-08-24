using UnityEngine;
using UnityEngine.InputSystem;

public class LoopResetInputScript : MonoBehaviour {
    private InputAction resetAction;

    void Start() {
        resetAction = InputSystem.actions.FindAction("Reset");

    }

    public void LoopReset()
    {
        QuestManager.ResetTemporary();

        GameTime.Instance.Hours = 12;
        GameTime.Instance.Minutes = 12;

        if (GlobalVariables.Instance == null || !GlobalVariables.Instance.player.hasArtifact)
        {
            InventoryStorage.Clear();
        }

        var monoBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in monoBehaviours) {
            if (mb is ILoopResettable resettable) {
                resettable.OnLoopReset();
            }
        }

    }

    void Update() {
        if (resetAction != null && resetAction.triggered) {
            LoopReset();
        }
    }
}
