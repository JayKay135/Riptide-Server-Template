using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Always make sure, that these enums are identical on the server as on the client side
/// </summary>
public enum ServerToClientId : ushort
{
    testMessage = 1
}

public enum ClientToServerId : ushort
{
    hololensConnected = 1,
    log = 2,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;

    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
            {
                _singleton = value;
            }
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }

        }
    }

    public Server Server { get; private set; }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    private bool ServerStarted = false;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        StartServer();
    }


    /// <summary>
    /// Starts the actual riptide server
    /// </summary>
    public static void StartServer()
    {
        Singleton.Server = new Server();
        Singleton.Server.Start(Singleton.port, Singleton.maxClientCount);
        Singleton.Server.ClientDisconnected += Singleton.ClientDisconnected;

        Debug.Log("started server");

        Singleton.ServerStarted = true;
    }

    private void FixedUpdate()
    {
        if (ServerStarted)
        {
            Server.Tick();
        }
    }

    private void OnApplicationQuit()
    {
        if (ServerStarted)
        {
            Server.Stop();
        }
    }

    /// <summary>
    /// Callback on client disconnection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        if (HoloLens.HoloLenses.ContainsKey(e.Id))
        {
            HoloLens.HoloLenses.Remove(e.Id);
            Log.AddLog(new Log("HoloLens [" + e.Id + "] disconnected", Log.LogType.Normal));
        }
    }
}
