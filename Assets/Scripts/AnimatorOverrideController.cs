using UnityEngine;
using System.Collections.Generic;

public class AnimatorOverrider : MonoBehaviour {
    [SerializeField] private Animator anim;
    [SerializeField] private AnimationClip newIdle;
    [SerializeField] private AnimationClip newRun;

    void Start() {
        var baseCtrl = Resources.Load<RuntimeAnimatorController>("BaseController");
        var aoc = new AnimatorOverrideController(baseCtrl);

        var list = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        aoc.GetOverrides(list);

        for (int i = 0; i < list.Count; i++) {
            var original = list[i].Key;
            if (original.name == "Idle") list[i] = new(original, newIdle);
            if (original.name == "Run") list[i] = new(original, newRun);
        }

        aoc.ApplyOverrides(list);
        anim.runtimeAnimatorController = aoc;
    }
}
