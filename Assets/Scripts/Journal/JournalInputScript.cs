using UnityEngine;
using UnityEngine.InputSystem;

public class JournalInputScript : MonoBehaviour
{
    private InputAction journalAction;
    private JournalUI journalUI;

    private void Start()
    {
        journalAction = InputSystem.actions?.FindAction("Journal");
        journalUI = FindObjectOfType<JournalUI>();
    }

    private void Update()
    {
        if (journalUI == null)
            return;

        if (journalAction == null)
        {
            journalAction = InputSystem.actions?.FindAction("Journal");
            if (journalAction == null)
                return;
        }

        if (journalAction.triggered) {
            journalUI.Toggle();
            Debug.Log("journal called");
        }
            
    }
}
