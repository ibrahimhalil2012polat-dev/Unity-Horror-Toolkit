# LAN Multiplayer

1. Install Netcode for GameObjects and Unity Transport via Package Manager before importing the sample.
2. Create a NetworkManager GameObject with UnityTransport; assign it as the NetworkManager transport.
3. Configure a player prefab with NetworkObject. Add your own network-aware movement if needed.
4. Add LanSession to an active GameObject and assign the NetworkManager. Default port: 7777.
5. Connect UI buttons to StartHost, StartClient and Disconnect; wire an input field's OnEndEdit to SetHostAddress.
6. Start a host in one instance and a client in another. Use 127.0.0.1 on the same machine or the host's local IP on another device on the same network.

Verify actual connection via NetworkManager connection events or spawned player objects; the Status property only reports startup. Firewall settings and network isolation can affect LAN access. Do not expose this prototype to the public internet. No gameplay replication, room codes, relay, authentication or matchmaking is implemented.
