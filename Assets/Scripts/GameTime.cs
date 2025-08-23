using TMPro;
using UnityEngine;

public class GameTime : MonoBehaviour {

    [SerializeField] TMP_Text clockText;

    public static GameTime Instance { get; private set; }

    public int Hours { get; private set; } = 12;
    public int Minutes { get; private set; } = 12;

    void Awake() => Instance = this;

    public void AddMinutes(int delta) {
        Minutes += delta;
        while (Minutes >= 60) { Hours++; Minutes -= 60; }
        if (Hours >= 24) Hours = 0;
        // тут можно бросать событие OnTimeChanged
    }

    public override string ToString() => $"{Hours:D2}:{Minutes:D2}";

    public void Update() {
        clockText.text = GameTime.Instance.ToString();
    }
    }
