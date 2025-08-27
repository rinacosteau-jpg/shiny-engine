using System.Collections.Generic;


public struct PlayerState {
    public HashSet<string> Knowledge;
    public bool hasArtifact;
    public bool hasGun;

    public int moralCap;
    public int moralVal;

    public Skill skillPersuasion;
    public Skill skillPerseption;
    public Skill skillAccuracy;

    public PlayerState(HashSet<string> knowledge = null, bool hasArtifact = false, bool hasGun = false, int moralCap = 10, int moralVal = 10) {
        Knowledge = knowledge ?? new HashSet<string>();
        this.hasArtifact = hasArtifact;
        this.hasGun = hasGun;
        this.moralCap = moralCap;
        this.moralVal = moralVal;
        skillPersuasion = new Skill("Persuasion");
        skillPerseption = new Skill("Perseption");
        skillAccuracy = new Skill("Accuracy");
    }
}
