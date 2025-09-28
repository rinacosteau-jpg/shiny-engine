using System.Collections.Generic;
using Articy.World_Of_Red_Moon.GlobalVariables;

public struct PlayerState {
    public HashSet<string> Knowledge;
    public bool hasArtifact;
    public bool hasGun;

    public int moralVal {
        readonly get {
            var ps = ArticyGlobalVariables.Default?.PS;
            return ps != null ? ps.moralVal : 0;
        }
        set {
            var ps = ArticyGlobalVariables.Default?.PS;
            if (ps == null)
                return;

            ps.moralVal = value;
        }
    }

    public int moralCap {
        readonly get {
            var ps = ArticyGlobalVariables.Default?.PS;
            return ps != null ? ps.moralCap : 0;
        }
        set {
            var ps = ArticyGlobalVariables.Default?.PS;
            if (ps == null)
                return;

            ps.moralCap = value;
        }
    }

    public PlayerState(HashSet<string> knowledge = null, bool hasArtifact = false, bool hasGun = false) {
        Knowledge = knowledge ?? new HashSet<string>();
        this.hasArtifact = hasArtifact;
        this.hasGun = hasGun;
    }
}
