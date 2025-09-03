using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;

/// <summary>
/// Synchronizes clue items between Unity inventory and Articy global variables.
/// </summary>
public static class ArticyClueSync {
    /// <summary>Definitions of clues and their score values.</summary>
    public static readonly Dictionary<string, int> ClueValues =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
            { ItemIds.HarmonicRow, 2 },
            { ItemIds.SonoceramicShard, 2 },
            { ItemIds.SonusGuideTube, 1 },
            { ItemIds.ReceiptWhisperer, 2 },
            { ItemIds.WaxStoppers, 1 },
            { ItemIds.MaintScrollHum, 1 },
            { ItemIds.VentFiddle, 2 },
            { ItemIds.EarPressureReports, 1 }
        };

    private static object CLUE => ArticyGlobalVariables.Default.CLUE;

    /// <summary>
    /// Applies CLUE boolean variables from Articy to the Unity inventory.
    /// </summary>
    public static void SyncFromArticy() {
        try {
            var clueSet = CLUE;
            foreach (var kvp in ClueValues) {
                var prop = clueSet.GetType().GetProperty(kvp.Key, BindingFlags.Instance | BindingFlags.Public);
                if (prop == null || prop.PropertyType != typeof(bool))
                    continue;

                bool flag = (bool)prop.GetValue(clueSet);
                bool present = InventoryStorage.Contains(kvp.Key);
                if (flag && !present)
                    InventoryStorage.Add(kvp.Key);
                else if (!flag && present)
                    InventoryStorage.Remove(kvp.Key, InventoryStorage.GetCount(kvp.Key));
            }
            PushTotalScoreToArticy();
        } catch (Exception e) {
            Debug.LogWarning($"[ArticyClueSync] SyncFromArticy error: {e.Message}");
        }
    }

    /// <summary>
    /// Updates a CLUE boolean in Articy based on presence in the inventory.
    /// </summary>
    public static void PushToArticy(string id, bool present) {
        try {
            var clueSet = CLUE;
            var prop = clueSet.GetType().GetProperty(id, BindingFlags.Instance | BindingFlags.Public);
            if (prop != null && prop.PropertyType == typeof(bool))
                prop.SetValue(clueSet, present);
        } catch (Exception e) {
            Debug.LogWarning($"[ArticyClueSync] PushToArticy error: {e.Message}");
        }
    }

    /// <summary>
    /// Checks if an item id corresponds to a clue and returns its score.
    /// </summary>
    public static bool TryGetClueValue(string id, out int value) =>
        ClueValues.TryGetValue(id, out value);

    /// <summary>
    /// Updates PS.clueTotalScore to match the total score calculated in Unity.
    /// </summary>
    public static void PushTotalScoreToArticy() {
        try {
            ArticyGlobalVariables.Default.PS.clueTotalScore =
                Mathf.RoundToInt(InventoryStorage.ClueTotalScore);
        } catch (Exception e) {
            Debug.LogWarning($"[ArticyClueSync] PushTotalScoreToArticy error: {e.Message}");
        }
    }
}

