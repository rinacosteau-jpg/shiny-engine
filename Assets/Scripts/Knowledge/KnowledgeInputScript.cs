using UnityEngine;
using UnityEngine.InputSystem;

public class KnowledgeInputScript : MonoBehaviour
{
    private InputAction knowledgeAction;
    private InputAction escapeAction;
    private KnowledgeUI knowledgeUI;

    private void Start()
    {
        knowledgeAction = InputSystem.actions?.FindAction("Knowledge");
        escapeAction = InputSystem.actions?.FindAction("Escape");
        knowledgeUI = FindObjectOfType<KnowledgeUI>();
    }

    private void Update()
    {
        if (knowledgeUI == null)
        {
            knowledgeUI = FindObjectOfType<KnowledgeUI>();
            if (knowledgeUI == null)
                return;
        }

        if (knowledgeAction == null)
            knowledgeAction = InputSystem.actions?.FindAction("Knowledge");

        if (escapeAction == null)
            escapeAction = InputSystem.actions?.FindAction("Escape");

        if (knowledgeAction != null && knowledgeAction.triggered)
        {
            knowledgeUI.Show();
            Debug.Log("knowledge opened");
        }

        if (escapeAction != null && escapeAction.triggered)
        {
            knowledgeUI.Hide();
            Debug.Log("knowledge closed");
        }
    }
}
