using TMPro;
using UnityEngine;
using Articy.World_Of_Red_Moon.GlobalVariables;

public class PlayerStatsDisplay : MonoBehaviour {
    [SerializeField] private TMP_Text targetText;

    private void Awake() {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
    }

    private void Update() {
        if (targetText == null || GlobalVariables.Instance == null)
            return;

        var player = GlobalVariables.Instance.player;
        int loopState = ArticyGlobalVariables.Default.PS.loopCounter;

        targetText.text =
            $"Moral: {player.moralVal}/{player.moralCap}\n" +
            $"Loop: {loopState}\n" +
            "Skills:\n" +
            $"  Persuasion: {player.skillPersuasion.Value}\n" +
            $"  Perception: {player.skillPerseption.Value}\n" +
            $"  Accuracy: {player.skillAccuracy.Value}";
    }
}

