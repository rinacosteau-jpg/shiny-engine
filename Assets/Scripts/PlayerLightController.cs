using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLightController : MonoBehaviour {
    [Header("Light Settings")]
    [SerializeField] private GameObject lightObject;
    [SerializeField] private float fadeDuration = 3f;
    [SerializeField] private Vector3 lightOffset = Vector3.zero;

    private InputAction lightAction;
    private Light[] lightComponents;
    private float[] initialIntensities;
    private float activeTimer;

    void Start() {
        lightAction = InputSystem.actions?.FindAction("Light");
        
        CacheLightComponents();
        SetLightActive(false);
    }

    void OnDestroy() {
        SetLightActive(false);
    }

    void Update() {
        if (lightAction != null && lightAction.triggered) {
            Debug.Log("light on");
            ActivateLight();
        }

        if (activeTimer > 0f && lightObject != null) {
            lightObject.transform.position = transform.position + lightOffset;

            activeTimer -= Time.deltaTime;
            float normalized = Mathf.Clamp01(activeTimer / Mathf.Max(fadeDuration, Mathf.Epsilon));
            ApplyIntensity(normalized);

            if (activeTimer <= 0f) {
                SetLightActive(false);
            }
        }
    }

    private void ActivateLight() {
        if (lightObject == null) {
            return;
        }

        if (!lightObject.activeSelf) {
            lightObject.SetActive(true);
        }

        lightObject.transform.position = transform.position + lightOffset;
        activeTimer = Mathf.Max(fadeDuration, Mathf.Epsilon);
        ApplyIntensity(1f);
    }

    private void CacheLightComponents() {
        if (lightObject == null) {
            lightComponents = System.Array.Empty<Light>();
            initialIntensities = System.Array.Empty<float>();
            return;
        }

        lightComponents = lightObject.GetComponentsInChildren<Light>(true);
        initialIntensities = new float[lightComponents.Length];
        for (int i = 0; i < lightComponents.Length; i++) {
            initialIntensities[i] = lightComponents[i].intensity;
        }
    }

    private void ApplyIntensity(float normalizedValue) {
        if (lightComponents == null || lightComponents.Length == 0) {
            return;
        }

        float clamped = Mathf.Clamp01(normalizedValue);
        for (int i = 0; i < lightComponents.Length; i++) {
            lightComponents[i].intensity = initialIntensities[i] * clamped;
        }
    }

    private void SetLightActive(bool isActive) {
        if (lightObject == null) {
            return;
        }

        if (isActive) {
            if (!lightObject.activeSelf) {
                lightObject.SetActive(true);
            }
            activeTimer = Mathf.Max(fadeDuration, Mathf.Epsilon);
            ApplyIntensity(1f);
        } else {
            if (lightComponents != null) {
                for (int i = 0; i < lightComponents.Length; i++) {
                    lightComponents[i].intensity = 0f;
                }
            }

            lightObject.SetActive(false);
            activeTimer = 0f;
        }
    }
}
