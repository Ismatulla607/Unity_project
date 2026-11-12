using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class NetworkPlayerAim : NetworkBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner || mainCamera == null)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float distance = transform.position.z
                         - mainCamera.transform.position.z;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, distance)
        );

        Vector2 direction = worldPosition - transform.position;

        if (direction.sqrMagnitude < 0.01f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x)
                      * Mathf.Rad2Deg;

        if (IsServer)
        {
            ApplyRotation(angle);
        }
        else
        {
            SubmitRotationRpc(angle);
        }
    }

    [Rpc(SendTo.Server)]
    private void SubmitRotationRpc(float angle)
    {
        ApplyRotation(angle);
    }

    private void ApplyRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}