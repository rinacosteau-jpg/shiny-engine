using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementScript : MonoBehaviour {
    InputAction moveAction;
    public Rigidbody rb;
    public float movementSpeed = 5;

    void Start() {
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update() {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        Vector3 movement = new Vector3(moveValue.x, 0, moveValue.y);

        // Нормализуем вектор, чтобы скорость была одинаковой во всех направлениях
        if (movement.magnitude > 1f)
            movement.Normalize();

        rb.linearVelocity = movement * movementSpeed;
    }
}
