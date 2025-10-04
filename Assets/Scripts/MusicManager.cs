using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private AudioClip defaultClip;
    private float defaultVolume = 1f;
    private Coroutine transitionRoutine;
    private bool isAlternateActive;

    private void Awake()
    {
        if (audioSource != null)
        {
            defaultClip = audioSource.clip;
            defaultVolume = audioSource.volume;
        }
    }

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource not assigned to MusicManager");
            return;
        }

        if (audioSource.clip == null && defaultClip != null)
        {
            audioSource.clip = defaultClip;
        }

        audioSource.volume = defaultVolume;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void ToggleAlternateTrack(AudioClip alternateClip, float fadeDuration)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("MusicManager: No AudioSource assigned for toggling");
            return;
        }

        if (alternateClip == null && !isAlternateActive)
        {
            Debug.LogWarning("MusicManager: Alternate clip not provided for toggle");
            return;
        }

        AudioClip targetClip = isAlternateActive ? defaultClip : alternateClip;

        if (targetClip == null)
        {
            Debug.LogWarning("MusicManager: Target clip missing during toggle");
            return;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        transitionRoutine = StartCoroutine(SwapMusicRoutine(targetClip, fadeDuration));
        isAlternateActive = !isAlternateActive;
    }

    private IEnumerator SwapMusicRoutine(AudioClip nextClip, float fadeDuration)
    {
        float initialVolume = audioSource.volume;
        float duration = Mathf.Max(0.01f, fadeDuration);

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float t = elapsed / duration;
            audioSource.volume = Mathf.Lerp(initialVolume, 0f, t);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();

        audioSource.clip = nextClip;
        audioSource.Play();

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float t = elapsed / duration;
            audioSource.volume = Mathf.Lerp(0f, defaultVolume, t);
            yield return null;
        }

        audioSource.volume = defaultVolume;
        transitionRoutine = null;
    }
}
