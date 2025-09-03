using TMPro;
using UnityEngine;

/// <summary>
/// Displays the total clue score on a TextMeshPro component.
/// </summary>
public class ClueTotalScoreDisplay : MonoBehaviour {
    [SerializeField] private TMP_Text targetText;

    private void Awake() {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
    }

    private void Update() {
        if (targetText == null)
            return;
        targetText.text = $"Clue Score: {InventoryStorage.ClueTotalScore:F1}";
    }
}
