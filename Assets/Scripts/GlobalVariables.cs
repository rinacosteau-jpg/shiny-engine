using System;
using UnityEngine;
using TMPro;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;
using System.Collections;
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

    private int lastPlayerMoralVal;
    private int lastPlayerMoralCap;
    private int lastArticyMoralVal;
    private int lastArticyMoralCap;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        player = new PlayerState(null, false, false);
        player.moralCap = 10;
        player.moralVal = 10;
        Debug.Log("op");
        var selector = FindFirstObjectByType<SkillSelectionUI>(FindObjectsInactive.Include);
        if (selector) {
            Debug.Log("selector");
            selector.Open(player);
        }
        StartCoroutine(DelayOpen());
        IEnumerator DelayOpen() { yield return null; selector.Open(player); }


        if (!setOfKnowledge) setOfKnowledge = GetComponent<TMP_Text>();

        // Subscribe to inventory events to update flags on any change
        InventoryStorage.OnItemCountChanged += OnItemCountChanged;
        InventoryStorage.OnInventoryCleared += OnInventoryCleared;

        // Initial flag calculation on start in case items are already present
        RecalculateFlagsFromInventory();

        ArticyClueSync.SyncFromArticy();

        // Sync moral values from Unity to Articy at start
        SyncMoralToArticy();

        lastPlayerMoralVal = player.moralVal;
        lastPlayerMoralCap = player.moralCap;
        
        lastArticyMoralVal = ArticyGlobalVariables.Default.PS.moralVal;
        lastArticyMoralCap = ArticyGlobalVariables.Default.PS.moralCap;

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
        if (setOfQuests)
            setOfQuests.text = "Quests:\n" + QuestManager.DisplayQuests();
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

    private void SyncMoralToArticy() {
        ArticyGlobalVariables.Default.PS.moralVal = player.moralVal;
        ArticyGlobalVariables.Default.PS.moralCap = player.moralCap;
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

    public void SyncGlobalsToArticy()
    {
        RecalculateFlagsFromInventory();
        SyncMoralToArticy();
        ArticyClueSync.PushTotalScoreToArticy();
        ArticyInventorySync.PushAllCountsToArticy();
    }

    private int GetSkillValue(string name) {
        var field = typeof(PlayerState).GetField($"skill{name}", BindingFlags.Instance | BindingFlags.Public);
        if (field?.GetValue(player) is Skill skill)
            return skill.Value;
        return 0;
    }

    private void Update() {
        var articyMoralVal = ArticyGlobalVariables.Default.PS.moralVal;
        if (articyMoralVal != lastArticyMoralVal) {
            player.moralVal = articyMoralVal;
            if (player.moralVal > player.moralCap) {
                player.moralVal = player.moralCap;
                ArticyGlobalVariables.Default.PS.moralVal = player.moralVal;
            }
            lastArticyMoralVal = player.moralVal;
            lastPlayerMoralVal = player.moralVal;
        }

        var articyMoralCap = ArticyGlobalVariables.Default.PS.moralCap;
        if (articyMoralCap != lastArticyMoralCap) {
            player.moralCap = articyMoralCap;
            if (player.moralCap <= 0) {
                GameOver.Trigger();
            }
            if (player.moralVal > player.moralCap) {
                player.moralVal = player.moralCap;
                ArticyGlobalVariables.Default.PS.moralVal = player.moralVal;
                lastArticyMoralVal = player.moralVal;
                lastPlayerMoralVal = player.moralVal;
            }
            lastArticyMoralCap = articyMoralCap;
            lastPlayerMoralCap = player.moralCap;
        }

        if (player.moralVal != lastPlayerMoralVal) {
            if (player.moralVal > player.moralCap)
                player.moralVal = player.moralCap;
            ArticyGlobalVariables.Default.PS.moralVal = player.moralVal;
            lastPlayerMoralVal = player.moralVal;
            lastArticyMoralVal = player.moralVal;
        }

        if (player.moralCap != lastPlayerMoralCap) {
            if (player.moralCap <= 0) {
                GameOver.Trigger();
            }
            if (player.moralVal > player.moralCap) {
                player.moralVal = player.moralCap;
                ArticyGlobalVariables.Default.PS.moralVal = player.moralVal;
                lastPlayerMoralVal = player.moralVal;
                lastArticyMoralVal = player.moralVal;
            }
            ArticyGlobalVariables.Default.PS.moralCap = player.moralCap;
            lastPlayerMoralCap = player.moralCap;
            lastArticyMoralCap = player.moralCap;
        }

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
