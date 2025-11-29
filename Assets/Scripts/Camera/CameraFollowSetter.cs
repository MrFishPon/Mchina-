using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

public class CameraFollowSetter : MonoBehaviour
{
    private CinemachineCamera vcam;

    private void Start()
    {
        vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam == null)
        {
            Debug.LogError("Virtual Camera not found in scene!");
            return;
        }

        // Пытаемся привязать сразу (на случай хоста)
        TryAttachToLocalPlayer();

        // Также ждём появления игрока (на случай загрузки сцены)
        NetworkManager.Singleton.OnClientConnectedCallback += (_) => TryAttachToLocalPlayer();
        NetworkManager.Singleton.OnServerStarted += TryAttachToLocalPlayer;
    }

    private void TryAttachToLocalPlayer()
    {
        var myPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (myPlayer != null)
        {
            vcam.Follow = myPlayer.transform;
            vcam.LookAt = myPlayer.transform;
            Debug.Log($"Camera attached to player {myPlayer.OwnerClientId}");
        }
        else
        {
            // Если игрок ещё не заспавнен — подождём немного
            Invoke(nameof(TryAttachToLocalPlayer), 0.5f);
        }
    }
}
