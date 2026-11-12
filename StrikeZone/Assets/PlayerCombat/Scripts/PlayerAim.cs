using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private VirtualJoystick aimJoystick;
    [SerializeField] private bool useJoystickAim = true;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (useJoystickAim)
        {
            if (aimJoystick != null)
                AimInDirection(aimJoystick.Value);

            return;
        }

        if (Mouse.current == null || mainCamera == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float distance = transform.position.z
                         - mainCamera.transform.position.z;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, distance)
        );

        Vector2 direction = worldPosition - transform.position;
        AimInDirection(direction);
    }

    private void AimInDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x)
                      * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}