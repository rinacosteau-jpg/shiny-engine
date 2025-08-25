using System.Collections.Generic;
using Articy.World_Of_Red_Moon.GlobalVariables;

public struct PlayerState {
    public HashSet<string> Knowledge;
    public bool hasArtifact;
    public bool hasGun;

    private int _skillPersuasion;
    private int _skillPerseption;

    public int skillPersuasion {
        get => _skillPersuasion;
        set {
            _skillPersuasion = value;
            ArticyGlobalVariables.Default.PS.skill_Persuasion = value;
        }
    }

    public int skillPerseption {
        get => _skillPerseption;
        set {
            _skillPerseption = value;
            ArticyGlobalVariables.Default.PS.skill_Perseption = value;
        }
    }

    public PlayerState(HashSet<string> knowledge = null, bool hasArtifact = false, bool hasGun = false, int skillPersuasion = 0, int skillPerseption = 0) {
        Knowledge = knowledge ?? new HashSet<string>();
        this.hasArtifact = hasArtifact;
        this.hasGun = hasGun;
        _skillPersuasion = skillPersuasion;
        _skillPerseption = skillPerseption;
        ArticyGlobalVariables.Default.PS.skill_Persuasion = skillPersuasion;
        ArticyGlobalVariables.Default.PS.skill_Perseption = skillPerseption;
    }
}
