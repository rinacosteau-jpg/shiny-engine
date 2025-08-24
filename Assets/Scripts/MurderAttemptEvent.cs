using System.Collections;
using UnityEngine;
using Articy.World_Of_Red_Moon.GlobalVariables;
using Cinemachine;

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

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float zoomedOutSize = 8f;
    [SerializeField] private float zoomDuration = 1f;

    private float defaultCameraSize;

    private bool triggered;

    void Start() {
        GameTime.Instance.OnTimeChanged += OnTimeChanged;

        if (virtualCamera == null)
        {
            var brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;
            if (brain != null)
                virtualCamera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        }
        if (virtualCamera != null)
            defaultCameraSize = virtualCamera.m_Lens.Orthographic ? virtualCamera.m_Lens.OrthographicSize : virtualCamera.m_Lens.FieldOfView;
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

        GlobalVariables.Instance?.ForceCloseDialogue();
        foreach (var dialogue in FindObjectsOfType<DialogueUI>(true))
            dialogue.CloseDialogue();


        if (playerMovement != null) playerMovement.enabled = false;
        if (playerInteract != null) playerInteract.enabled = false;

        if (virtualCamera != null)
            yield return StartCoroutine(ZoomCamera(zoomedOutSize));

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

        if (virtualCamera != null)
            yield return StartCoroutine(ZoomCamera(defaultCameraSize));

        if (playerMovement != null) playerMovement.enabled = true;
        if (playerInteract != null) playerInteract.enabled = true;

        yield break;
    }

    public void OnLoopReset() {
        triggered = false;
    }

    private IEnumerator ZoomCamera(float targetSize)
    {
        float startSize = virtualCamera.m_Lens.Orthographic ? virtualCamera.m_Lens.OrthographicSize : virtualCamera.m_Lens.FieldOfView;
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            float size = Mathf.Lerp(startSize, targetSize, t);
            if (virtualCamera.m_Lens.Orthographic)
                virtualCamera.m_Lens.OrthographicSize = size;
            else
                virtualCamera.m_Lens.FieldOfView = size;
            yield return null;
        }

        if (virtualCamera.m_Lens.Orthographic)
            virtualCamera.m_Lens.OrthographicSize = targetSize;
        else
            virtualCamera.m_Lens.FieldOfView = targetSize;
    }
}
