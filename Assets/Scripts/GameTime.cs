using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Simple in-game clock that tracks hours and minutes, updates the UI, and notifies listeners on changes.
/// </summary>
public class GameTime : MonoBehaviour {

    [SerializeField] TMP_Text clockText; // UI label to render the current time.
    [SerializeField] private LoopResetInputScript loopReset; // Optional: reference for external reset triggers.

    public static GameTime Instance { get; private set; }

    public int Hours { get; set; } = 12;
    public int Minutes { get; set; } = 12;

    public event Action<int, int> OnTimeChanged;

    void Awake() => Instance = this;

    /// <summary>
    /// Advances time by the specified number of minutes, wrapping at 60 minutes and 24 hours.
    /// Invokes <see cref="OnTimeChanged"/> and updates the UI.
    /// </summary>
    public void AddMinutes(int delta) {
        Minutes += delta;
        while (Minutes >= 60) { Hours++; Minutes -= 60; }
        if (Hours >= 24) Hours = 0;
        Update();
        OnTimeChanged?.Invoke(Hours, Minutes);
    }

    public override string ToString() => $"{Hours:D2}:{Minutes:D2}";

    public void Update() {
        clockText.text = GameTime.Instance.ToString();
    }
}
