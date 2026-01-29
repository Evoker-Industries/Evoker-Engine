---
tags:
  - gamesystem
---

# Networking and Multiplayer System

The Networking system in Evoker Engine provides TCP-based client/server multiplayer functionality with support for entity synchronization, inventory updates, dimension changes, and more.

## Overview

The networking system consists of:

- **NetworkServer**: Host multiplayer games
- **NetworkClient**: Connect to multiplayer games  
- **NetworkConnection**: Manages individual client-server connections
- **NetworkMessage**: Base class for all network messages
- **NetworkingSystem**: ECS integration for multiplayer

## Creating a Server

### Basic Server Setup

```csharp
using EvokerEngine.Networking;

// Create and start server
var server = new NetworkServer(port: 7777);
server.Start();

// Handle client connections
server.OnClientConnected += (connection) =>
{
    Logger.Info($"Client connected: {connection.ConnectionId}");
};

server.OnClientDisconnected += (connection) =>
{
    Logger.Info($"Client disconnected: {connection.ConnectionId}");
};

// Handle messages
server.OnMessageReceived += (connection, message) =>
{
    switch (message.Type)
    {
        case MessageType.ChatMessage:
            var chat = (ChatMessage)message;
            Logger.Info($"{chat.Sender}: {chat.Message}");
            // Broadcast to all clients
            await server.BroadcastAsync(chat);
            break;
    }
};

// When done
server.Stop();
```

### Using NetworkingSystem as Server

```csharp
using EvokerEngine.Networking;

var networkSystem = new NetworkingSystem();

// Start as server
networkSystem.StartServer(port: 7777);

// Handle player joins
networkSystem.OnPlayerJoined += (clientId, playerName) =>
{
    Logger.Info($"Player joined: {playerName}");
    
    // Create player entity
    var scene = Application.Instance.ActiveScene;
    var playerEntity = scene.CreateEntity(playerName);
    
    // Associate entity with client
    networkSystem.AssociateEntityWithClient(playerEntity, clientId);
};

// Send entity updates
await networkSystem.SendEntityUpdateAsync(
    entity, 
    position, 
    rotation, 
    velocity
);
```

## Creating a Client

### Basic Client Setup

```csharp
using EvokerEngine.Networking;

// Create client
var client = new NetworkClient(host: "127.0.0.1", port: 7777);

// Handle connection
client.OnConnected += () =>
{
    Logger.Info("Connected to server!");
};

client.OnDisconnected += () =>
{
    Logger.Info("Disconnected from server");
};

// Handle messages
client.OnMessageReceived += (message) =>
{
    switch (message.Type)
    {
        case MessageType.EntityUpdate:
            var update = (EntityUpdateMessage)message;
            // Update entity position
            break;
    }
};

// Connect
await client.ConnectAsync();

// Send messages
var chatMsg = new ChatMessage
{
    Sender = "Player1",
    Message = "Hello, World!",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
await client.SendAsync(chatMsg);

// Disconnect
client.Disconnect();
```

### Using NetworkingSystem as Client

```csharp
var networkSystem = new NetworkingSystem();

// Connect to server
await networkSystem.ConnectAsClientAsync(
    host: "127.0.0.1",
    port: 7777,
    playerName: "MyPlayer"
);

// Handle connection events
networkSystem.OnConnectedToServer += () =>
{
    Logger.Info("Connected!");
};

networkSystem.OnDisconnectedFromServer += () =>
{
    Logger.Info("Disconnected!");
};
```

## Network Messages

### Built-in Message Types

```csharp
// Connect/Disconnect
var connectMsg = new ConnectMessage
{
    PlayerName = "Player1",
    Version = "1.0.0"
};

var disconnectMsg = new DisconnectMessage
{
    Reason = "Player quit"
};

// Ping/Pong for latency
var pingMsg = new PingMessage
{
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

// Chat
var chatMsg = new ChatMessage
{
    Sender = "Player1",
    Message = "Hello!",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

// Entity updates
var entityMsg = new EntityUpdateMessage
{
    EntityId = 123,
    Position = new Vector3(10, 0, 10),
    Rotation = new Vector3(0, 90, 0),
    Velocity = new Vector3(1, 0, 0)
};

// Inventory updates (uses namespace:key format)
var inventoryMsg = new InventoryUpdateMessage
{
    EntityId = 123,
    SlotIndex = 5,
    ItemId = "game:diamond_sword", // namespace:key
    Quantity = 1
};

// Dimension changes (uses namespace:key format)
var dimensionMsg = new DimensionChangeMessage
{
    EntityId = 123,
    DimensionId = "evoker:nether", // namespace:key
    Position = new Vector3(100, 64, 200)
};
```

### Creating Custom Messages

```csharp
public class CustomGameMessage : NetworkMessage
{
    public override MessageType Type => MessageType.Custom;
    
    public string CustomData { get; set; } = string.Empty;
    
    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        WriteString(writer, CustomData);
        return ms.ToArray();
    }
    
    public override void Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        CustomData = ReadString(reader);
    }
}
```

## Complete Multiplayer Example

### Server Layer

```csharp
public class MultiplayerServerLayer : Layer
{
    private NetworkingSystem _network;
    private Dictionary<Guid, Entity> _players;
    
    public override void OnAttach()
    {
        _network = new NetworkingSystem();
        _players = new Dictionary<Guid, Entity>();
        
        _network.OnPlayerJoined += OnPlayerJoined;
        _network.OnPlayerLeft += OnPlayerLeft;
        
        _network.StartServer(7777);
        Logger.Info("Multiplayer server started");
    }
    
    private void OnPlayerJoined(Guid clientId, string playerName)
    {
        var scene = Application.Instance.ActiveScene;
        var playerEntity = scene.CreateEntity(playerName);
        
        // Add components
        var transform = scene.Registry.AddComponent<TransformComponent>(playerEntity);
        transform.Position = new Vector3(0, 0, 0);
        
        var inventory = scene.Registry.AddComponent<InventoryComponent>(playerEntity);
        
        _network.AssociateEntityWithClient(playerEntity, clientId);
        _players[clientId] = playerEntity;
        
        Logger.Info($"Player joined: {playerName}");
    }
    
    private void OnPlayerLeft(Guid clientId)
    {
        if (_players.TryGetValue(clientId, out var entity))
        {
            var scene = Application.Instance.ActiveScene;
            scene.DestroyEntity(entity);
            _players.Remove(clientId);
        }
    }
    
    public override void OnUpdate(float deltaTime)
    {
        _network.Update(deltaTime);
        
        // Sync player positions
        var scene = Application.Instance.ActiveScene;
        foreach (var kvp in _players)
        {
            var entity = kvp.Value;
            var transform = scene.Registry.GetComponent<TransformComponent>(entity);
            if (transform != null)
            {
                _ = _network.SendEntityUpdateAsync(
                    entity,
                    transform.Position,
                    transform.Rotation,
                    Vector3.Zero
                );
            }
        }
    }
    
    public override void OnDetach()
    {
        _network?.Shutdown();
    }
}
```

### Client Layer

```csharp
public class MultiplayerClientLayer : Layer
{
    private NetworkingSystem _network;
    private Entity _localPlayer;
    
    public override void OnAttach()
    {
        _network = new NetworkingSystem();
        
        _network.OnConnectedToServer += () =>
        {
            Logger.Info("Connected to server!");
            CreateLocalPlayer();
        };
        
        _ = _network.ConnectAsClientAsync("127.0.0.1", 7777, "MyPlayer");
    }
    
    private void CreateLocalPlayer()
    {
        var scene = Application.Instance.ActiveScene;
        _localPlayer = scene.CreateEntity("LocalPlayer");
        
        var transform = scene.Registry.AddComponent<TransformComponent>(_localPlayer);
        transform.Position = new Vector3(0, 0, 0);
    }
    
    public override void OnUpdate(float deltaTime)
    {
        if (_network.IsClient && _localPlayer.Id != 0)
        {
            var scene = Application.Instance.ActiveScene;
            var transform = scene.Registry.GetComponent<TransformComponent>(_localPlayer);
            
            if (transform != null)
            {
                // Send position updates
                _ = _network.SendEntityUpdateAsync(
                    _localPlayer,
                    transform.Position,
                    transform.Rotation,
                    Vector3.Zero
                );
            }
        }
    }
    
    public override void OnDetach()
    {
        _network?.Shutdown();
    }
}
```

## Items, Dimensions, and Inventories with Colon Notation

The networking system fully supports the ResourceKey `namespace:key` format for items, dimensions, and inventories:

```csharp
// Item synchronization
var itemMsg = new InventoryUpdateMessage
{
    ItemId = "minecraft:diamond_sword", // namespace:key format
    Quantity = 1
};

// Dimension synchronization  
var dimMsg = new DimensionChangeMessage
{
    DimensionId = "evoker:nether", // namespace:key format
    Position = new Vector3(100, 64, 200)
};

// Parse received items
var itemKey = ResourceKey.Parse(itemMsg.ItemId);
var item = ItemRegistry.Get(itemKey);

// Parse received dimensions
var dimKey = ResourceKey.Parse(dimMsg.DimensionId);
var dimension = DimensionRegistry.Instance.Get(dimKey);
```

## Best Practices

### ✅ Do's

- Use NetworkingSystem for ECS integration
- Handle disconnections gracefully
- Validate all received data
- Use async/await for network operations
- Implement timeout/retry logic
- Compress large messages
- Use ResourceKey format for items/dimensions

### ❌ Don'ts

- Don't send messages too frequently (throttle updates)
- Don't trust client input without validation
- Don't block the main thread with network operations
- Don't send large amounts of data per message (> 10MB)
- Don't forget to dispose connections

## Performance Tips

1. **Throttle Updates**: Don't send entity updates every frame
2. **Batch Messages**: Combine multiple updates when possible
3. **Compression**: Compress data for large messages
4. **Delta Updates**: Only send changed values
5. **Priority System**: Send critical updates first

## See Also

- [ECS](../core/ecs.md) - Entity Component System
- [Inventory](../systems/inventory.md) - Inventory system
- [Dimensions](../systems/dimensions.md) - Dimension system
