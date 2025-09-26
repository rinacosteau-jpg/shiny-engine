using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractableOutline : MonoBehaviour {
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");

    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField, Range(0.001f, 0.1f)] private float outlineThickness = 0.02f;

    private readonly Dictionary<Renderer, Material> outlineMaterials = new Dictionary<Renderer, Material>();
    private bool initialized;

    private void Awake() {
        Initialize();
        SetHighlighted(false);
    }

    private void Reset() {
        targetRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Initialize() {
        if (initialized)
            return;

        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>();

        Shader outlineShader = Shader.Find("Custom/InteractableOutline");
        if (outlineShader == null) {
            Debug.LogError("[InteractableOutline] Shader 'Custom/InteractableOutline' not found.");
            return;
        }

        foreach (Renderer renderer in targetRenderers) {
            if (renderer == null)
                continue;

            Material outlineMaterial = new Material(outlineShader) {
                hideFlags = HideFlags.HideAndDontSave
            };

            outlineMaterial.SetColor(OutlineColor, outlineColor);
            outlineMaterial.SetFloat(OutlineThickness, outlineThickness);
            outlineMaterial.SetFloat(OutlineEnabled, 0f);

            var materials = new List<Material>(renderer.sharedMaterials) { outlineMaterial };
            renderer.materials = materials.ToArray();
            outlineMaterials[renderer] = outlineMaterial;
        }

        initialized = true;
    }

    public void SetHighlighted(bool highlighted) {
        Initialize();

        float enabledValue = highlighted ? 1f : 0f;
        foreach (Material material in outlineMaterials.Values) {
            if (material == null)
                continue;

            material.SetFloat(OutlineEnabled, enabledValue);
        }
    }

    private void OnDestroy() {
        foreach (Material material in outlineMaterials.Values) {
            if (material != null)
                Destroy(material);
        }

        outlineMaterials.Clear();
    }
}
