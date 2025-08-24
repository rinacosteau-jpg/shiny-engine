using System;
using System.Reflection;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon; // если у тебя другой ns от Articy — поправь
using Articy.World_Of_Red_Moon.GlobalVariables;

public static class ArticyInventorySync {
    // В ITM держим:
    // item_<ID>_delta : int  — диалоги прибавляют/убавляют сюда (может быть +/−)
    // item_<ID>_count : int  — фактическое количество; мы обновляем из InventoryStorage
    private const string Prefix = "item_";
    private const string SuffixDelta = "_delta";
    private const string SuffixCount = "_count";

    private static object ITM => ArticyGlobalVariables.Default.ITM;

    /// <summary>
    /// Считывает ВСЕ item_*_delta из ITM, применяет к InventoryStorage и обнуляет их.
    /// Затем пушит текущие *_count обратно в ITM.
    /// </summary>
    public static void ApplyItemDeltasFromArticy() {
        try {
            var itm = ITM;
            var props = itm.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            int applied = 0;

            foreach (var p in props) {
                if (p.PropertyType != typeof(int)) continue;
                var name = p.Name;
                if (!name.StartsWith(Prefix) || !name.EndsWith(SuffixDelta)) continue;

                int delta = (int)p.GetValue(itm);
                if (delta == 0) continue;

                string itemId = name.Substring(Prefix.Length, name.Length - Prefix.Length - SuffixDelta.Length);

                if (delta > 0) InventoryStorage.Add(itemId, delta);
                else InventoryStorage.Remove(itemId, -delta);

                p.SetValue(itm, 0); // чтобы не применить второй раз
                applied++;
            }

            if (applied > 0) {
                PushAllCountsToArticy();
                Debug.Log($"[ArticyInventorySync] Applied {applied} item deltas from ITM.");
            }
        } catch (Exception e) {
            Debug.LogWarning($"[ArticyInventorySync] ApplyItemDeltasFromArticy error: {e.Message}");
        }
    }

    /// <summary>
    /// Пушит в ITM все item_*_count по факту из InventoryStorage.
    /// Лишние *_count, для которых предметов нет, — обнуляет.
    /// </summary>
    public static void PushAllCountsToArticy() {
        try {
            var itm = ITM;
            var type = itm.GetType();

            // 1) обновить count для предметов, которые есть
            foreach (var item in InventoryStorage.Items) {
                var propName = $"{Prefix}{item.TechnicalName}{SuffixCount}";
                var p = type.GetProperty(propName);
                if (p != null && p.PropertyType == typeof(int))
                    p.SetValue(itm, item.ItemCount);
            }

            // 2) обнулить count у тех, кого больше нет в инвентаре
            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                if (p.PropertyType != typeof(int)) continue;
                var name = p.Name;
                if (!name.StartsWith(Prefix) || !name.EndsWith(SuffixCount)) continue;

                string itemId = name.Substring(Prefix.Length, name.Length - Prefix.Length - SuffixCount.Length);
                bool present = false;
                foreach (var itmEntry in InventoryStorage.Items)
                    if (itmEntry.TechnicalName == itemId) { present = true; break; }
                if (!present)
                    p.SetValue(itm, 0);
            }
        } catch (Exception e) {
            Debug.LogWarning($"[ArticyInventorySync] PushAllCountsToArticy error: {e.Message}");
        }
    }

    /// <summary>
    /// (опционально) Обнуляет ВСЕ item_*_delta в ITM. Полезно на полном ресете петли.
    /// </summary>
    public static void ResetAllItemDeltas() {
        try {
            var itm = ITM;
            var type = itm.GetType();
            int cleared = 0;
            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                if (p.PropertyType != typeof(int)) continue;
                var n = p.Name;
                if (n.StartsWith(Prefix) && n.EndsWith(SuffixDelta)) {
                    p.SetValue(itm, 0);
                    cleared++;
                }
            }
            if (cleared > 0)
                Debug.Log($"[ArticyInventorySync] Cleared {cleared} item deltas in ITM.");
        } catch (Exception e) {
            Debug.LogWarning($"[ArticyInventorySync] ResetAllItemDeltas error: {e.Message}");
        }
    }
}
