using System.Collections;
using UnityEngine;
using Articy.World_Of_Red_Moon.GlobalVariables;

public class MurderAttemptEvent : MonoBehaviour, ILoopResettable {
    [SerializeField] private Transform firstNpcA;
    [SerializeField] private Transform firstNpcB;
    [SerializeField] private Transform secondNpcA;
    [SerializeField] private Transform secondNpcB;
    [SerializeField] private Transform secondNpcC;
    [SerializeField] private Transform secondNpcD;
    [SerializeField] private Transform secondNpcE;
    [SerializeField] private PlayerMovementScript playerMovement;
    [SerializeField] private PlayerInteractScript playerInteract;

    [SerializeField] Transform spawnAo1, spawnTomas1, spawnAo2, spawnTomas2, spawnTasha, spawnGuardM, spawnGuardD;

    private bool triggered;

    void Start() {
        GameTime.Instance.OnTimeChanged += OnTimeChanged;
    }

    void OnDestroy() {
        if (GameTime.Instance != null)
            GameTime.Instance.OnTimeChanged -= OnTimeChanged;
    }

    void OnTimeChanged(int hours, int minutes) {
        if (!triggered && hours == 12 && minutes == 42) {
            triggered = true;
            StartCoroutine(EventSequence());
        }
    }

    IEnumerator EventSequence() {
        var dialogue = FindObjectOfType<DialogueUI>();
        dialogue?.CloseDialogue();

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerInteract != null) playerInteract.enabled = false;

        yield return new WaitForSeconds(3f);

        if (firstNpcA != null) firstNpcA.position = spawnAo1.position; // TODO: specify target position
        if (firstNpcB != null) firstNpcB.position = spawnTomas1.position; // TODO: specify target position

        yield return new WaitForSeconds(3f);

        if (secondNpcA != null) secondNpcA.position =spawnAo2.position;
        if (secondNpcB != null) secondNpcB.position = spawnTomas2.position;// TODO: specify target position
        if (secondNpcC != null) secondNpcC.position = spawnTasha.position; // TODO: specify target position
        if (secondNpcD != null) secondNpcD.position = spawnGuardM.position; // TODO: specify target position
        if (secondNpcE != null) secondNpcE.position = spawnGuardD.position; // TODO: specify target position

        ArticyGlobalVariables.Default.EVT.event_murderAttempt = 1;

        if (playerMovement != null) playerMovement.enabled = true;
        if (playerInteract != null) playerInteract.enabled = true;

        yield break;
    }

    public void OnLoopReset() {
        triggered = false;
    }
}
