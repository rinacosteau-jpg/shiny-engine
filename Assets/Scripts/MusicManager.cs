using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource not assigned to MusicManager");
            return;
        }

        audioSource.loop = true;
        audioSource.Play();
    }
}
