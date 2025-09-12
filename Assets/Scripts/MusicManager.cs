using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
