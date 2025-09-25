using UnityEngine;

public class TransformLoopResetter : MonoBehaviour, ILoopResettable {
    [SerializeField] private bool resetVelocities = true;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Vector3 _initialScale;
    private Transform _initialParent;
    private Rigidbody _rigidbody;

    public bool resetActiveState;

    private bool hasCachedState;

    void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        CacheInitialState();
    }

    void Start() {
        if (!hasCachedState) {
            CacheInitialState();
        }
    }

    public void OnLoopReset() {
        if (!hasCachedState) {
            CacheInitialState();
        }

        transform.SetParent(_initialParent);

        if (_rigidbody != null) {
            if (resetVelocities) {
                TryResetVelocities(_rigidbody);
            }

            _rigidbody.position = _initialPosition;
            _rigidbody.rotation = _initialRotation;
            _rigidbody.MovePosition(_initialPosition);
            _rigidbody.MoveRotation(_initialRotation);
        } else {
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
        }

        transform.localScale = _initialScale;

        if (resetActiveState) {
            gameObject.SetActive(true);
        }
    }

    private void CacheInitialState() {
        hasCachedState = true;

        if (_rigidbody != null) {
            _initialPosition = _rigidbody.position;
            _initialRotation = _rigidbody.rotation;
        } else {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }

        _initialScale = transform.localScale;
        _initialParent = transform.parent;
    }

    private static void TryResetVelocities(Rigidbody rb) {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
        rb.angularVelocity = Vector3.zero;
    }
}
