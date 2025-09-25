using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLightController : MonoBehaviour {
    [Header("Light Settings")]
    [SerializeField] private GameObject lightObject;
    [SerializeField] private float fadeDuration = 3f;
    [SerializeField] private Vector3 lightOffset = Vector3.zero;

    [Header("Follow Target")]
    [SerializeField] private Transform followTarget;

    private InputAction lightAction;
    private Light[] lightComponents;
    private float[] initialIntensities;
    private float activeTimer;
    private Transform resolvedFollowTarget;
    private Transform lightTransform;

    void Awake() {
        ResolveFollowTarget();
        CacheLightComponents();
    }

    void Start() {
        lightAction = InputSystem.actions?.FindAction("Light");
        UpdateLightTransform();
        SetLightActive(false);
    }

    void OnDestroy() {
        SetLightActive(false);
    }

    void Update() {
        if (lightAction != null && lightAction.triggered) {
            ActivateLight();
        }

        if (activeTimer > 0f && lightObject != null) {
            UpdateLightTransform();

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
            lightTransform = null;
            return;
        }

        lightTransform = lightObject.transform;
        lightComponents = lightObject.GetComponentsInChildren<Light>(true);
        initialIntensities = new float[lightComponents.Length];
        for (int i = 0; i < lightComponents.Length; i++) {
            initialIntensities[i] = lightComponents[i].intensity;
        }
    }

    private void ResolveFollowTarget() {
        if (followTarget != null) {
            resolvedFollowTarget = followTarget;
            return;
        }

        var movement = GetComponentInParent<PlayerMovementScript>();
        if (movement != null) {
            resolvedFollowTarget = movement.transform;
            return;
        }

        var resetter = GetComponentInParent<TransformLoopResetter>();
        if (resetter != null) {
            resolvedFollowTarget = resetter.transform;
            return;
        }

        var fallbackMovement = FindObjectOfType<PlayerMovementScript>();
        if (fallbackMovement != null) {
            resolvedFollowTarget = fallbackMovement.transform;
            return;
        }

        resolvedFollowTarget = transform;
    }

    private void UpdateLightTransform() {
        if (lightTransform == null) {
            return;
        }

        if (resolvedFollowTarget == null) {
            if (Application.isPlaying) {
                ResolveFollowTarget();
            } else {
                resolvedFollowTarget = followTarget != null ? followTarget : transform;
            }
        }

        Transform target = resolvedFollowTarget != null ? resolvedFollowTarget : transform;
        Vector3 worldPosition = target.position + lightOffset;
        lightTransform.position = worldPosition;
    }

#if UNITY_EDITOR
    void OnValidate() {
        CacheLightComponents();
        resolvedFollowTarget = followTarget;
        UpdateLightTransform();
    }
#endif

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
