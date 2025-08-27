using UnityEngine;

public class WhoDisablesMe : MonoBehaviour {
    void OnEnable() { Debug.Log($"[WhoDisablesMe] ENABLED {GetPath(transform)} frame={Time.frameCount}"); }
    void OnDisable() { Debug.Log($"[WhoDisablesMe] DISABLED {GetPath(transform)} frame={Time.frameCount}"); }

    string GetPath(Transform t) {
        var p = t.name;
        while ((t = t.parent) != null) p = t.name + "/" + p;
        return p;
    }
}
