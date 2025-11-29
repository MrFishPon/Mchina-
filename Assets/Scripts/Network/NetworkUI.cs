using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    void OnGUI()
    {

        var nm = NetworkManager.Singleton;

        if (nm == null)
            return;

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();

            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();

            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        }
    }
}