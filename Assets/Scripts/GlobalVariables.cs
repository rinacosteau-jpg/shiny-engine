using System;
using UnityEngine;
using TMPro;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;
using System.Reflection;

public class GlobalVariables : MonoBehaviour {
    // IDs of unique items affecting PlayerState
    private const string ArtefactId = "InventoryArtefact"; // Technical name of the artefact item
    private const string GunId = "Gun";          // if the gun is also managed via inventory

    // Singleton
    public static GlobalVariables Instance { get; private set; }

    [SerializeField] private DialogueUI dialogueUI; // assign in inspector

    public void ForceCloseDialogue() => dialogueUI?.CloseDialogue();


    // Public player state
    public PlayerState player;
    

    // Optional UI for debugging / displaying
    [SerializeField] public TMP_Text setOfKnowledge;
    [SerializeField] public TMP_Text setOfQuests;

    private int lastArticyMoralVal;
    private int lastArticyMoralCap;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        player = new PlayerState(null, false, false);
        if (!setOfKnowledge) setOfKnowledge = GetComponent<TMP_Text>();

        // Subscribe to inventory events to update flags on any change
        InventoryStorage.OnItemCountChanged += OnItemCountChanged;
        InventoryStorage.OnInventoryCleared += OnInventoryCleared;

        // Initial flag calculation on start in case items are already present
        RecalculateFlagsFromInventory();

        ArticyClueSync.SyncFromArticy();

        lastArticyMoralVal = ArticyGlobalVariables.Default.PS.moralVal;
        lastArticyMoralCap = ArticyGlobalVariables.Default.PS.moralCap;

        UpdateMoralState(forceCheck: true);

    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
        InventoryStorage.OnItemCountChanged -= OnItemCountChanged;
        InventoryStorage.OnInventoryCleared -= OnInventoryCleared;
    }

    // Public methods used by UI/systems

    // Pull knowledge from Articy (namespace NKNW: bool flags → KnowledgeManager)
    public void GetKnowledge() {
        var knw = ArticyGlobalVariables.Default.NKNW;

        foreach (var prop in typeof(NKNW).GetProperties()) {
            if (prop.PropertyType != typeof(bool)) continue;

            bool value = (bool)prop.GetValue(knw);
            if (value) {
                string name = prop.Name;
                KnowledgeManager.AddKnowledge(name);
            }
        }

        if (setOfKnowledge)
            setOfKnowledge.text = KnowledgeManager.DisplayKnowledges();
    }

    // Pull quests from Articy and render list
    public void GetTempObjectives() {
        QuestManager.SyncFromArticy();
        var journalUI = FindObjectOfType<JournalUI>();
        if (journalUI != null)
            journalUI.Refresh();
        if (setOfQuests)
            setOfQuests.text = string.Empty;
    }

    // Apply item_*_delta from ITM → InventoryStorage; sync *_count back
    // Useful to call after dialogue nodes that give or remove items
    public void GetItems() {
        ArticyInventorySync.ApplyItemDeltasFromArticy();
        ArticyClueSync.SyncFromArticy();
        RecalculateFlagsFromInventory(); // update flags immediately
    }

    // Internal logic for updating flags based on inventory

    private void OnItemCountChanged(string id, int count) {
        // Artefact is unique: presence of item ↔ flag
        if (id.Equals(ArtefactId, StringComparison.OrdinalIgnoreCase))
            player.hasArtifact = count > 0;

        // If gun is also stored in inventory, support the flag
        if (id.Equals(GunId, StringComparison.OrdinalIgnoreCase))
            player.hasGun = count > 0;
    }

    private void OnInventoryCleared() {
        // If inventory was cleared (e.g., loop reset) reset item flags
        player.hasArtifact = false;
        player.hasGun = false;
    }

    public void RecalculateFlagsFromInventory() {
        player.hasArtifact = InventoryStorage.Contains(ArtefactId);
        player.hasGun = InventoryStorage.Contains(GunId);

        Debug.Log(player.hasArtifact);
        Debug.Log(player.hasGun);
    }

    private void UpdateMoralState(bool forceCheck = false) {
        var ps = ArticyGlobalVariables.Default?.PS;
        if (ps == null)
            return;

        int currentVal = ps.moralVal;
        int currentCap = ps.moralCap;

        bool reachedZero = currentVal <= 0 || currentCap <= 0;

        if (!forceCheck && currentVal == lastArticyMoralVal && currentCap == lastArticyMoralCap) {
            if (reachedZero)
                GameOver.Trigger();
            return;
        }

        int clampedCap = Mathf.Max(0, currentCap);
        if (clampedCap != currentCap)
            ps.moralCap = clampedCap;

        int clampedVal = Mathf.Clamp(currentVal, 0, clampedCap);
        if (clampedVal != currentVal)
            ps.moralVal = clampedVal;

        lastArticyMoralCap = ps.moralCap;
        lastArticyMoralVal = ps.moralVal;

        if (ps.moralVal <= 0 || ps.moralCap <= 0)
            GameOver.Trigger();
    }

    private void ResolveSkillChecks() {
        var schProperty = typeof(ArticyGlobalVariables).GetProperty("SCH");
        if (schProperty == null)
            return;

        var sch = schProperty.GetValue(ArticyGlobalVariables.Default);
        if (sch == null)
            return;

        foreach (var prop in sch.GetType().GetProperties()) {
            if (prop.PropertyType != typeof(int))
                continue;

            int value = (int)prop.GetValue(sch);
            if (value == -1) {
                int roll = UnityEngine.Random.Range(1, 101);
                int skillValue = GetSkillValue(prop.Name);
                int total = roll + skillValue;
                Debug.Log(total);
                Debug.Log(ArticyGlobalVariables.Default.SCH.Accuracy);
                prop.SetValue(sch, total);
                Debug.Log(ArticyGlobalVariables.Default.SCH.Accuracy);
            }
        }
    }

    private int GetSkillValue(string name) {
        var ps = ArticyGlobalVariables.Default?.PS;
        if (ps == null)
            return 0;

        var property = ps.GetType().GetProperty($"skill_{name}", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property != null && property.PropertyType == typeof(int) && property.GetIndexParameters().Length == 0)
            return (int)property.GetValue(ps);
        return 0;
    }

    private void Update() {
        UpdateMoralState();

        ResolveSkillChecks();

        // Reflectively check for the Articy flag RFLG.kotIdentify. If it's set, reset it
        // and mark all inventory items as identified.
        var rflgObj = typeof(ArticyGlobalVariables).GetProperty("RFLG")?.GetValue(ArticyGlobalVariables.Default);
        var kotIdentifyProp = rflgObj?.GetType().GetProperty("kotIdentify");
        if (kotIdentifyProp != null && kotIdentifyProp.GetValue(rflgObj) is bool flag && flag) {
            kotIdentifyProp.SetValue(rflgObj, false);
            InventoryStorage.IdentifyAll();
        }
    }
}
