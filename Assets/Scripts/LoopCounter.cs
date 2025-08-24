using TMPro;
using UnityEngine;

public class LoopCounter : MonoBehaviour, ILoopResettable
{
    [SerializeField] private TMP_Text loopText;

    public static LoopCounter Instance { get; private set; }

    public int Count { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateText();
    }

    public void OnLoopReset()
    {
        Count++;
        UpdateText();
    }

    private void UpdateText()
    {
        if (loopText != null)
            loopText.text = Count.ToString();
    }
}
