using System.Collections.Generic;

public struct KnowledgeItem
{
    public string Name;
    public string Description;
    public int Hash;

    public KnowledgeItem(string name, string description)
    {
        Name = name;
        Description = description;
        Hash = name.GetHashCode();
    }
}

public struct PlayerState
{
    public Dictionary<int, KnowledgeItem> Knowledge;
    public bool hasArtifact;
    public bool hasGun;

    public PlayerState(Dictionary<int, KnowledgeItem> knowledge = null, bool hasArtifact = false, bool hasGun = false)
    {
        Knowledge = knowledge ?? new Dictionary<int, KnowledgeItem>();
        this.hasArtifact = hasArtifact;
        this.hasGun = hasGun;
    }
}
