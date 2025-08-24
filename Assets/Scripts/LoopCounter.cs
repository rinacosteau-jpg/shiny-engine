using TMPro;
using UnityEngine;

public class LoopCounter : MonoBehaviour, ILoopResettable {
    [SerializeField] private TMP_Text loopCounterText;

    public static LoopCounter Instance { get; private set; }

    public int Count { get; private set; } = 0;

    void Awake() {
        Instance = this;
        UpdateText();
    }

    public void OnLoopReset() {
        Count++;
        UpdateText();
    }

    private void UpdateText() {
        if (loopCounterText != null)
            loopCounterText.text = Count.ToString();
    }
}
