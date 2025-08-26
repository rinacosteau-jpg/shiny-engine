using System.Collections.Generic;


public struct PlayerState {
    public HashSet<string> Knowledge;
    public bool hasArtifact;
    public bool hasGun;

    public Skill skillPersuasion;
    public Skill skillPerseption;

    public PlayerState(HashSet<string> knowledge = null, bool hasArtifact = false, bool hasGun = false) {
        Knowledge = knowledge ?? new HashSet<string>();
        this.hasArtifact = hasArtifact;
        this.hasGun = hasGun;
        skillPersuasion = new Skill("Persuasion");
        skillPerseption = new Skill("Perseption");
    }
}
