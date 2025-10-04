using UnityEngine;

public class MusicToggleTrigger : MonoBehaviour
{
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private AudioClip alternateClip;
    [SerializeField] private float fadeDuration = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerCollider == null)
        {
            Debug.LogWarning("MusicToggleTrigger: Trigger collider reference missing", this);
            return;
        }

        if (other != triggerCollider)
        {
            return;
        }

        if (musicManager == null)
        {
            Debug.LogWarning("MusicToggleTrigger: MusicManager reference missing", this);
            return;
        }

        musicManager.ToggleAlternateTrack(alternateClip, fadeDuration);
    }
}
