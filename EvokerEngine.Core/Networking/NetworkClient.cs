using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using EvokerEngine.Core;

namespace EvokerEngine.Networking;

/// <summary>
/// Network client for multiplayer games
/// </summary>
public class NetworkClient : IDisposable
{
    private NetworkConnection? _connection;
    private TcpClient? _tcpClient;

    /// <summary>
    /// Server host address
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Server port
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Whether the client is connected
    /// </summary>
    public bool IsConnected => _connection?.IsConnected ?? false;

    /// <summary>
    /// Event fired when connected to server
    /// </summary>
    public event Action? OnConnected;

    /// <summary>
    /// Event fired when disconnected from server
    /// </summary>
    public event Action? OnDisconnected;

    /// <summary>
    /// Event fired when a message is received from server
    /// </summary>
    public event Action<NetworkMessage>? OnMessageReceived;

    public NetworkClient(string host = "127.0.0.1", int port = 7777)
    {
        Host = host;
        Port = port;
    }

    /// <summary>
    /// Connect to the server
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        if (IsConnected)
        {
            Logger.Warning("Already connected to server");
            return false;
        }

        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(Host, Port);

            _connection = new NetworkConnection(_tcpClient);
            _connection.OnMessageReceived += OnMessageReceivedHandler;
            _connection.OnDisconnected += OnDisconnectedHandler;
            _connection.StartReceiving();

            Logger.Info($"Connected to server at {Host}:{Port}");
            OnConnected?.Invoke();

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to connect to server: {ex.Message}");
            _tcpClient?.Close();
            _tcpClient = null;
            _connection = null;
            return false;
        }
    }

    /// <summary>
    /// Disconnect from the server
    /// </summary>
    public void Disconnect()
    {
        if (!IsConnected)
            return;

        _connection?.Close();
        _connection = null;

        _tcpClient?.Close();
        _tcpClient = null;

        Logger.Info("Disconnected from server");
    }

    /// <summary>
    /// Send a message to the server
    /// </summary>
    public async Task SendAsync(NetworkMessage message)
    {
        if (!IsConnected || _connection == null)
            throw new InvalidOperationException("Not connected to server");

        await _connection.SendAsync(message);
    }

    /// <summary>
    /// Handle incoming messages
    /// </summary>
    private void OnMessageReceivedHandler(NetworkMessage message)
    {
        OnMessageReceived?.Invoke(message);
    }

    /// <summary>
    /// Handle disconnection
    /// </summary>
    private void OnDisconnectedHandler(NetworkConnection connection)
    {
        Logger.Info("Connection to server lost");
        OnDisconnected?.Invoke();
    }

    public void Dispose()
    {
        Disconnect();
    }
}
