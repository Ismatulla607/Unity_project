using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkRespawnManager : MonoBehaviour
{
    public static NetworkRespawnManager Instance { get; private set; }

    [SerializeField] private NetworkObject playerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void RespawnPlayer(ulong clientId, Vector3 spawnPosition)
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
            return;

        StartCoroutine(RespawnRoutine(clientId, spawnPosition));
    }

    private IEnumerator RespawnRoutine(ulong clientId, Vector3 spawnPosition)
    {
        yield return null;

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            yield break;

        NetworkObject player = Instantiate(
            playerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        player.SpawnAsPlayerObject(clientId, true);
    }
}