using System;
using System.Collections.Generic;
using EvokerEngine.Core;
using EvokerEngine.ECS;

namespace EvokerEngine.Networking;

/// <summary>
/// System for managing networking in the ECS
/// </summary>
public class NetworkingSystem
{
    private NetworkServer? _server;
    private NetworkClient? _client;
    private bool _isServer;
    private readonly Dictionary<uint, Guid> _entityToClient;
    private readonly Dictionary<Guid, uint> _clientToEntity;

    /// <summary>
    /// Whether this instance is running as a server
    /// </summary>
    public bool IsServer => _isServer;

    /// <summary>
    /// Whether this instance is running as a client
    /// </summary>
    public bool IsClient => _client != null;

    /// <summary>
    /// Server instance (null if not running as server)
    /// </summary>
    public NetworkServer? Server => _server;

    /// <summary>
    /// Client instance (null if not running as client)
    /// </summary>
    public NetworkClient? Client => _client;

    /// <summary>
    /// Event fired when a player connects (server only)
    /// </summary>
    public event Action<Guid, string>? OnPlayerJoined;

    /// <summary>
    /// Event fired when a player disconnects (server only)
    /// </summary>
    public event Action<Guid>? OnPlayerLeft;

    /// <summary>
    /// Event fired when connected to server (client only)
    /// </summary>
    public event Action? OnConnectedToServer;

    /// <summary>
    /// Event fired when disconnected from server (client only)
    /// </summary>
    public event Action? OnDisconnectedFromServer;

    public NetworkingSystem()
    {
        _entityToClient = new Dictionary<uint, Guid>();
        _clientToEntity = new Dictionary<Guid, uint>();
    }

    /// <summary>
    /// Start as a server
    /// </summary>
    public void StartServer(int port = 7777)
    {
        if (_server != null)
        {
            Logger.Warning("Server is already running");
            return;
        }

        _server = new NetworkServer(port);
        _server.OnClientConnected += OnClientConnected;
        _server.OnClientDisconnected += OnClientDisconnected;
        _server.OnMessageReceived += OnServerMessageReceived;
        _server.Start();

        _isServer = true;
        Logger.Info($"Started as server on port {port}");
    }

    /// <summary>
    /// Stop the server
    /// </summary>
    public void StopServer()
    {
        if (_server == null)
            return;

        _server.Stop();
        _server.Dispose();
        _server = null;
        _isServer = false;

        Logger.Info("Server stopped");
    }

    /// <summary>
    /// Connect as a client
    /// </summary>
    public async System.Threading.Tasks.Task<bool> ConnectAsClientAsync(string host = "127.0.0.1", int port = 7777, string playerName = "Player")
    {
        if (_client != null)
        {
            Logger.Warning("Client is already connected");
            return false;
        }

        _client = new NetworkClient(host, port);
        _client.OnConnected += () =>
        {
            OnConnectedToServer?.Invoke();
            Logger.Info($"Connected as client to {host}:{port}");
        };
        _client.OnDisconnected += () =>
        {
            OnDisconnectedFromServer?.Invoke();
            Logger.Info("Disconnected from server");
        };
        _client.OnMessageReceived += OnClientMessageReceived;

        var success = await _client.ConnectAsync();
        if (success)
        {
            // Send connect message
            var connectMsg = new ConnectMessage
            {
                PlayerName = playerName,
                Version = "1.0.0"
            };
            await _client.SendAsync(connectMsg);
        }

        return success;
    }

    /// <summary>
    /// Disconnect as a client
    /// </summary>
    public void DisconnectClient()
    {
        if (_client == null)
            return;

        _client.Disconnect();
        _client.Dispose();
        _client = null;

        Logger.Info("Client disconnected");
    }

    /// <summary>
    /// Associate an entity with a client connection
    /// </summary>
    public void AssociateEntityWithClient(Entity entity, Guid clientId)
    {
        _entityToClient[entity.Id] = clientId;
        _clientToEntity[clientId] = entity.Id;
    }

    /// <summary>
    /// Get the client ID associated with an entity
    /// </summary>
    public Guid? GetClientForEntity(Entity entity)
    {
        return _entityToClient.TryGetValue(entity.Id, out var clientId) ? clientId : null;
    }

    /// <summary>
    /// Get the entity ID associated with a client
    /// </summary>
    public uint? GetEntityForClient(Guid clientId)
    {
        return _clientToEntity.TryGetValue(clientId, out var entityId) ? entityId : null;
    }

    /// <summary>
    /// Handle client connection (server)
    /// </summary>
    private void OnClientConnected(NetworkConnection connection)
    {
        Logger.Info($"Client connected: {connection.ConnectionId}");
    }

    /// <summary>
    /// Handle client disconnection (server)
    /// </summary>
    private void OnClientDisconnected(NetworkConnection connection)
    {
        Logger.Info($"Client disconnected: {connection.ConnectionId}");
        
        // Clean up associations
        if (_clientToEntity.TryGetValue(connection.ConnectionId, out var entityId))
        {
            _entityToClient.Remove(entityId);
            _clientToEntity.Remove(connection.ConnectionId);
        }

        OnPlayerLeft?.Invoke(connection.ConnectionId);
    }

    /// <summary>
    /// Handle messages received by server
    /// </summary>
    private void OnServerMessageReceived(NetworkConnection connection, NetworkMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Connect:
                var connectMsg = (ConnectMessage)message;
                OnPlayerJoined?.Invoke(connection.ConnectionId, connectMsg.PlayerName);
                break;

            case MessageType.Ping:
                // Respond with pong
                var pingMsg = (PingMessage)message;
                var pongMsg = new PongMessage { Timestamp = pingMsg.Timestamp };
                _ = connection.SendAsync(pongMsg);
                break;

            case MessageType.EntityUpdate:
            case MessageType.InventoryUpdate:
            case MessageType.DimensionChange:
                // Broadcast to all other clients
                if (_server != null)
                {
                    _ = _server.BroadcastExceptAsync(connection.ConnectionId, message);
                }
                break;
        }
    }

    /// <summary>
    /// Handle messages received by client
    /// </summary>
    private void OnClientMessageReceived(NetworkMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Pong:
                var pongMsg = (PongMessage)message;
                var latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - pongMsg.Timestamp;
                Logger.Info($"Latency: {latency}ms");
                break;

            case MessageType.EntityUpdate:
                // Update entity from server
                break;

            case MessageType.InventoryUpdate:
                // Update inventory from server
                break;

            case MessageType.DimensionChange:
                // Handle dimension change from server
                break;
        }
    }

    /// <summary>
    /// Send an entity update to all clients (server) or to server (client)
    /// </summary>
    public async System.Threading.Tasks.Task SendEntityUpdateAsync(Entity entity, System.Numerics.Vector3 position, System.Numerics.Vector3 rotation, System.Numerics.Vector3 velocity)
    {
        var message = new EntityUpdateMessage
        {
            EntityId = entity.Id,
            Position = position,
            Rotation = rotation,
            Velocity = velocity
        };

        if (_isServer && _server != null)
        {
            await _server.BroadcastAsync(message);
        }
        else if (_client != null && _client.IsConnected)
        {
            await _client.SendAsync(message);
        }
    }

    /// <summary>
    /// Send an inventory update
    /// </summary>
    public async System.Threading.Tasks.Task SendInventoryUpdateAsync(Entity entity, int slotIndex, string itemId, int quantity)
    {
        var message = new InventoryUpdateMessage
        {
            EntityId = entity.Id,
            SlotIndex = slotIndex,
            ItemId = itemId,
            Quantity = quantity
        };

        if (_isServer && _server != null)
        {
            await _server.BroadcastAsync(message);
        }
        else if (_client != null && _client.IsConnected)
        {
            await _client.SendAsync(message);
        }
    }

    /// <summary>
    /// Send a dimension change
    /// </summary>
    public async System.Threading.Tasks.Task SendDimensionChangeAsync(Entity entity, string dimensionId, System.Numerics.Vector3 position)
    {
        var message = new DimensionChangeMessage
        {
            EntityId = entity.Id,
            DimensionId = dimensionId,
            Position = position
        };

        if (_isServer && _server != null)
        {
            await _server.BroadcastAsync(message);
        }
        else if (_client != null && _client.IsConnected)
        {
            await _client.SendAsync(message);
        }
    }

    /// <summary>
    /// Shutdown the networking system
    /// </summary>
    public void Shutdown()
    {
        StopServer();
        DisconnectClient();
        _entityToClient.Clear();
        _clientToEntity.Clear();
    }
}
