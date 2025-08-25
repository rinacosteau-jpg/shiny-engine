using TMPro;
using UnityEngine;
using Articy.World_Of_Red_Moon.GlobalVariables;

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
        ArticyGlobalVariables.Default.PS.loopCounter = Count;
    }

    public void OnLoopReset()
    {
        Count++;
        ArticyGlobalVariables.Default.PS.loopCounter = Count;
        Debug.Log("Articy loopcount: " + ArticyGlobalVariables.Default.PS.loopCounter);
        UpdateText();
    }

    private void UpdateText()
    {
        if (loopText != null)
            loopText.text = Count.ToString();
    }
}
