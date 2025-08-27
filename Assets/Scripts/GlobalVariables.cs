using System;
using UnityEngine;
using TMPro;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;
using System.Collections; // <-- проверь свой namespace от Articy

public class GlobalVariables : MonoBehaviour {
    // === IDs уникальных предметов, влияющих на PlayerState ===
    private const string ArtefactId = "InventoryArtefact"; // TechnicalName предмета артефакта
    private const string GunId = "Gun";          // если пистолет тоже идёт через инвентарь

    // === Синглтон ===
    public static GlobalVariables Instance { get; private set; }

    [SerializeField] private DialogueUI dialogueUI; // перетащи в инспекторе

    public void ForceCloseDialogue() => dialogueUI?.CloseDialogue();


    // === Публичное состояние игрока (как у тебя) ===
    public PlayerState player;

    private int _prevPlayerMoralCap;
    private int _prevPlayerMoralVal;
    private int _prevArticyMoralCap;
    private int _prevArticyMoralVal;

    // === UI (по желанию, для дебага/вывода) ===
    [SerializeField] public TMP_Text setOfKnowledge;
    [SerializeField] public TMP_Text setOfQuests;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        player = new PlayerState(null, false, false);
        Debug.Log("op");
        var ps = ArticyGlobalVariables.Default.PS;
        ps.moralCap = player.moralCap;
        ps.moralVal = player.moralVal;
        _prevPlayerMoralCap = player.moralCap;
        _prevPlayerMoralVal = player.moralVal;
        _prevArticyMoralCap = ps.moralCap;
        _prevArticyMoralVal = ps.moralVal;
        var selector = FindFirstObjectByType<SkillSelectionUI>(FindObjectsInactive.Include);
        if (selector) {
            Debug.Log("selector");
            selector.Open(player);
        }
        StartCoroutine(DelayOpen());
        IEnumerator DelayOpen() { yield return null; selector.Open(player); }


        if (!setOfKnowledge) setOfKnowledge = GetComponent<TMP_Text>();

        // Подписки на инвентарь: обновляем флаги при любом изменении
        InventoryStorage.OnItemCountChanged += OnItemCountChanged;
        InventoryStorage.OnInventoryCleared += OnInventoryCleared;

        // Первичный пересчёт флагов на старте (если предметы уже лежат в инвентаре)
        RecalculateFlagsFromInventory();


    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
        InventoryStorage.OnItemCountChanged -= OnItemCountChanged;
        InventoryStorage.OnInventoryCleared -= OnInventoryCleared;
    }

    private void Update() {
        var ps = ArticyGlobalVariables.Default.PS;

        if (ps.moralCap != _prevArticyMoralCap) {
            player.moralCap = ps.moralCap;
            _prevArticyMoralCap = ps.moralCap;
            _prevPlayerMoralCap = player.moralCap;
        } else if (player.moralCap != _prevPlayerMoralCap) {
            ps.moralCap = player.moralCap;
            _prevArticyMoralCap = ps.moralCap;
            _prevPlayerMoralCap = player.moralCap;
        }

        if (ps.moralVal != _prevArticyMoralVal) {
            player.moralVal = ps.moralVal;
            _prevArticyMoralVal = ps.moralVal;
            _prevPlayerMoralVal = player.moralVal;
        } else if (player.moralVal != _prevPlayerMoralVal) {
            ps.moralVal = player.moralVal;
            _prevArticyMoralVal = ps.moralVal;
            _prevPlayerMoralVal = player.moralVal;
        }
    }

    // === Паблик-методы, которые ты уже дергаешь из UI/систем ===

    // Подтянуть знания из Articy (сет NKNW: bool-флаги → KnowledgeManager)
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

    // Подтянуть квесты из Articy и отрисовать список
    public void GetTempObjectives() {
        QuestManager.SyncFromArticy();
        if (setOfQuests)
            setOfQuests.text = "Quests:\n" + QuestManager.DisplayQuests();
    }

    // Применить item_*_delta из ITM → InventoryStorage; синкнуть *_count обратно
    // Полезно вызывать после узлов диалога, которые выдают/забирают предметы
    public void GetItems() {
        ArticyInventorySync.ApplyItemDeltasFromArticy();
        RecalculateFlagsFromInventory(); // чтобы флаги моментально обновились
    }

    // === Внутренняя логика обновления флагов по инвентарю ===

    private void OnItemCountChanged(string id, int count) {
        // Артефакт уникальный: наличие предмета ↔ флаг
        if (id.Equals(ArtefactId, StringComparison.OrdinalIgnoreCase))
            player.hasArtifact = count > 0;

        // Если пистолет тоже хранится в инвентаре — поддержим флаг
        if (id.Equals(GunId, StringComparison.OrdinalIgnoreCase))
            player.hasGun = count > 0;
    }

    private void OnInventoryCleared() {
        // Если инвентарь подчистили (например, при сбросе петли) — сбрасываем флаги предметов
        player.hasArtifact = false;
        player.hasGun = false;
    }

    public void RecalculateFlagsFromInventory() {
        player.hasArtifact = InventoryStorage.Contains(ArtefactId);
        player.hasGun = InventoryStorage.Contains(GunId);


        Debug.Log(player.hasArtifact);
        Debug.Log(player.hasGun);
    }
}
