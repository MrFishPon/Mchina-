using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

public class CameraFollowSetter : MonoBehaviour
{
    private CinemachineCamera vcam;

    void Start()
    {
        vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam == null)
        {
            Debug.LogError("Virtual Camera not found in scene!");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // “олько локальный клиент настраивает свою камеру
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        // Ќаходим своего игрока
        var myPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (myPlayer != null)
        {
            vcam.Follow = myPlayer.transform;
            vcam.LookAt = myPlayer.transform;
        }
    }
}