using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovementScript : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private Animator animator;
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float castSkin = 0.05f;     // зазор при капсуль-касте
    [SerializeField] private float groundSnapDistance = 20f; // сколько метров проверяем вниз при старте
    [SerializeField] private LayerMask collisionMask = ~0;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private InputAction moveAction;

    private Vector2 moveInput;
    private Vector3 desiredDir; // желаемое направление в мире (от камеры)

    void Awake() {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        if (cam == null) cam = Camera.main;

        AlignColliderPivot();
        SnapToGround();

        // Настройки физики для персонажа
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.linearDamping = 2f;              // уменьшает «катание на льду»
        rb.angularDamping = 999f;     // фактически гасим любой спин от физики
        rb.maxAngularVelocity = 0.01f;
    }

    void Start() {
        moveAction = InputSystem.actions.FindAction("Move");
        if (dialogueUI == null) dialogueUI = FindObjectOfType<DialogueUI>();
        if (animator != null) animator.applyRootMotion = false;
    }

    void Update() {
        if (dialogueUI != null && dialogueUI.IsDialogueOpen) {
            moveInput = Vector2.zero;
            desiredDir = Vector3.zero;
            UpdateAnimator(0f, false);
            return;
        }

        moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        // локальные оси от камеры в плоскости XZ
        Vector3 fwd = cam.transform.forward; fwd.y = 0; fwd.Normalize();
        Vector3 right = cam.transform.right; right.y = 0; right.Normalize();

        desiredDir = (fwd * moveInput.y + right * moveInput.x);
        if (desiredDir.sqrMagnitude > 1f) desiredDir.Normalize();

        float targetSpeed = desiredDir.magnitude * moveSpeed;
        UpdateAnimator(targetSpeed, desiredDir.sqrMagnitude > 0.0001f);
    }

    void FixedUpdate() {
        // гасим любой спин от физики
        rb.angularVelocity = Vector3.zero;

        Vector3 moveVec = desiredDir * moveSpeed * Time.fixedDeltaTime;

        if (moveVec.sqrMagnitude > 0f) {
            // капсуль-каст вперед, чтобы не врезаться и соскальзывать по стене
            Vector3 p1, p2; float radius;
            GetCapsuleWorld(out p1, out p2, out radius);

            if (Physics.CapsuleCast(p1, p2, radius, moveVec.normalized, out RaycastHit hit, moveVec.magnitude + castSkin, collisionMask, QueryTriggerInteraction.Ignore)) {
                // убираем компонент вдоль нормали, скользим по поверхности
                Vector3 slideDir = Vector3.ProjectOnPlane(moveVec, hit.normal);
                Vector3 targetPos = rb.position + slideDir;
                rb.MovePosition(targetPos);
            } else {
                rb.MovePosition(rb.position + moveVec);
            }

            // плавный поворот к направлению движения
            Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        } else {
            // при остановке сохраняем вертикальную скорость (гравитация), XZ — к нулю
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void UpdateAnimator(float targetSpeed, bool isMoving) {
        if (animator == null) return;
        animator.SetFloat("Speed", targetSpeed);
        animator.SetBool("IsMoving", isMoving);
    }

    private void GetCapsuleWorld(out Vector3 p1, out Vector3 p2, out float r) {
        // пересчёт локального CapsuleCollider в мировые точки
        r = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float height = Mathf.Max(capsule.height * transform.lossyScale.y, r * 2f);
        Vector3 center = transform.TransformPoint(capsule.center);
        Vector3 up = transform.up;

        float half = (height * 0.5f) - r;
        p1 = center + up * half;
        p2 = center - up * half;
    }

    private void AlignColliderPivot() {
        if (capsule == null) return;
        // Корректируем стартовую позицию, чтобы нижняя точка капсулы совпадала с опорой.
        float halfHeight = capsule.height * 0.5f;
        float bottomOffset = capsule.center.y - halfHeight;
        if (Mathf.Approximately(bottomOffset, 0f)) return;

        Vector3 pos = rb != null ? rb.position : transform.position;
        pos.y -= bottomOffset;

        if (rb != null) {
            rb.position = pos;
        } else {
            transform.position = pos;
        }
    }

    private void SnapToGround() {
        if (capsule == null) return;

        Vector3 up = transform.up;
        Vector3 worldCenter = transform.TransformPoint(capsule.center);
        float radius = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float height = Mathf.Max(capsule.height * transform.lossyScale.y, radius * 2f);
        float halfHeight = height * 0.5f;

        float extraProbe = Mathf.Max(0.1f, castSkin);
        Vector3 rayOrigin = worldCenter + up * (halfHeight + extraProbe);
        float rayLength = halfHeight + groundSnapDistance + extraProbe;

        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, -up, rayLength, collisionMask, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0) return;

        bool foundGround = false;
        float closestDistance = float.PositiveInfinity;
        RaycastHit groundHit = default;

        for (int i = 0; i < hits.Length; i++) {
            RaycastHit hit = hits[i];
            if (hit.collider == null || hit.collider == capsule) continue;
            if (hit.distance < closestDistance) {
                closestDistance = hit.distance;
                groundHit = hit;
                foundGround = true;
            }
        }

        if (!foundGround) return;

        Vector3 bottomPoint = worldCenter - up * (halfHeight - radius);
        Vector3 targetPoint = groundHit.point + up * castSkin;
        float delta = Vector3.Dot(targetPoint - bottomPoint, up);
        if (Mathf.Approximately(delta, 0f)) return;

        Vector3 shift = up * delta;
        if (rb != null) {
            rb.position += shift;
        } else {
            transform.position += shift;
        }
    }
}
