# Quick Start: Multiplayer & HUD Features

This guide provides quick examples for using the new multiplayer networking and HUD systems.

## Multiplayer Quick Start

### 1. Creating a Server

```csharp
using EvokerEngine.Networking;

var networkSystem = new NetworkingSystem();
networkSystem.StartServer(port: 7777);

Logger.Info("Server started on port 7777");
```

### 2. Creating a Client

```csharp
using EvokerEngine.Networking;

var networkSystem = new NetworkingSystem();
await networkSystem.ConnectAsClientAsync(
    host: "127.0.0.1",
    port: 7777,
    playerName: "Player1"
);

Logger.Info("Connected to server!");
```

### 3. Sending Messages

```csharp
// Chat message
var chatMsg = new ChatMessage
{
    Sender = "Player1",
    Message = "Hello, everyone!",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

if (networkSystem.IsServer)
{
    await networkSystem.Server.BroadcastAsync(chatMsg);
}
else
{
    await networkSystem.Client.SendAsync(chatMsg);
}
```

### 4. Syncing Items with Colon Notation

```csharp
// Inventory update with namespace:key format
await networkSystem.SendInventoryUpdateAsync(
    entity: playerEntity,
    slotIndex: 5,
    itemId: "game:diamond_sword",  // Uses namespace:key
    quantity: 1
);
```

### 5. Syncing Dimensions with Colon Notation

```csharp
// Dimension change with namespace:key format
await networkSystem.SendDimensionChangeAsync(
    entity: playerEntity,
    dimensionId: "evoker:nether",  // Uses namespace:key
    position: new Vector3(100, 64, 200)
);
```

## HUD Quick Start

### 1. Simple Text Display

```csharp
using EvokerEngine.UI;

var hud = HudManager.Instance;
var text = hud.AddText(
    text: "Score: 0",
    position: new Vector2(10, 10),
    fontSize: 24
);
```

### 2. Health Bar

```csharp
var healthBar = hud.AddHealthBar(
    position: new Vector2(10, 50),
    size: new Vector2(200, 20),
    currentHealth: 100,
    maxHealth: 100
);

// Update later
healthBar.Value = 75;
```

### 3. Player Stats HUD

```csharp
var statsPanel = hud.CreatePlayerStatsHUD(
    position: new Vector2(10, 10),
    health: 100, maxHealth: 100,
    mana: 50, maxMana: 100,
    stamina: 80, maxStamina: 100
);
```

### 4. Inventory Display

```csharp
var inventoryPanel = hud.CreateInventoryDisplay(
    position: new Vector2(10, 500),
    slots: 9,
    slotSize: 50
);
```

### 5. Button with Click Handler

```csharp
var button = hud.AddButton(
    text: "Start Game",
    position: new Vector2(100, 100),
    size: new Vector2(150, 50),
    onClick: () =>
    {
        Logger.Info("Game started!");
        StartGame();
    }
);
```

### 6. Crosshair

```csharp
hud.CreateCrosshair(size: 20);
```

### 7. Debug Panel

```csharp
var debugPanel = hud.CreateDebugPanel(
    position: new Vector2(10, 10),
    "FPS: 60",
    "Position: 100, 200, 300",
    "Health: 100/100"
);
```

## Complete Game Layer Example

```csharp
using EvokerEngine.Core;
using EvokerEngine.Networking;
using EvokerEngine.UI;

public class MultiplayerGameLayer : Layer
{
    private NetworkingSystem _network;
    private HudManager _hud;
    private HudBar _healthBar;
    private HudText _scoreText;
    private bool _isServer;
    
    public MultiplayerGameLayer(bool isServer = false) : base("Game")
    {
        _isServer = isServer;
    }
    
    public override void OnAttach()
    {
        // Setup networking
        _network = new NetworkingSystem();
        
        if (_isServer)
        {
            _network.StartServer(7777);
            _network.OnPlayerJoined += (clientId, name) =>
            {
                Logger.Info($"Player joined: {name}");
            };
        }
        else
        {
            _ = _network.ConnectAsClientAsync("127.0.0.1", 7777, "Player");
        }
        
        // Setup HUD
        _hud = HudManager.Instance;
        _hud.ScreenWidth = 1920;
        _hud.ScreenHeight = 1080;
        
        // Create HUD elements
        _healthBar = _hud.AddHealthBar(
            new Vector2(10, 10),
            new Vector2(200, 20),
            100, 100
        );
        
        _scoreText = _hud.AddText(
            "Score: 0",
            new Vector2(1800, 10),
            20
        );
        
        _hud.CreateCrosshair(20);
    }
    
    public override void OnUpdate(float deltaTime)
    {
        _hud.Update(deltaTime);
        
        // Update HUD
        var player = GetLocalPlayer();
        if (player != null)
        {
            _healthBar.Value = player.Health;
            _scoreText.Text = $"Score: {player.Score}";
            
            // Send updates to network
            if (_network.IsClient || _network.IsServer)
            {
                _ = _network.SendEntityUpdateAsync(
                    player.Entity,
                    player.Position,
                    player.Rotation,
                    player.Velocity
                );
            }
        }
    }
    
    public override void OnRender()
    {
        _hud.Render();
    }
    
    public override void OnDetach()
    {
        _network?.Shutdown();
        _hud?.Clear();
    }
    
    private Player? GetLocalPlayer()
    {
        // Your player logic here
        return null;
    }
}
```

## ResourceKey Examples

Items, dimensions, and inventories all support the `namespace:key` format:

```csharp
// Items
var sword = new SimpleItem("game:diamond_sword", "Diamond Sword");
var key = ResourceKey.Parse("game:diamond_sword");
Assert.Equal("game", key.Namespace);
Assert.Equal("diamond_sword", key.Key);

// Dimensions
var nether = new SimpleDimension("evoker:nether", "The Nether");
DimensionRegistry.Instance.Register(nether);
var dim = DimensionRegistry.Instance.Get("evoker:nether");

// Inventory with network sync
var inventory = new Inventory(20);
inventory.AddItem(sword, 1);

// Send over network
await networkSystem.SendInventoryUpdateAsync(
    playerEntity,
    slotIndex: 0,
    itemId: "game:diamond_sword",
    quantity: 1
);
```

## Running Your Game

```csharp
// Server mode
var app = new Application("My Game Server", 1280, 720);
app.PushLayer(new MultiplayerGameLayer(isServer: true));
app.Run();

// Client mode
var app = new Application("My Game Client", 1280, 720);
app.PushLayer(new MultiplayerGameLayer(isServer: false));
app.Run();
```

## Next Steps

1. Read the full documentation:
   - [Networking System](docs/systems/networking.md)
   - [HUD System](docs/systems/hud.md)
   - [Inventory System](docs/systems/inventory.md)
   - [Dimension System](docs/systems/dimensions.md)

2. Check out the tests for more examples:
   - `EvokerEngine.Tests/NetworkingTests.cs`
   - `EvokerEngine.Tests/HudTests.cs`
   - `EvokerEngine.Tests/InventoryTests.cs`
   - `EvokerEngine.Tests/DimensionTests.cs`

3. Explore the demo project:
   - `EvokerEngine.Demo/`
