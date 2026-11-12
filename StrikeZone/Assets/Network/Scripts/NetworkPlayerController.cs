using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        Vector2 input = Vector2.zero;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
        }

        input = Vector2.ClampMagnitude(input, 1f);

        if (IsServer)
        {
            moveInput = input;
        }
        else
        {
            SubmitMovementRpc(input);
        }
    }

    [Rpc(SendTo.Server)]
    private void SubmitMovementRpc(Vector2 input)
    {
        moveInput = Vector2.ClampMagnitude(input, 1f);
    }

    private void FixedUpdate()
    {
        if (!IsSpawned || !IsServer)
            return;

        rb.linearVelocity = moveInput * moveSpeed;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && rb != null)
            rb.linearVelocity = Vector2.zero;

        moveInput = Vector2.zero;
    }
}