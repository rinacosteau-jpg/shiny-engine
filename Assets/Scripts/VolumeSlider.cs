using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour {
    [SerializeField] private Slider slider;
    [SerializeField] private AudioSource musicSource; // или используйте AudioListener

    private const string VolumeKey = "MasterVolume";

    private void Awake() {
        float saved = PlayerPrefs.GetFloat(VolumeKey, 1f);
        slider.value = saved * 100f;
        Apply(saved);
        slider.onValueChanged.AddListener(v => Apply(v / 100f));
    }

    private void Apply(float value01) {
        if (musicSource != null) musicSource.volume = value01;
        else AudioListener.volume = value01;
        PlayerPrefs.SetFloat(VolumeKey, value01);
    }
}
