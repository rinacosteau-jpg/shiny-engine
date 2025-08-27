using System;
using System.Reflection;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;

public static class ArticyReset {
    /// <summary>
    ///     Resets a set of Articy global variables by name.
    ///     Supported types are int, bool, string, float, double and enums.
    /// </summary>
    /// <param name="setName">Name of the variable set inside ArticyGlobalVariables.Default.</param>
    public static void ResetArticySet(string setName) {
        try {
            var gvDefault = ArticyGlobalVariables.Default;
            var setProp = gvDefault.GetType().GetProperty(setName, BindingFlags.Instance | BindingFlags.Public);
            if (setProp == null) {
                Debug.LogWarning($"[ArticyReset] Set {setName} not found.");
                return;
            }

            var set = setProp.GetValue(gvDefault);
            if (set == null) {
                Debug.LogWarning($"[ArticyReset] Set {setName} is null.");
                return;
            }

            var props = set.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
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

                p.SetValue(set, val);
                changed++;
            }

            Debug.Log($"[ArticyReset] {setName} cleared: {changed} variables.");
        } catch (Exception e) {
            Debug.LogWarning($"[ArticyReset] ResetArticySet error on {setName}: {e.Message}");
        }
    }
}
