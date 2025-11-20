using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EvokerEngine.Core;

namespace EvokerEngine.Networking;

/// <summary>
/// Network server for multiplayer games
/// </summary>
public class NetworkServer : IDisposable
{
    private TcpListener? _listener;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _acceptTask;
    private readonly ConcurrentDictionary<Guid, NetworkConnection> _connections;

    /// <summary>
    /// Server port
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Whether the server is running
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Number of connected clients
    /// </summary>
    public int ClientCount => _connections.Count;

    /// <summary>
    /// All connected clients
    /// </summary>
    public IEnumerable<NetworkConnection> Connections => _connections.Values;

    /// <summary>
    /// Event fired when a client connects
    /// </summary>
    public event Action<NetworkConnection>? OnClientConnected;

    /// <summary>
    /// Event fired when a client disconnects
    /// </summary>
    public event Action<NetworkConnection>? OnClientDisconnected;

    /// <summary>
    /// Event fired when a message is received from any client
    /// </summary>
    public event Action<NetworkConnection, NetworkMessage>? OnMessageReceived;

    public NetworkServer(int port = 7777)
    {
        Port = port;
        _connections = new ConcurrentDictionary<Guid, NetworkConnection>();
    }

    /// <summary>
    /// Start the server
    /// </summary>
    public void Start()
    {
        if (IsRunning)
        {
            Logger.Warning("Server is already running");
            return;
        }

        try
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            IsRunning = true;

            _cancellationTokenSource = new CancellationTokenSource();
            _acceptTask = Task.Run(() => AcceptLoop(_cancellationTokenSource.Token));

            Logger.Info($"Server started on port {Port}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to start server: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Stop the server
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
            return;

        IsRunning = false;

        // Cancel accept loop
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        // Disconnect all clients
        foreach (var connection in _connections.Values)
        {
            connection.Close();
        }
        _connections.Clear();

        // Stop listener
        _listener?.Stop();
        _listener = null;

        Logger.Info("Server stopped");
    }

    /// <summary>
    /// Accept incoming connections
    /// </summary>
    private async Task AcceptLoop(CancellationToken cancellationToken)
    {
        if (_listener == null)
            return;

        try
        {
            while (!cancellationToken.IsCancellationRequested && IsRunning)
            {
                var client = await _listener.AcceptTcpClientAsync();
                var connection = new NetworkConnection(client);

                // Set up event handlers
                connection.OnMessageReceived += (msg) => OnMessageReceived?.Invoke(connection, msg);
                connection.OnDisconnected += OnConnectionDisconnected;

                // Add to connections
                _connections.TryAdd(connection.ConnectionId, connection);

                // Start receiving
                connection.StartReceiving();

                Logger.Info($"Client connected: {connection.RemoteEndPoint} (ID: {connection.ConnectionId})");
                OnClientConnected?.Invoke(connection);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            Logger.Error($"Error accepting connections: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle connection disconnection
    /// </summary>
    private void OnConnectionDisconnected(NetworkConnection connection)
    {
        _connections.TryRemove(connection.ConnectionId, out _);
        Logger.Info($"Client disconnected: {connection.RemoteEndPoint} (ID: {connection.ConnectionId})");
        OnClientDisconnected?.Invoke(connection);
    }

    /// <summary>
    /// Send message to a specific client
    /// </summary>
    public async Task SendToClientAsync(Guid clientId, NetworkMessage message)
    {
        if (_connections.TryGetValue(clientId, out var connection))
        {
            await connection.SendAsync(message);
        }
    }

    /// <summary>
    /// Broadcast message to all clients
    /// </summary>
    public async Task BroadcastAsync(NetworkMessage message)
    {
        var tasks = _connections.Values.Select(c => c.SendAsync(message));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Broadcast message to all clients except one
    /// </summary>
    public async Task BroadcastExceptAsync(Guid exceptClientId, NetworkMessage message)
    {
        var tasks = _connections.Values
            .Where(c => c.ConnectionId != exceptClientId)
            .Select(c => c.SendAsync(message));
        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        Stop();
    }
}
