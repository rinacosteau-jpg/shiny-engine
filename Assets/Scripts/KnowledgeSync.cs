using System.Reflection;
using UnityEngine;
using Articy.Unity.Interfaces;
using Articy.World_Of_Red_Moon.GlobalVariables;

public class KnowledgeSync : MonoBehaviour
{
    public PlayerState player;

    private void OnEnable()
    {
        ArticyGlobalVariables.Default.VariableChanged += OnVariableChanged;
        UpdateKnowledgeFromGlobals();
    }

    private void OnDisable()
    {
        ArticyGlobalVariables.Default.VariableChanged -= OnVariableChanged;
    }

    private void OnVariableChanged(IGlobalVariables gv, string variableName)
    {
        if (variableName.StartsWith("KNW."))
        {
            UpdateKnowledgeFromGlobals();
        }
    }

    private void UpdateKnowledgeFromGlobals()
    {
        var knw = ArticyGlobalVariables.Default.KNW;
        foreach (var prop in typeof(KNW).GetProperties())
        {
            if (prop.PropertyType == typeof(bool))
            {
                bool value = (bool)prop.GetValue(knw);
                if (value)
                {
                    string name = prop.Name;
                    int hash = name.GetHashCode();
                    if (!player.Knowledge.ContainsKey(hash))
                    {
                        player.Knowledge[hash] = new KnowledgeItem(name, name);
                    }
                }
            }
        }
    }
}
