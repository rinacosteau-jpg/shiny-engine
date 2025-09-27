using UnityEngine;
using UnityEngine.InputSystem;

public class JournalInputScript : MonoBehaviour
{
    private InputAction journalAction;
    private InputAction escapeAction;
    private JournalUI journalUI;

    private void Start()
    {
        journalAction = InputSystem.actions?.FindAction("Journal");
        escapeAction = InputSystem.actions?.FindAction("Escape");
        journalUI = FindObjectOfType<JournalUI>();
    }

    private void Update()
    {
        if (journalUI == null)
            return;

        if (journalAction == null)
            journalAction = InputSystem.actions?.FindAction("Journal");

        if (escapeAction == null)
            escapeAction = InputSystem.actions?.FindAction("Escape");

        if (journalAction != null && journalAction.triggered)
        {
            journalUI.Show();
            Debug.Log("journal opened");
        }

        if (escapeAction != null && escapeAction.triggered)
        {
            journalUI.Hide();
            Debug.Log("journal closed");
        }
    }
}
