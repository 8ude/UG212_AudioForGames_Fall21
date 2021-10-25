using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using UnityAtoms.BaseAtoms;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    [Header("Connection Parameters")]
    [SerializeField] private FloatReference connectionTimeout;

    [Header("Connection Events")]
    [SerializeField] private UnityEvent onStartClient;
    [SerializeField] private UnityEvent onStopClient;
    [SerializeField] private UnityEvent onStopHost;

    [SerializeField] private UnityEvent onClientConnect;
    [SerializeField] private UnityEvent onClientConnectTimeout;

    [Header("Exposed Variables") ]
    [SerializeField] private FloatReference timeout01;
    [SerializeField] private StringVariable hostIP;

    // -- private members --
    private float timeoutTimer = 0;

    // -- queries --
    private bool isClientConnected => NetworkClient.isConnected;

    private bool isDisconnectPressed => 
            Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKeyDown(KeyCode.L);

    // -- Overrides --
    public override void OnStartClient() => onStartClient?.Invoke();
    public override void OnStopClient() => onStopClient?.Invoke();
    public override void OnStopHost() => onStopHost?.Invoke();
    public override void OnStartHost() {
        StartCoroutine(GetIpRequest());
    }

    private string ipAddressGetterUrl = "http://ifconfig.io/ip";
    IEnumerator GetIpRequest()
    {
        var uri = ipAddressGetterUrl;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                var ip = webRequest.downloadHandler.text;
                hostIP.Value =  ip;
                GUIUtility.systemCopyBuffer = ip;
            }
        }
    }

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        
        onClientConnect?.Invoke();
    }

    // -- lifecycle --
    private void Update() {
        if (isClientConnected && isDisconnectPressed) {
            Disconnect();
        }

        if(NetworkClient.active && !isClientConnected) {
            timeoutTimer += Time.deltaTime;
            if(timeout01 != null) {
                timeout01.Value = Mathf.Clamp01(timeoutTimer/connectionTimeout.Value);
            }

            if(timeoutTimer > connectionTimeout.Value) {
                Disconnect();
                timeoutTimer = 0.0f;
                onClientConnectTimeout?.Invoke();
            }
        } else {
            timeoutTimer = 0.0f;
        }
    }

    // -- Commands --
    public void StartClient(string uri) {
        networkAddress = uri;
        hostIP.Value = uri;
        StartClient();
    }

    public void Disconnect() {
        // stop host if host mode
        if (NetworkServer.active && NetworkClient.active) {
            StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.active) {
            StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active) {
            StopServer();
        }
    }
}
