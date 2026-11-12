using UnityEngine;
using Unity.Netcode;

public class NetworkHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private NetworkVariable<int> currentHealth =
        new NetworkVariable<int>(
            100,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Vector3 initialSpawnPosition;
    private bool isDead;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            initialSpawnPosition = transform.position;
            isDead = false;
        }

        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer || isDead || damage <= 0)
            return;

        currentHealth.Value = Mathf.Max(
            0,
            currentHealth.Value - damage
        );

        if (currentHealth.Value == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!IsServer || isDead)
            return;

        isDead = true;

        ulong clientId = OwnerClientId;

        Debug.Log($"{name} died. Respawning...");

        if (NetworkRespawnManager.Instance != null)
        {
            NetworkRespawnManager.Instance.RespawnPlayer(
                clientId,
                initialSpawnPosition
            );
        }
        else
        {
            Debug.LogError("NetworkRespawnManager not found!");
        }

        NetworkObject.Despawn(true);
    }

    private void OnHealthChanged(int previousHealth, int newHealth)
    {
        Debug.Log($"{name}: HP = {newHealth}");
    }
}