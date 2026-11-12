using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private VirtualJoystick moveJoystick;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector2 keyboardInput = Vector2.zero;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) keyboardInput.y += 1f;
            if (keyboard.sKey.isPressed) keyboardInput.y -= 1f;
            if (keyboard.aKey.isPressed) keyboardInput.x -= 1f;
            if (keyboard.dKey.isPressed) keyboardInput.x += 1f;
        }

        Vector2 joystickInput = moveJoystick != null
            ? moveJoystick.Value
            : Vector2.zero;

        moveInput = joystickInput.sqrMagnitude > 0.01f
            ? joystickInput
            : Vector2.ClampMagnitude(keyboardInput, 1f);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}