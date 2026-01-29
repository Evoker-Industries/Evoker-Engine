---
tags:
  - gamesystem
---

# HUD (Heads-Up Display) System

The HUD system in Evoker Engine provides an easy-to-use API for creating and managing on-screen UI elements like health bars, text displays, buttons, panels, and more.

## Overview

The HUD system consists of:

- **HudManager**: Central manager for all HUD elements
- **HudElement**: Base class for all HUD elements
- **HudText**: Display text on screen
- **HudImage**: Display images/sprites
- **HudBar**: Health/progress bars
- **HudPanel**: Container/background panels
- **HudButton**: Interactive buttons

## Quick Start

### Simple Text Display

```csharp
using EvokerEngine.UI;

// Get the HUD manager
var hud = HudManager.Instance;

// Add text
var text = hud.AddText(
    text: "Hello, World!",
    position: new Vector2(10, 10),
    fontSize: 24
);
```

### Health Bar

```csharp
// Add health bar
var healthBar = hud.AddHealthBar(
    position: new Vector2(10, 10),
    size: new Vector2(200, 20),
    currentHealth: 75,
    maxHealth: 100
);

// Update health
healthBar.Value = 50;
```

### Image Display

```csharp
// Add image
var playerIcon = hud.AddImage(
    textureId: "player_icon",
    position: new Vector2(50, 50),
    size: new Vector2(64, 64)
);
```

## HUD Elements

### HudText

Display text on screen with customizable font, size, and color.

```csharp
var text = new HudText
{
    Text = "Score: 1000",
    Position = new Vector2(100, 50),
    FontSize = 20,
    Color = new Vector4(1, 1, 0, 1), // Yellow
    Alignment = TextAlignment.Center,
    DrawShadow = true,
    ShadowOffset = new Vector2(2, 2)
};

hud.AddElement(text);
```

### HudImage

Display images and sprites.

```csharp
var image = new HudImage
{
    TextureId = "sword_icon",
    Position = new Vector2(10, 10),
    Size = new Vector2(32, 32),
    Tint = new Vector4(1, 1, 1, 1),
    Alpha = 0.8f
};

hud.AddElement(image);
```

### HudBar

Progress/health/mana bars.

```csharp
var healthBar = new HudBar
{
    Position = new Vector2(10, 10),
    Size = new Vector2(200, 20),
    Value = 75,
    MaxValue = 100,
    FillColor = new Vector4(1, 0, 0, 1), // Red
    BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 0.8f),
    BorderColor = new Vector4(1, 1, 1, 1),
    BorderThickness = 2,
    FillDirection = BarFillDirection.LeftToRight
};

hud.AddElement(healthBar);
```

### HudPanel

Container panels with background and borders.

```csharp
var panel = new HudPanel
{
    Position = new Vector2(100, 100),
    Size = new Vector2(300, 200),
    BackgroundColor = new Vector4(0, 0, 0, 0.7f),
    BorderColor = new Vector4(1, 1, 1, 1),
    BorderThickness = 3,
    CornerRadius = 5,
    ShowBorder = true
};

// Add children
var title = new HudText
{
    Text = "Inventory",
    Position = new Vector2(10, 10),
    FontSize = 24
};
panel.AddChild(title);

hud.AddElement(panel);
```

### HudButton

Interactive buttons.

```csharp
var button = new HudButton
{
    Text = "Start Game",
    Position = new Vector2(100, 100),
    Size = new Vector2(150, 50),
    NormalColor = new Vector4(0.3f, 0.3f, 0.3f, 1),
    HoverColor = new Vector4(0.4f, 0.4f, 0.4f, 1),
    PressedColor = new Vector4(0.2f, 0.2f, 0.2f, 1),
    TextColor = new Vector4(1, 1, 1, 1)
};

button.OnClick += () =>
{
    Logger.Info("Button clicked!");
    StartGame();
};

hud.AddElement(button);
```

## HudManager Helpers

### Player Stats HUD

```csharp
var statsPanel = hud.CreatePlayerStatsHUD(
    position: new Vector2(10, 10),
    health: 100, maxHealth: 100,
    mana: 50, maxMana: 100,
    stamina: 80, maxStamina: 100
);

// Update individual bars
var healthBar = statsPanel.GetChildren()
    .FirstOrDefault(c => c.Tag == "health_bar") as HudBar;
if (healthBar != null)
{
    healthBar.Value = 75; // Update health
}
```

### Inventory Display

```csharp
var inventoryPanel = hud.CreateInventoryDisplay(
    position: new Vector2(10, 500),
    slots: 9,
    slotSize: 50
);

// Access individual slots
for (int i = 0; i < 9; i++)
{
    var slot = inventoryPanel.GetChildren()
        .FirstOrDefault(c => c.Tag == $"inventory_slot_{i}");
}
```

### Debug Panel

```csharp
var debugPanel = hud.CreateDebugPanel(
    position: new Vector2(10, 10),
    "FPS: 60",
    "Position: 100, 200",
    "Health: 75/100"
);

// Update debug text
var lines = debugPanel.GetChildren().OfType<HudText>().ToList();
lines[0].Text = "FPS: 75";
```

### Minimap

```csharp
var minimap = hud.CreateMinimap(
    position: new Vector2(1750, 10),
    size: 150
);
```

### Crosshair

```csharp
hud.CreateCrosshair(
    size: 20,
    color: new Vector4(1, 1, 1, 0.8f)
);
```

## Element Hierarchy

HUD elements support parent-child relationships:

```csharp
var container = new HudPanel
{
    Position = new Vector2(100, 100),
    Size = new Vector2(400, 300)
};

var title = new HudText
{
    Text = "Settings",
    Position = new Vector2(10, 10) // Relative to parent
};

var closeButton = new HudButton
{
    Text = "X",
    Position = new Vector2(360, 10),
    Size = new Vector2(30, 30)
};

container.AddChild(title);
container.AddChild(closeButton);

hud.AddElement(container);
```

## Positioning and Layout

### Absolute Positioning

```csharp
element.Position = new Vector2(100, 50); // Pixels from top-left
```

### Anchoring

```csharp
element.Anchor = new Vector2(1, 0); // Top-right corner
element.Position = new Vector2(-10, 10); // 10px from top-right
```

### Layers

```csharp
background.Layer = 0;
middleground.Layer = 5;
foreground.Layer = 10; // Drawn on top
```

### Visibility and Interaction

```csharp
element.Visible = false; // Hide element
element.Enabled = false; // Disable interaction
element.Alpha = 0.5f;    // 50% transparent
```

## Complete Examples

### Game HUD

```csharp
public class GameHudLayer : Layer
{
    private HudManager _hud;
    private HudBar _healthBar;
    private HudText _scoreText;
    private HudPanel _minimap;
    
    public override void OnAttach()
    {
        _hud = HudManager.Instance;
        _hud.ScreenWidth = 1920;
        _hud.ScreenHeight = 1080;
        
        // Health bar
        _healthBar = _hud.AddHealthBar(
            new Vector2(10, 10),
            new Vector2(200, 20),
            100, 100
        );
        
        // Score
        _scoreText = _hud.AddText(
            "Score: 0",
            new Vector2(1800, 10),
            20,
            new Vector4(1, 1, 0, 1)
        );
        
        // Minimap
        _minimap = _hud.CreateMinimap(new Vector2(1750, 900), 150);
        
        // Crosshair
        _hud.CreateCrosshair(20);
    }
    
    public override void OnUpdate(float deltaTime)
    {
        _hud.Update(deltaTime);
        
        // Update health
        var player = GetPlayer();
        if (player != null)
        {
            _healthBar.Value = player.Health;
            _scoreText.Text = $"Score: {player.Score}";
        }
    }
    
    public override void OnRender()
    {
        _hud.Render();
    }
}
```

### Menu System

```csharp
public class MainMenuLayer : Layer
{
    private HudManager _hud;
    
    public override void OnAttach()
    {
        _hud = HudManager.Instance;
        
        // Background panel
        var panel = _hud.AddPanel(
            new Vector2(660, 340),
            new Vector2(600, 400),
            new Vector4(0, 0, 0, 0.8f)
        );
        
        // Title
        var title = new HudText
        {
            Text = "Main Menu",
            Position = new Vector2(250, 30),
            FontSize = 32,
            Alignment = TextAlignment.Center
        };
        panel.AddChild(title);
        
        // Buttons
        var startButton = _hud.AddButton(
            "Start Game",
            new Vector2(725, 450),
            new Vector2(250, 50),
            () => StartGame()
        );
        
        var optionsButton = _hud.AddButton(
            "Options",
            new Vector2(725, 520),
            new Vector2(250, 50),
            () => ShowOptions()
        );
        
        var quitButton = _hud.AddButton(
            "Quit",
            new Vector2(725, 590),
            new Vector2(250, 50),
            () => Application.Instance.Close()
        );
    }
    
    private void StartGame()
    {
        Logger.Info("Starting game...");
    }
    
    private void ShowOptions()
    {
        Logger.Info("Showing options...");
    }
}
```

### Dynamic HUD Updates

```csharp
public class DynamicHudLayer : Layer
{
    private HudManager _hud;
    private List<HudText> _damageNumbers;
    
    public override void OnAttach()
    {
        _hud = HudManager.Instance;
        _damageNumbers = new List<HudText>();
    }
    
    public void ShowDamageNumber(Vector2 position, int damage)
    {
        var damageText = new HudText
        {
            Text = $"-{damage}",
            Position = position,
            FontSize = 24,
            Color = new Vector4(1, 0, 0, 1),
            Alpha = 1.0f
        };
        
        _hud.AddElement(damageText);
        _damageNumbers.Add(damageText);
    }
    
    public override void OnUpdate(float deltaTime)
    {
        // Animate damage numbers (fade and move up)
        foreach (var text in _damageNumbers.ToList())
        {
            text.Position += new Vector2(0, -50 * deltaTime);
            text.Alpha -= deltaTime;
            
            if (text.Alpha <= 0)
            {
                _hud.RemoveElement(text);
                _damageNumbers.Remove(text);
            }
        }
    }
}
```

## Best Practices

### ✅ Do's

- Use HudManager helpers for common UI patterns
- Organize elements with parent-child hierarchy
- Use layers for proper rendering order
- Cache frequently accessed elements
- Update HudManager in OnUpdate()
- Render HudManager in OnRender()

### ❌ Don'ts

- Don't create HUD elements every frame
- Don't forget to remove unused elements
- Don't overlap interactive elements
- Don't use too many layers (0-100 is sufficient)
- Don't modify elements during rendering

## Performance Tips

1. **Reuse Elements**: Update existing elements instead of creating new ones
2. **Culling**: Hide offscreen elements
3. **Batching**: Group similar elements together
4. **Caching**: Cache screen positions for static elements
5. **Lazy Updates**: Only update visible elements

## See Also

- [Application](../core/application.md) - Main application
- [Layers](../core/layers.md) - Layer system
- [Rendering](../graphics/rendering-pipeline.md) - Rendering pipeline
