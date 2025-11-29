using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
public class ConnectionUI : MonoBehaviour
{

    public Button hostButton;
    public Button joinButton;
    public TMP_InputField ipInput;
    public TMP_InputField portInput;

    UnityTransport unityTransport;

    private void Start ()
    {

        unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        hostButton.onClick.AddListener(StartHost);

        joinButton.onClick.AddListener(StartClient);

    }

    private void StartHost()
    {

        ushort port = ushort.Parse(portInput.text);

        unityTransport.SetConnectionData("0.0.0.0", port);

        NetworkManager.Singleton.StartHost();

        Debug.Log($"Started Host on port {port}");

    }
    private void StartClient()
    {

        string ip = ipInput.text;

        ushort port = ushort.Parse(portInput.text);

        unityTransport.SetConnectionData(ip, port);

        NetworkManager.Singleton.StartClient();

        Debug.Log($"Connecting to {ip}:{port}");

    }

    private void OnDestroy()
    {
        hostButton.onClick.RemoveAllListeners();

        joinButton.onClick.RemoveAllListeners();
    }









}
