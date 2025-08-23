using UnityEngine;

public class TransformLoopResetter : MonoBehaviour, ILoopResettable
{
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Vector3 _initialScale;
    private Transform _initialParent;

    public bool resetActiveState;

    void Start()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialScale = transform.localScale;
        _initialParent = transform.parent;
    }

    public void OnLoopReset()
    {
        transform.SetParent(_initialParent);
        transform.SetPositionAndRotation(_initialPosition, _initialRotation);
        transform.localScale = _initialScale;

        if (resetActiveState)
        {
            gameObject.SetActive(true);
        }
    }
}
