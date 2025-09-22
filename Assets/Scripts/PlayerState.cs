using System.Collections.Generic;

public struct PlayerState {
    public HashSet<string> Knowledge;
    public bool hasArtifact;
    public bool hasGun;

    public int moralVal;
    public int moralCap;

    public PlayerState(HashSet<string> knowledge = null, bool hasArtifact = false, bool hasGun = false, int moralVal = 10, int moralCap = 10) {
        Knowledge = knowledge ?? new HashSet<string>();
        this.hasArtifact = hasArtifact;
        this.hasGun = hasGun;
        this.moralVal = moralVal;
        this.moralCap = moralCap;
    }
}
