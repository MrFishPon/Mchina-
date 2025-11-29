using System;
using System.Text;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientId;
    public FixedString32Bytes playerName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
    }

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && playerName.Equals(other.playerName);
    }

    public override bool Equals(object obj) => obj is PlayerData other && Equals(other);

    public override int GetHashCode() => playerName.GetHashCode() ^ clientId.GetHashCode();

    public static bool operator ==(PlayerData left, PlayerData right) => left.Equals(right);
    public static bool operator !=(PlayerData left, PlayerData right) => !left.Equals(right);
}

public class LobbyManager : NetworkBehaviour
{
    public TMP_Text playersText;
    private NetworkList<PlayerData> players;

    void Awake()
    {
        players = new NetworkList<PlayerData>();
    }

    public override void OnNetworkSpawn() //override = перезаписать
    {
        players.OnListChanged += OnPlayersListChanged;

        if (IsServer)
        {
            // Подписываемся на подключение/отключение
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        RefreshUI();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        players.OnListChanged -= OnPlayersListChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        AddPlayer(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
        RefreshUI();
    }

    private void AddPlayer(ulong clientId)
    {
        var name = new FixedString32Bytes($"Player{clientId}");
        var playerData = new PlayerData { clientId = clientId, playerName = name };
        players.Add(playerData);
        RefreshUI();
    }

    private void OnPlayersListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (playersText == null) return;
        StringBuilder sb = new StringBuilder("Players:\n");
        foreach (var p in players)
            sb.AppendLine($"{p.playerName.ToString()}");
        playersText.text = sb.ToString();
    }

    public void StartGame()
    {
        if (!IsServer) return;
        NetworkManager.SceneManager.LoadScene("_Main-medium", LoadSceneMode.Single);
    }
}
