using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LoopResetInputScript : MonoBehaviour {
    private InputAction resetAction;
    private ILoopResettable[] resettableObjects;

    void Start() {
        resetAction = InputSystem.actions.FindAction("Reset");

        var monoBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        var list = new List<ILoopResettable>();
        foreach (var mb in monoBehaviours) {
            if (mb is ILoopResettable resettable) {
                list.Add(resettable);
            }
        }
        resettableObjects = list.ToArray();
    }

    public void LoopReset()
    {
        foreach (var resettable in resettableObjects) {
            resettable.OnLoopReset();
        }

        InventoryStorage.Clear();
    }

    void Update() {
        if (resetAction != null && resetAction.triggered) {
            LoopReset();
        }
    }
}
