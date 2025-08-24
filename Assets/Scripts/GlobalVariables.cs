using System.Reflection;
using UnityEngine;
using Articy.Unity.Interfaces;
using Articy.World_Of_Red_Moon.GlobalVariables;
using Unity.VisualScripting;
using TMPro;

public class GlobalVariables : MonoBehaviour {
    public PlayerState player;

    [SerializeField] public TMP_Text setOfKnowledge;
    [SerializeField] public TMP_Text setOfQuests;

    public static GlobalVariables Instance { get; private set; }
    void Awake() {
        Instance = this;
        if (!setOfKnowledge) setOfKnowledge = GetComponent<TMP_Text>();
    }



    public void GetKnowledge() {

        var knw = ArticyGlobalVariables.Default.NKNW;

        string knowledgeDisplay = "Knowledge: ";

        foreach (var prop in typeof(NKNW).GetProperties()) {
            if (prop.PropertyType == typeof(bool)) {
                Debug.Log("has one of them");
                bool value = (bool)prop.GetValue(knw);
                if (value) {
                    string name = prop.Name;
                    KnowledgeManager.AddKnowledge(name);
                }
            }
        }

        setOfKnowledge.text = KnowledgeManager.DisplayKnowledges();
    }

    public void GetTempObjectives() {

        /*var rque = ArticyGlobalVariables.Default.RQUE;

        string questDisplay = "Quests: ";

        foreach (var prop in typeof(RQUE).GetProperties()) {
            if (prop.PropertyType == typeof(bool)) {
                Debug.Log("has one of them");
                bool value = (bool)prop.GetValue(rque);
                if (value) {
                    string name = prop.Name;
                  //  QuestManager.AddQuest(name, true);
                }
            }
        }

        setOfQuests.text = QuestManager.DisplayQuests();*/
        QuestManager.SyncFromArticy();                   // подтянуть из Articy
        setOfQuests.text = "Quests:\n" + QuestManager.DisplayQuests();  // показать
    }
}
