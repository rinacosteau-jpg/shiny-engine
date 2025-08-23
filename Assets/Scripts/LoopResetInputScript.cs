using UnityEngine;
using UnityEngine.InputSystem;

public class LoopResetInputScript : MonoBehaviour {
    private InputAction resetAction;

    void Start() {
        resetAction = InputSystem.actions.FindAction("Reset");

    }

    public void LoopReset()
    {
        var monoBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in monoBehaviours) {
            if (mb is ILoopResettable resettable) {
                resettable.OnLoopReset();
            }
        }

        InventoryStorage.Clear();
    }

    void Update() {
        if (resetAction != null && resetAction.triggered) {
            LoopReset();
        }
    }
}
