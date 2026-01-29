---
tags:
  - coresystem
  - input
  - architecture
---

# Event System

The Event system in Evoker Engine provides a flexible way to handle input, window events, and custom game events. Events are dispatched to layers, allowing for decoupled communication between systems.

## Overview

The event system:

- Handles keyboard, mouse, and gamepad input
- Manages window events (resize, close, focus)
- Supports event propagation and handling
- Allows layers to stop event propagation
- Type-safe event handling with inheritance

## Event Base Class

```csharp
public abstract class Event
{
    public bool Handled { get; set; }
    
    public abstract EventType GetEventType();
    public abstract string GetName();
}
```

### Event Properties

- **Handled**: Set to `true` to stop event propagation to lower layers
- **GetEventType()**: Returns the event type enum
- **GetName()**: Returns a string description of the event

## Event Types

```csharp
public enum EventType
{
    None = 0,
    // Window events
    WindowClose, WindowResize, WindowFocus, WindowLostFocus, WindowMoved,
    // Application events
    AppTick, AppUpdate, AppRender,
    // Keyboard events
    KeyPressed, KeyReleased, KeyTyped,
    // Mouse events
    MouseButtonPressed, MouseButtonReleased, MouseMoved, MouseScrolled,
    // Gamepad events
    GamepadConnected, GamepadDisconnected, 
    GamepadButtonPressed, GamepadButtonReleased
}
```

## Window Events

### WindowCloseEvent

Fired when the window close button is clicked.

```csharp
public class WindowCloseEvent : Event
{
    // No additional properties
}
```

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is WindowCloseEvent)
    {
        SaveGameState();
        Logger.Info("Window closing");
    }
}
```

### WindowResizeEvent

Fired when the window is resized.

```csharp
public class WindowResizeEvent : Event
{
    public int Width { get; }
    public int Height { get; }
}
```

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is WindowResizeEvent resizeEvent)
    {
        Logger.Info($"Window resized to {resizeEvent.Width}x{resizeEvent.Height}");
        UpdateCamera(resizeEvent.Width, resizeEvent.Height);
    }
}
```

## Keyboard Events

### KeyPressedEvent

Fired when a key is pressed or held down.

```csharp
public class KeyPressedEvent : Event
{
    public Key KeyCode { get; }
    public bool IsRepeat { get; }
}
```

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent keyEvent)
    {
        if (keyEvent.KeyCode == Key.Space && !keyEvent.IsRepeat)
        {
            Player.Jump();
            e.Handled = true;
        }
        
        if (keyEvent.KeyCode == Key.Escape)
        {
            OpenPauseMenu();
        }
    }
}
```

### KeyReleasedEvent

Fired when a key is released.

```csharp
public class KeyReleasedEvent : Event
{
    public Key KeyCode { get; }
}
```

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is KeyReleasedEvent keyEvent)
    {
        if (keyEvent.KeyCode == Key.W)
        {
            Player.StopMovingForward();
        }
    }
}
```

## Mouse Events

### MouseButtonPressedEvent

Fired when a mouse button is pressed.

```csharp
public class MouseButtonPressedEvent : Event
{
    public MouseButton Button { get; }
}
```

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is MouseButtonPressedEvent mouseEvent)
    {
        if (mouseEvent.Button == MouseButton.Left)
        {
            var mousePos = Input.GetMousePosition();
            ShootProjectile(mousePos);
        }
        
        if (mouseEvent.Button == MouseButton.Right)
        {
            OpenContextMenu();
        }
    }
}
```

### MouseButtonReleasedEvent

Fired when a mouse button is released.

```csharp
public class MouseButtonReleasedEvent : Event
{
    public MouseButton Button { get; }
}
```

### MouseMovedEvent

Fired when the mouse cursor moves.

```csharp
public class MouseMovedEvent : Event
{
    public float X { get; }
    public float Y { get; }
}
```

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is MouseMovedEvent moveEvent)
    {
        // Update crosshair position
        _crosshairPos = new Vector2(moveEvent.X, moveEvent.Y);
        
        // First-person camera look
        _camera.Rotate(moveEvent.X * _sensitivity, moveEvent.Y * _sensitivity);
    }
}
```

### MouseScrolledEvent

Fired when the mouse wheel is scrolled.

```csharp
public class MouseScrolledEvent : Event
{
    public float OffsetX { get; }
    public float OffsetY { get; }
}
```

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is MouseScrolledEvent scrollEvent)
    {
        // Zoom camera
        _cameraZoom += scrollEvent.OffsetY * 0.1f;
        _cameraZoom = Math.Clamp(_cameraZoom, 0.5f, 5.0f);
    }
}
```

## Gamepad Events

### GamepadConnectedEvent

Fired when a gamepad is connected.

```csharp
public class GamepadConnectedEvent : Event
{
    public int GamepadIndex { get; }
    public string GamepadName { get; }
}
```

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is GamepadConnectedEvent gamepadEvent)
    {
        Logger.Info($"Gamepad connected: {gamepadEvent.GamepadName} at index {gamepadEvent.GamepadIndex}");
        ShowControllerUI();
    }
}
```

### GamepadDisconnectedEvent

Fired when a gamepad is disconnected.

```csharp
public class GamepadDisconnectedEvent : Event
{
    public int GamepadIndex { get; }
    public string GamepadName { get; }
}
```

### GamepadButtonPressedEvent

Fired when a gamepad button is pressed.

```csharp
public class GamepadButtonPressedEvent : Event
{
    public ButtonName Button { get; }
    public int GamepadIndex { get; }
}
```

**Example:**
```csharp
public override void OnEvent(Event e)
{
    if (e is GamepadButtonPressedEvent gamepadEvent)
    {
        switch (gamepadEvent.Button)
        {
            case ButtonName.A:
                Player.Jump();
                break;
            case ButtonName.B:
                Player.Dodge();
                break;
            case ButtonName.Start:
                OpenPauseMenu();
                break;
        }
    }
}
```

### GamepadButtonReleasedEvent

Fired when a gamepad button is released.

```csharp
public class GamepadButtonReleasedEvent : Event
{
    public ButtonName Button { get; }
    public int GamepadIndex { get; }
}
```

## Event Handling Patterns

### Pattern 1: Type Checking with is

```csharp
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent keyEvent)
    {
        HandleKeyPress(keyEvent);
    }
    else if (e is MouseButtonPressedEvent mouseEvent)
    {
        HandleMouseClick(mouseEvent);
    }
}
```

### Pattern 2: Switch on Event Type

```csharp
public override void OnEvent(Event e)
{
    switch (e.GetEventType())
    {
        case EventType.KeyPressed:
            HandleKeyPress((KeyPressedEvent)e);
            break;
        case EventType.MouseButtonPressed:
            HandleMouseClick((MouseButtonPressedEvent)e);
            break;
    }
}
```

### Pattern 3: Dedicated Handler Methods

```csharp
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent keyEvent)
        OnKeyPressed(keyEvent);
    else if (e is MouseMovedEvent moveEvent)
        OnMouseMoved(moveEvent);
}

private void OnKeyPressed(KeyPressedEvent e)
{
    // Handle key press
}

private void OnMouseMoved(MouseMovedEvent e)
{
    // Handle mouse movement
}
```

## Event Propagation

Events are dispatched to layers in **reverse order** (top to bottom):

```
┌──────────────┐
│  UI Layer    │  ← Event received first
├──────────────┤
│  Game Layer  │  ← Event received second (if not handled)
└──────────────┘
```

### Stopping Propagation

Set `e.Handled = true` to prevent the event from reaching lower layers:

```csharp
class UILayer : Layer
{
    public override void OnEvent(Event e)
    {
        if (e is MouseButtonPressedEvent mouseEvent)
        {
            if (IsMouseOverButton(mouseEvent))
            {
                OnButtonClicked();
                e.Handled = true; // Don't pass to game layer
            }
        }
    }
}

class GameLayer : Layer
{
    public override void OnEvent(Event e)
    {
        if (e is MouseButtonPressedEvent mouseEvent)
        {
            // Only executed if UI didn't handle it
            if (!e.Handled)
            {
                SelectUnit(mouseEvent);
            }
        }
    }
}
```

## Complete Examples

### Example 1: Player Controller

```csharp
class PlayerControllerLayer : Layer
{
    private Entity _player;
    private float _moveSpeed = 5f;

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            HandleKeyPress(keyEvent);
        }
        else if (e is GamepadButtonPressedEvent gamepadEvent)
        {
            HandleGamepadButton(gamepadEvent);
        }
    }

    private void HandleKeyPress(KeyPressedEvent e)
    {
        switch (e.KeyCode)
        {
            case Key.Space:
                if (!e.IsRepeat)
                    Jump();
                break;
            case Key.ShiftLeft:
                _moveSpeed = 10f; // Sprint
                break;
        }
    }

    private void HandleGamepadButton(GamepadButtonPressedEvent e)
    {
        if (e.Button == ButtonName.A)
        {
            Jump();
        }
    }

    private void Jump()
    {
        var scene = Application.Instance.ActiveScene;
        var rb = scene.Registry.GetComponent<RigidbodyComponent>(_player);
        if (rb != null)
        {
            rb.AddImpulse(new Vector3(0, 10, 0));
        }
    }
}
```

### Example 2: Pause Menu

```csharp
class PauseMenuLayer : Layer
{
    private bool _isPaused = false;

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.Escape)
            {
                TogglePause();
                e.Handled = true;
            }
        }
        
        if (_isPaused)
        {
            // Block all input when paused
            e.Handled = true;
        }
    }

    private void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.TimeScale = _isPaused ? 0f : 1f;
        Logger.Info(_isPaused ? "Game paused" : "Game resumed");
    }

    public override void OnRender()
    {
        if (_isPaused)
        {
            RenderPauseMenu();
        }
    }
}
```

### Example 3: Camera Controller

```csharp
class CameraControllerLayer : Layer
{
    private Entity _camera;
    private float _rotationX = 0f;
    private float _rotationY = 0f;
    private float _sensitivity = 0.1f;

    public override void OnEvent(Event e)
    {
        if (e is MouseMovedEvent moveEvent)
        {
            HandleMouseLook(moveEvent);
        }
        else if (e is MouseScrolledEvent scrollEvent)
        {
            HandleZoom(scrollEvent);
        }
    }

    private void HandleMouseLook(MouseMovedEvent e)
    {
        var mouseDelta = Input.GetMouseDelta();
        
        _rotationY += mouseDelta.X * _sensitivity;
        _rotationX -= mouseDelta.Y * _sensitivity;
        _rotationX = Math.Clamp(_rotationX, -89f, 89f);
        
        var scene = Application.Instance.ActiveScene;
        var transform = scene.Registry.GetComponent<TransformComponent>(_camera);
        transform.Rotation = new Vector3(_rotationX, _rotationY, 0);
    }

    private void HandleZoom(MouseScrolledEvent e)
    {
        var scene = Application.Instance.ActiveScene;
        var camera = scene.Registry.GetComponent<Camera3DComponent>(_camera);
        camera.FieldOfView -= e.OffsetY * 2f;
        camera.FieldOfView = Math.Clamp(camera.FieldOfView, 20f, 120f);
    }
}
```

## Best Practices

### ✅ Do's

- Use `e.Handled = true` to stop propagation
- Check `!e.Handled` before processing in lower layers
- Use type-safe pattern matching with `is`
- Create focused event handlers
- Handle gamepad disconnect gracefully

### ❌ Don'ts

- Don't perform heavy computation in event handlers
- Don't modify the event object (except `Handled`)
- Don't forget to check for null when casting
- Don't block the event dispatch thread

## Performance Tips

1. **Early exit** - Check event type before casting
2. **Minimize allocations** - Reuse event objects where possible
3. **Batch processing** - Handle similar events together
4. **Profile handlers** - Identify slow event processing

## See Also

- [Input Handling](../game-dev/input-handling.md) - Input system documentation
- [Layers](layers.md) - Layer system
- [Application](application.md) - Application lifecycle
