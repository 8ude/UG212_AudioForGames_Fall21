// vis2k: GUILayout instead of spacey += ...; removed Update hotkeys to avoid
// confusion if someone accidentally presses one.

using Mirror;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// <summary>
/// An extension for the NetworkManager that displays a default HUD for controlling the network state of the game.
/// <para>This component also shows useful internal state for the networking system in the inspector window of the editor. It allows users to view connections, networked objects, message handlers, and packet statistics. This information can be helpful when debugging networked games.</para>
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Network/NetworkUi")]
[RequireComponent(typeof(NetworkManager))]
[HelpURL("https://mirror-networking.com/docs/Components/NetworkManagerHUD.html")]
public class NetworkUi : MonoBehaviour
{
    NetworkManager manager;

    /// <summary>
    /// Whether to show the default control HUD at runtime.
    /// </summary>
    public bool showGUI = true;

    /// <summary>
    /// The horizontal offset in pixels to draw the HUD runtime GUI at.
    /// </summary>
    public int offsetX;

    /// <summary>
    /// The vertical offset in pixels to draw the HUD runtime GUI at.
    /// </summary>
    public int offsetY;

    public BoolVariable cursorVisible;

    private void Awake() {
        manager = GetComponent<NetworkManager>();
    }

    private void Update() {
        if (IsClientConnected() && IsDisconnectPressed()) {
            Disconnect();
        }
    }

    private void OnGUI() {
        if (!showGUI) {
            return;
        }

        GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, 215, 9999));

        if (!NetworkClient.isConnected && !NetworkServer.active) {
            StartButtons();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (!NetworkClient.active)
        {
            // Server + Client
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (GUILayout.Button("Host (Server + Client)"))
                {
                    manager.StartHost();
                }
            }

            // Client + IP
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Client"))
            {
                manager.StartClient();
            }
            manager.networkAddress = GUILayout.TextField(manager.networkAddress);
            GUILayout.EndHorizontal();

            // Server Only
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // cant be a server in webgl build
                GUILayout.Box("(  WebGL cannot be server  )");
            }
            else
            {
                if (GUILayout.Button("Server Only")) manager.StartServer();
            }
        }
        else
        {
            // Connecting
            GUILayout.Label("Connecting to " + manager.networkAddress + "..");
            if (GUILayout.Button("Cancel Connection Attempt"))
            {
                manager.StopClient();
            }
        }
    }

    // -- commands --
    private void Disconnect() {
        cursorVisible.Value = true;

        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected) {
            manager.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected) {
            manager.StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active) {
            manager.StopServer();
        }
    }

    // -- queries --
    private bool IsClientConnected() {
        return NetworkClient.isConnected;
    }

    private bool IsDisconnectPressed() {
        return (
            Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKeyDown(KeyCode.L)
        );
    }
}
