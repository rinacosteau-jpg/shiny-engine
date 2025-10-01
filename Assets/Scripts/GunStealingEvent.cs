using System.Collections;
using UnityEngine;
using Articy.Unity;
using Articy.World_Of_Red_Moon.GlobalVariables;

public class GunStealingEvent : MonoBehaviour, ILoopResettable {
    [SerializeField] private Transform playerNpc;
    [SerializeField] private Transform ratkoNpc;
    [SerializeField] private Transform positionA;
    [SerializeField] private Transform positionB;
    [SerializeField] private ArticyRef pathAStart;
    [SerializeField] private ArticyRef pathBStart;
    [SerializeField] private ArticyRef resultStart;
    [SerializeField] private PlayerMovementScript playerMovement;
    [SerializeField] private PlayerInteractScript playerInteract;
    [SerializeField] private DialogueUI dialogueUI;

    private Vector3 playerStartPos;
    private Vector3 ratkoStartPos;
    private bool triggered;

    private void Start() {
        if (playerNpc != null) playerStartPos = playerNpc.position;
        if (ratkoNpc != null) ratkoStartPos = ratkoNpc.position;
        if (dialogueUI == null) dialogueUI = FindObjectOfType<DialogueUI>(true);
    }

    private void Update() {
        if (!triggered && ArticyGlobalVariables.Default.EVT.event_gunStealing == 1) {
            triggered = true;
            ArticyGlobalVariables.Default.EVT.event_gunStealing = 2;
            StartCoroutine(EventSequence());
        }
    }

    private IEnumerator EventSequence() {
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerInteract != null) playerInteract.enabled = false;

      //  bool isPathB = ArticyGlobalVariables.Default.RQUE.getGun_Obj_ratB == 1;
        Transform targetPlayerPos = positionA;
        Transform targetRatkoPos = positionB;
        ArticyRef pathStart = pathAStart;

       /* if (isPathB) {
            targetPlayerPos = positionB;
            targetRatkoPos = positionA;
            pathStart = pathBStart;
        }*/

        if (playerNpc != null && targetPlayerPos != null)
            playerNpc.position = targetPlayerPos.position;
        if (ratkoNpc != null && targetRatkoPos != null)
            ratkoNpc.position = targetRatkoPos.position;

        yield return new WaitForSeconds(3f);

        if (dialogueUI != null)
            dialogueUI.StartDialogue(pathStart);

        if (dialogueUI != null)
            yield return new WaitUntil(() => !dialogueUI.IsDialogueOpen);

        if (playerNpc != null)
            playerNpc.position = playerStartPos;
        if (ratkoNpc != null)
            ratkoNpc.position = ratkoStartPos;

        yield return new WaitForSeconds(3f);

        if (dialogueUI != null)
            dialogueUI.StartDialogue(resultStart);

        Debug.Log("GunStealingEventFinished");

        if (playerMovement != null) playerMovement.enabled = true;
        if (playerInteract != null) playerInteract.enabled = true;
    }

    public void OnLoopReset() {
        triggered = false;
    }
}

