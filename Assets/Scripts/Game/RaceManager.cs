using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;

    [SerializeField] private int totalLaps = 3;

    private List<PlayerRaceCounter> players = new List<PlayerRaceCounter>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        
    }

    public void RegisterPlayer(PlayerRaceCounter p)
    {
        if (!players.Contains(p)) players.Add(p);
        Debug.Log($"Player {p.OwnerClientId} registered.");
    }

    public void UnregisterPlayer(PlayerRaceCounter p)
    {
        if (players.Contains(p)) players.Remove(p);
    }

    // Вызывается на сервере из PlayerRaceCounter
    public void OnPlayerCompletedLap(ulong playerId, int lapCount)
    {
        if (!IsServer) return;

        Debug.Log($"RaceManager: player {playerId} lap {lapCount}/{totalLaps}");
        if (lapCount >= totalLaps)
        {
            Debug.Log($"Player {playerId} finished the race!");
            // TODO: выделить победителя, расставить финишировавших, оповестить клиентов и т.д.
        }
    }

    
}
