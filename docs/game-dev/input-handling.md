# Input Handling

The Input system in Evoker Engine provides comprehensive input handling for keyboard, mouse, and gamepad/controller input with both event-based and polling-based approaches.

## Overview

The input system supports:

- **Keyboard**: Key press/release detection and polling
- **Mouse**: Button clicks, movement, and scroll wheel
- **Gamepad**: Button input, analog sticks, triggers, and vibration
- **Hot-plug**: Automatic gamepad connect/disconnect detection

## Input Polling (Input Class)

Poll input state at any time during update:

```csharp
public static class Input
{
    // Keyboard
    public static bool IsKeyPressed(Key key);
    
    // Mouse
    public static bool IsMouseButtonPressed(MouseButton button);
    public static Vector2 GetMousePosition();
    public static float GetMouseX();
    public static float GetMouseY();
    public static Vector2 GetMouseDelta();
    public static Vector2 GetMouseScroll();
}
```

## Keyboard Input

### Key Polling

```csharp
using EvokerEngine.Core;
using Silk.NET.Input;

public override void OnUpdate(float deltaTime)
{
    // Check if key is pressed
    if (Input.IsKeyPressed(Key.W))
    {
        MoveForward(deltaTime);
    }
    
    if (Input.IsKeyPressed(Key.Space))
    {
        Jump();
    }
    
    // Multiple keys
    if (Input.IsKeyPressed(Key.ShiftLeft) && Input.IsKeyPressed(Key.W))
    {
        Sprint(deltaTime);
    }
}
```

### Key Events

```csharp
public override void OnEvent(Event e)
{
    if (e is KeyPressedEvent keyEvent)
    {
        // Triggered once when key is pressed
        if (keyEvent.KeyCode == Key.E)
        {
            OpenInventory();
        }
        
        // Check for key repeat (holding key down)
        if (keyEvent.KeyCode == Key.R && !keyEvent.IsRepeat)
        {
            Reload();  // Only trigger once, not while held
        }
    }
    else if (e is KeyReleasedEvent releaseEvent)
    {
        // Triggered when key is released
        if (releaseEvent.KeyCode == Key.W)
        {
            StopMoving();
        }
    }
}
```

### Common Keys

```csharp
// Arrow keys
Key.Up, Key.Down, Key.Left, Key.Right

// WASD
Key.W, Key.A, Key.S, Key.D

// Function keys
Key.F1, Key.F2, ..., Key.F12

// Modifiers
Key.ShiftLeft, Key.ShiftRight
Key.ControlLeft, Key.ControlRight
Key.AltLeft, Key.AltRight

// Special keys
Key.Space, Key.Enter, Key.Escape, Key.Tab
Key.Backspace, Key.Delete

// Number keys
Key.Number0, Key.Number1, ..., Key.Number9
```

## Mouse Input

### Mouse Buttons

```csharp
public override void OnUpdate(float deltaTime)
{
    // Check mouse button state
    if (Input.IsMouseButtonPressed(MouseButton.Left))
    {
        FireWeapon();
    }
    
    if (Input.IsMouseButtonPressed(MouseButton.Right))
    {
        Aim();
    }
}

public override void OnEvent(Event e)
{
    if (e is MouseButtonPressedEvent mouseEvent)
    {
        if (mouseEvent.Button == MouseButton.Left)
        {
            OnLeftClick();
        }
        else if (mouseEvent.Button == MouseButton.Right)
        {
            OnRightClick();
        }
        else if (mouseEvent.Button == MouseButton.Middle)
        {
            OnMiddleClick();
        }
    }
}
```

### Mouse Position

```csharp
public override void OnUpdate(float deltaTime)
{
    // Get current mouse position
    var mousePos = Input.GetMousePosition();
    UpdateCrosshair(mousePos);
    
    // Separate X and Y
    float x = Input.GetMouseX();
    float y = Input.GetMouseY();
}
```

### Mouse Movement

```csharp
public override void OnUpdate(float deltaTime)
{
    // Get mouse delta (movement since last frame)
    var delta = Input.GetMouseDelta();
    
    // First-person camera look
    _cameraYaw += delta.X * _sensitivity;
    _cameraPitch -= delta.Y * _sensitivity;
    _cameraPitch = Math.Clamp(_cameraPitch, -89f, 89f);
}

public override void OnEvent(Event e)
{
    if (e is MouseMovedEvent moveEvent)
    {
        // Absolute position
        UpdateTooltip(new Vector2(moveEvent.X, moveEvent.Y));
    }
}
```

### Mouse Scroll

```csharp
public override void OnUpdate(float deltaTime)
{
    var scroll = Input.GetMouseScroll();
    if (scroll.Y != 0)
    {
        // Zoom camera
        _cameraZoom += scroll.Y * 0.1f;
    }
}

public override void OnEvent(Event e)
{
    if (e is MouseScrolledEvent scrollEvent)
    {
        // Vertical scroll
        if (scrollEvent.OffsetY > 0)
            ZoomIn();
        else if (scrollEvent.OffsetY < 0)
            ZoomOut();
            
        // Horizontal scroll (if supported)
        if (scrollEvent.OffsetX != 0)
            PanHorizontally(scrollEvent.OffsetX);
    }
}
```

## Gamepad Input

### Button Input

```csharp
using EvokerEngine.Core;
using Silk.NET.Input;

public override void OnUpdate(float deltaTime)
{
    // Check if gamepad is connected
    if (!GamepadInput.IsGamepadConnected(0))
        return;
    
    // Check button state
    if (GamepadInput.IsButtonPressed(ButtonName.A))
    {
        Jump();
    }
    
    if (GamepadInput.IsButtonPressed(ButtonName.X))
    {
        Attack();
    }
}

public override void OnEvent(Event e)
{
    if (e is GamepadButtonPressedEvent gamepadEvent)
    {
        switch (gamepadEvent.Button)
        {
            case ButtonName.A:      // Xbox A / PlayStation Cross
                Jump();
                break;
            case ButtonName.B:      // Xbox B / PlayStation Circle
                Dodge();
                break;
            case ButtonName.Start:
                OpenPauseMenu();
                break;
        }
    }
}
```

### Analog Sticks

```csharp
public override void OnUpdate(float deltaTime)
{
    if (!GamepadInput.IsGamepadConnected(0))
        return;
    
    // Left stick - Movement
    var leftStick = GamepadInput.GetLeftStick();
    if (leftStick.Length() > 0.1f)  // Deadzone
    {
        var moveDir = new Vector3(leftStick.X, 0, -leftStick.Y);
        MovePlayer(moveDir * deltaTime);
    }
    
    // Right stick - Camera
    var rightStick = GamepadInput.GetRightStick();
    if (rightStick.Length() > 0.1f)  // Deadzone
    {
        _cameraYaw += rightStick.X * _sensitivity * deltaTime;
        _cameraPitch -= rightStick.Y * _sensitivity * deltaTime;
    }
}
```

### Triggers

```csharp
public override void OnUpdate(float deltaTime)
{
    if (!GamepadInput.IsGamepadConnected(0))
        return;
    
    // Left trigger - Aim
    float leftTrigger = GamepadInput.GetLeftTrigger();
    if (leftTrigger > 0.1f)
    {
        SetAimAmount(leftTrigger);  // 0.0 to 1.0
    }
    
    // Right trigger - Shoot
    float rightTrigger = GamepadInput.GetRightTrigger();
    if (rightTrigger > 0.5f)  // Half-press threshold
    {
        FireWeapon(rightTrigger);
    }
}
```

### D-Pad

```csharp
public override void OnUpdate(float deltaTime)
{
    if (!GamepadInput.IsGamepadConnected(0))
        return;
    
    // D-pad navigation
    if (GamepadInput.IsDPadUpPressed())
        NavigateMenuUp();
    
    if (GamepadInput.IsDPadDownPressed())
        NavigateMenuDown();
    
    if (GamepadInput.IsDPadLeftPressed())
        NavigateMenuLeft();
    
    if (GamepadInput.IsDPadRightPressed())
        NavigateMenuRight();
}
```

### Vibration/Rumble

```csharp
// Set vibration on both motors (0.0 to 1.0)
GamepadInput.SetVibration(0.5f, 0.5f);

// Strong rumble
GamepadInput.SetVibration(1.0f, 1.0f);

// Light rumble (high-frequency motor only)
GamepadInput.SetVibration(0.0f, 0.3f);

// Stop vibration
GamepadInput.StopVibration();

// Timed vibration
GamepadInput.SetVibration(0.8f, 0.8f);
await Task.Delay(500);  // 0.5 seconds
GamepadInput.StopVibration();
```

### Gamepad Connection Events

```csharp
public override void OnEvent(Event e)
{
    if (e is GamepadConnectedEvent connectedEvent)
    {
        Logger.Info($"Gamepad connected: {connectedEvent.GamepadName} at index {connectedEvent.GamepadIndex}");
        ShowControllerUI();
    }
    else if (e is GamepadDisconnectedEvent disconnectedEvent)
    {
        Logger.Info($"Gamepad disconnected: {disconnectedEvent.GamepadName}");
        PauseGame();
        ShowReconnectMessage();
    }
}
```

## Complete Examples

### Example 1: FPS Controller

```csharp
public class FPSController : Layer
{
    private Entity _player;
    private float _moveSpeed = 5f;
    private float _lookSensitivity = 0.1f;
    private float _yaw = 0f;
    private float _pitch = 0f;

    public override void OnUpdate(float deltaTime)
    {
        var scene = Application.Instance.ActiveScene;
        var transform = scene.Registry.GetComponent<TransformComponent>(_player);
        
        // Calculate forward and right vectors
        var forward = new Vector3(
            MathF.Sin(_yaw * MathHelper.Deg2Rad),
            0,
            MathF.Cos(_yaw * MathHelper.Deg2Rad)
        );
        var right = Vector3.Cross(forward, Vector3.UnitY);
        
        // Keyboard movement
        var movement = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W)) movement += forward;
        if (Input.IsKeyPressed(Key.S)) movement -= forward;
        if (Input.IsKeyPressed(Key.A)) movement -= right;
        if (Input.IsKeyPressed(Key.D)) movement += right;
        
        if (movement.Length() > 0)
        {
            movement = Vector3.Normalize(movement);
            transform.Position += movement * _moveSpeed * deltaTime;
        }
        
        // Mouse look
        var mouseDelta = Input.GetMouseDelta();
        _yaw += mouseDelta.X * _lookSensitivity;
        _pitch -= mouseDelta.Y * _lookSensitivity;
        _pitch = Math.Clamp(_pitch, -89f, 89f);
        
        transform.Rotation = new Vector3(_pitch, _yaw, 0);
        
        // Gamepad support
        if (GamepadInput.IsGamepadConnected(0))
        {
            // Movement with left stick
            var leftStick = GamepadInput.GetLeftStick();
            if (leftStick.Length() > 0.1f)
            {
                var gamepadMove = forward * -leftStick.Y + right * leftStick.X;
                transform.Position += gamepadMove * _moveSpeed * deltaTime;
            }
            
            // Look with right stick
            var rightStick = GamepadInput.GetRightStick();
            if (rightStick.Length() > 0.1f)
            {
                _yaw += rightStick.X * _lookSensitivity * 60f * deltaTime;
                _pitch -= rightStick.Y * _lookSensitivity * 60f * deltaTime;
                _pitch = Math.Clamp(_pitch, -89f, 89f);
            }
        }
    }

    public override void OnEvent(Event e)
    {
        // Jump
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.Space)
                Jump();
        }
        else if (e is GamepadButtonPressedEvent gamepadEvent)
        {
            if (gamepadEvent.Button == ButtonName.A)
                Jump();
        }
        
        // Shoot
        if (e is MouseButtonPressedEvent mouseEvent)
        {
            if (mouseEvent.Button == MouseButton.Left)
                Shoot();
        }
        
        if (GamepadInput.GetRightTrigger() > 0.5f)
        {
            Shoot();
        }
    }

    private void Jump() { /* ... */ }
    private void Shoot() { /* ... */ }
}
```

### Example 2: 2D Platformer

```csharp
public class PlatformerController : Layer
{
    private Entity _player;
    private Vector3 _velocity = Vector3.Zero;
    private bool _isGrounded = false;
    private float _jumpForce = 10f;
    private float _moveSpeed = 5f;

    public override void OnUpdate(float deltaTime)
    {
        var scene = Application.Instance.ActiveScene;
        var transform = scene.Registry.GetComponent<TransformComponent>(_player);
        
        // Horizontal movement (keyboard)
        float moveInput = 0f;
        if (Input.IsKeyPressed(Key.A)) moveInput -= 1f;
        if (Input.IsKeyPressed(Key.D)) moveInput += 1f;
        
        // Gamepad support
        if (GamepadInput.IsGamepadConnected(0))
        {
            var leftStick = GamepadInput.GetLeftStick();
            moveInput = leftStick.X;
        }
        
        _velocity.X = moveInput * _moveSpeed;
        
        // Apply gravity
        if (!_isGrounded)
        {
            _velocity.Y -= 20f * deltaTime;
        }
        
        // Apply movement
        transform.Position += _velocity * deltaTime;
        
        // Simple ground check
        if (transform.Position.Y <= 0)
        {
            transform.Position = new Vector3(transform.Position.X, 0, 0);
            _velocity.Y = 0;
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    public override void OnEvent(Event e)
    {
        // Jump (keyboard)
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.Space && _isGrounded)
            {
                _velocity.Y = _jumpForce;
            }
        }
        
        // Jump (gamepad)
        if (e is GamepadButtonPressedEvent gamepadEvent)
        {
            if (gamepadEvent.Button == ButtonName.A && _isGrounded)
            {
                _velocity.Y = _jumpForce;
                GamepadInput.SetVibration(0.3f, 0.3f);  // Rumble on jump
                Task.Delay(100).ContinueWith(_ => GamepadInput.StopVibration());
            }
        }
    }
}
```

## Best Practices

### ✅ Do's

- Use polling for continuous input (movement)
- Use events for discrete actions (jumping, menu navigation)
- Implement deadzone for analog sticks (0.1-0.2)
- Support both keyboard/mouse and gamepad
- Stop gamepad vibration after use

### ❌ Don'ts

- Don't query input in OnEvent (use OnUpdate instead)
- Don't forget to check gamepad connection
- Don't hardcode controls (make them rebindable)
- Don't forget to clamp camera pitch

## Input Remapping Example

```csharp
public class InputManager
{
    private Dictionary<string, Key> _keyBindings = new()
    {
        { "forward", Key.W },
        { "back", Key.S },
        { "left", Key.A },
        { "right", Key.D },
        { "jump", Key.Space }
    };

    public bool IsActionPressed(string action)
    {
        if (_keyBindings.TryGetValue(action, out var key))
        {
            return Input.IsKeyPressed(key);
        }
        return false;
    }

    public void RebindKey(string action, Key newKey)
    {
        _keyBindings[action] = newKey;
    }
}

// Usage
if (_inputManager.IsActionPressed("jump"))
{
    Jump();
}
```

## See Also

- [Events](../core/events.md) - Event system for input events
- [Layers](../core/layers.md) - Layer system for organizing input handling
