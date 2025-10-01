using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLightController : MonoBehaviour {
    [Header("Light Settings")]
    [SerializeField] private GameObject lightObject;
    [SerializeField] private float fadeDuration = 3f;
    [SerializeField] private Vector3 lightOffset = Vector3.zero;

    private InputAction lightAction;
    [SerializeField] private Transform playerTransform;

    private Transform lightTransform;
    private Light[] lightComponents;
    private float[] initialIntensities;
    private float activeTimer;
    private Transform cachedTransform;

    void Awake() {
        cachedTransform = transform;

        if (playerTransform == null) {
            var movement = FindObjectOfType<PlayerMovementScript>();
            if (movement != null) {
                playerTransform = movement.transform;
            }
        }

        if (playerTransform == null) {
            playerTransform = cachedTransform;
        }

        if (lightObject != null) {
            lightTransform = lightObject.transform;
        }
    }

    void OnEnable() {
        if (lightAction == null) {
            lightAction = InputSystem.actions?.FindAction("Light");
        }
        lightAction?.Enable();
        UpdateLightTransform();
    }

    void OnDisable() {
        lightAction?.Disable();
    }

    void Start() {
        if (lightAction == null) {
            lightAction = InputSystem.actions?.FindAction("Light");
            lightAction?.Enable();
        }

        CacheLightComponents();
        SetLightActive(false);
    }

    void OnDestroy() {
        SetLightActive(false);
    }

    void Update() {
        UpdateLightTransform();

        if (lightAction != null && lightAction.triggered) {
            Debug.Log("light on");
            ActivateLight();
        }

        if (activeTimer > 0f && lightObject != null) {
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

        UpdateLightTransform();
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

    private void UpdateLightTransform() {
        if (lightTransform == null) {
            return;
        }

        Vector3 targetPosition = playerTransform != null ? playerTransform.position : cachedTransform.position;
        Vector3 finalPosition = targetPosition + lightOffset;
        finalPosition.y = 0.7f;
        lightTransform.position = finalPosition;
    }
}
