using UnityEngine;
using TMPro;

public class NpcLabel : MonoBehaviour {
    public string labelText = "NPC";
    public float height = 2f;
    public Color color = Color.white;
    public Color outlineColor = Color.black;
    public float outlineWidth = 0.25f;
    public int fontSize = 4;

    private TextMeshPro textMesh;

    /*void Start() {
        GameObject go = new GameObject("NpcLabelText");
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0, height, 0);

        textMesh = go.AddComponent<TextMeshPro>();
        textMesh.text = labelText;
        textMesh.color = color;
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;

        // Обводка
        textMesh.outlineColor = outlineColor;
        textMesh.outlineWidth = outlineWidth;
    }

    void LateUpdate() {
        if (Camera.main != null && textMesh != null) {
            textMesh.transform.rotation = Quaternion.LookRotation(
                textMesh.transform.position - Camera.main.transform.position
            );
        }
    }*/
}
