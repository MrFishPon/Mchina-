using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Spawn : NetworkBehaviour
{
    public Transform[] spawnPoints;
    private static int nextSpawnIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        StartCoroutine(SpawnPlayer(clientId));
    }

    private IEnumerator SpawnPlayer(ulong clientId)
    {
        NetworkObject player = null;

        while (player == null)
        {
            player = NetworkManager.Singleton.SpawnManager
                .GetPlayerNetworkObject(clientId);
            yield return null;
        }

        Transform point = spawnPoints[nextSpawnIndex];
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;

        // 🔴 2D ФИКС
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

        player.transform.position = point.position;

        yield return null;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.simulated = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}
