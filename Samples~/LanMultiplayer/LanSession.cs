using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace HorrorToolkit.Samples
{
    [DisallowMultipleComponent]
    public sealed class LanSession : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private string hostAddress = "127.0.0.1";
        [SerializeField, Range(1, 65535)] private int port = 7777;
        public string Status { get; private set; } = "Idle";
        private void Awake()
        {
            if (networkManager == null) networkManager = NetworkManager.Singleton;
        }
        public void SetHostAddress(string address)
        {
            if (!string.IsNullOrWhiteSpace(address)) hostAddress = address.Trim();
        }
        public void StartHost()
        {
            if (!TryGetTransport(out var transport)) return;
            transport.SetConnectionData("127.0.0.1", (ushort)Mathf.Clamp(port, 1, 65535), "0.0.0.0");
            Status = networkManager.StartHost() ? "Hosting" : "Host failed";
            Debug.Log(Status, this);
        }
        public void StartClient()
        {
            if (!TryGetTransport(out var transport)) return;
            transport.SetConnectionData(hostAddress, (ushort)Mathf.Clamp(port, 1, 65535));
            Status = networkManager.StartClient() ? "Client starting" : "Client failed";
            Debug.Log(Status, this);
        }
        public void Disconnect()
        {
            if (networkManager != null && networkManager.IsListening) networkManager.Shutdown();
            Status = "Disconnected";
        }
        private bool TryGetTransport(out UnityTransport transport)
        {
            transport = null;
            if (networkManager == null) networkManager = NetworkManager.Singleton;
            if (networkManager == null || networkManager.IsListening || networkManager.ShutdownInProgress)
            {
                Debug.LogWarning("NetworkManager missing, busy, or shutting down.", this);
                return false;
            }
            transport = networkManager.GetComponent<UnityTransport>();
            if (transport == null || networkManager.NetworkConfig.NetworkTransport != transport)
            {
                Debug.LogWarning("Assign UnityTransport to the NetworkManager.", this);
                return false;
            }
            return true;
        }
    }
}
