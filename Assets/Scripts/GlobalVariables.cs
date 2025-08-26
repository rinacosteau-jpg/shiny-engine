using System;
using UnityEngine;
using TMPro;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables; // <-- проверь свой namespace от Articy

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

    // === UI (по желанию, для дебага/вывода) ===
    [SerializeField] public TMP_Text setOfKnowledge;
    [SerializeField] public TMP_Text setOfQuests;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        player = new PlayerState();
        player.skillPerseption.Value = 10;
        player.skillPersuasion.Value = 10;

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
