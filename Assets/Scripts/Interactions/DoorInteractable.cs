using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable, ILoopResettable
{
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;
    [SerializeField] private GameObject doorObject;
    [SerializeField] private float openRotationZ;
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip openSound;

    private bool startIsOpen;
    private Quaternion originalRotation;
    private Quaternion loopStartRotation;
    private Quaternion openRotation;

    private void Awake()
    {
        if (doorObject == null && transform.childCount > 0)
            doorObject = transform.GetChild(0).gameObject;

        startIsOpen = isOpen;

        if (doorObject != null)
        {
            originalRotation = doorObject.transform.localRotation;
            loopStartRotation = originalRotation;

            Vector3 originalEuler = doorObject.transform.localEulerAngles;
            openRotation = Quaternion.Euler(originalEuler + new Vector3(0f, 0f, openRotationZ));

            SetState(isOpen, false);
            loopStartRotation = doorObject.transform.localRotation;
        }
    }

    public void Interact()
    {
        if (isLocked)
            return;

        if (doorObject == null)
            return;

        SetState(!isOpen, true);
    }

    public void OnLoopReset()
    {
        if (doorObject == null)
            return;

        doorObject.transform.localRotation = loopStartRotation;
        isOpen = startIsOpen;
    }

    public void ForceOpen()
    {
        if (doorObject == null)
            return;

        SetState(true, false);
    }

    private void SetState(bool open, bool playAudio)
    {
        if (doorObject == null)
            return;

        bool wasOpen = isOpen;
        isOpen = open;

        doorObject.transform.localRotation = open ? openRotation : originalRotation;

        if (playAudio && open && !wasOpen && doorAudioSource != null && openSound != null)
            doorAudioSource.PlayOneShot(openSound);
    }
}
