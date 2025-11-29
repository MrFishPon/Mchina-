using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;                 // LobbyService
using Unity.Services.Lobbies.Models;          // CreateLobbyOptions, DataObject
using Unity.Services.Relay;                   // RelayService
using Unity.Services.Relay.Models;            // Allocation, JoinAllocation, AllocationUtils
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.SceneManagement;

public class OnlineLobbyManager : MonoBehaviour
{
    [Header("UI ссылки")]
    public TMP_InputField joinCodeInput;
    public TMP_Text infoText;
    public GameObject startGameButton; // 👈 кнопка "Start Game" в Canvas

    private Lobby currentLobby;

    async void Start()
    {
        await InitServices();

         if (startGameButton != null)
            startGameButton.SetActive(false); // скрываем до создания лобби
    }


    private async Task InitServices()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"✅ Signed in as {AuthenticationService.Instance.PlayerId}");
        }
    }

    // Host: создаём Relay allocation + Lobby, настраиваем транспорт и стартуем Host
    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "Lobby_" + UnityEngine.Random.Range(1000, 9999);
            int maxPlayers = 4;

            // 1) Создаём Relay allocation
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            Debug.Log("Relay joinCode: " + joinCode);

            // 2) Создаём Lobby и сохраняем joinCode в Data
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            };

            // Используем LobbyService.Instance (не Lobbies.Instance)
            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log($"Lobby created: {currentLobby.Id}");

            // 3) Конвертируем allocation в RelayServerData и настраиваем UnityTransport
            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            // AllocationUtils.ToRelayServerData возвращает нужный тип RelayServerData для UTP
            var relayServerData = AllocationUtils.ToRelayServerData(alloc, "dtls");
            utp.SetRelayServerData(relayServerData);

            // 4) Стартуем Host
            NetworkManager.Singleton.StartHost();
            infoText.text = $"Hosting lobby!\nJoin Code: {joinCode}";

             if (startGameButton != null)
                startGameButton.SetActive(true); // 👈 показываем кнопку Start Game
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            infoText.text = "Error: " + e.Message;
        }
    }

    // Client: подключаемся по joinCode
    public async void JoinLobby()
    {
        try
        {
            string joinCode = joinCodeInput.text.Trim();
            if (string.IsNullOrEmpty(joinCode))
            {
                infoText.text = "Введите join code!";
                return;
            }

            // 1) Получаем JoinAllocation через Relay
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // 2) Конвертируем JoinAllocation в RelayServerData и настраиваем транспорт
            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayServerData = AllocationUtils.ToRelayServerData(joinAlloc, "dtls");
            utp.SetRelayServerData(relayServerData);

            // 3) Стартуем клиент
            bool ok = NetworkManager.Singleton.StartClient();
            infoText.text = ok ? "Connecting to relay..." : "StartClient failed";
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            infoText.text = "Error: " + e.Message;
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            if (currentLobby != null)
            {
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
                currentLobby = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to delete lobby: " + e.Message);
        }

        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
            NetworkManager.Singleton.Shutdown();

        infoText.text = "Left lobby.";
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only host can start the game!");
            return;
        }

        Debug.Log("Starting game... Loading RaceTrack scene");
        infoText.text = "Loading game...";

        // 👇 используем встроенный SceneManager из Netcode
        NetworkManager.Singleton.SceneManager.LoadScene("_Main-medium", LoadSceneMode.Single);
    }

    
}
