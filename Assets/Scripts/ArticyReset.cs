using System;
using System.Reflection;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;

public static class ArticyReset {
    /// <summary>
    ///     ArticyGlobalVariables.Default.RQUE:
    /// int -> 0, bool -> false, string -> "".
    /// </summary>
    public static void ResetRQUE() {
        try {
            var rque = ArticyGlobalVariables.Default.RQUE;
            var props = rque.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            int changed = 0;
            foreach (var p in props) {
                if (!p.CanWrite) continue;

                var t = p.PropertyType;
                object val = null;

                if (t == typeof(int)) val = 0;
                else if (t == typeof(bool)) val = false;
                else if (t == typeof(string)) val = string.Empty;
                else if (t == typeof(float)) val = 0f;
                else if (t == typeof(double)) val = 0d;
                else if (t.IsEnum) val = Enum.GetValues(t).GetValue(0); // enum
                else continue;

                p.SetValue(rque, val);
                changed++;
            }

            Debug.Log($"[ArticyReset] RQUE cleared: {changed} variables.");
        } catch (Exception e) {
            Debug.LogWarning($"[ArticyReset] ResetRQUE error: {e.Message}");
        }
    }

    /// <summary>
    ///     Resets ArticyGlobalVariables.Default.EVT similar to RQUE.
    /// </summary>
    public static void ResetEVT() {
        try {
            var evt = ArticyGlobalVariables.Default.EVT;
            var props = evt.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            int changed = 0;
            foreach (var p in props) {
                if (!p.CanWrite) continue;

                var t = p.PropertyType;
                object val = null;

                if (t == typeof(int)) val = 0;
                else if (t == typeof(bool)) val = false;
                else if (t == typeof(string)) val = string.Empty;
                else if (t == typeof(float)) val = 0f;
                else if (t == typeof(double)) val = 0d;
                else if (t.IsEnum) val = Enum.GetValues(t).GetValue(0);
                else continue;

                p.SetValue(evt, val);
                changed++;
            }

            Debug.Log($"[ArticyReset] EVT cleared: {changed} variables.");
        } catch (Exception e) {
            Debug.LogWarning($"[ArticyReset] ResetEVT error: {e.Message}");
        }
    }
}
