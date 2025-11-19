using System;

namespace EvokerEngine.Core;

/// <summary>
/// Base class for all events
/// </summary>
public abstract class Event
{
    public bool Handled { get; set; }

    public abstract EventType GetEventType();
    public abstract string GetName();

    public override string ToString() => GetName();
}

/// <summary>
/// Event types
/// </summary>
public enum EventType
{
    None = 0,
    WindowClose, WindowResize, WindowFocus, WindowLostFocus, WindowMoved,
    AppTick, AppUpdate, AppRender,
    KeyPressed, KeyReleased, KeyTyped,
    MouseButtonPressed, MouseButtonReleased, MouseMoved, MouseScrolled,
    GamepadConnected, GamepadDisconnected, GamepadButtonPressed, GamepadButtonReleased
}

/// <summary>
/// Window close event
/// </summary>
public class WindowCloseEvent : Event
{
    public override EventType GetEventType() => EventType.WindowClose;
    public override string GetName() => "WindowClose";
}

/// <summary>
/// Window resize event
/// </summary>
public class WindowResizeEvent : Event
{
    public int Width { get; }
    public int Height { get; }

    public WindowResizeEvent(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public override EventType GetEventType() => EventType.WindowResize;
    public override string GetName() => $"WindowResize: {Width}, {Height}";
}

/// <summary>
/// Key pressed event
/// </summary>
public class KeyPressedEvent : Event
{
    public Silk.NET.Input.Key KeyCode { get; }
    public bool IsRepeat { get; }

    public KeyPressedEvent(Silk.NET.Input.Key keyCode, bool isRepeat = false)
    {
        KeyCode = keyCode;
        IsRepeat = isRepeat;
    }

    public override EventType GetEventType() => EventType.KeyPressed;
    public override string GetName() => $"KeyPressed: {KeyCode} (repeat = {IsRepeat})";
}

/// <summary>
/// Key released event
/// </summary>
public class KeyReleasedEvent : Event
{
    public Silk.NET.Input.Key KeyCode { get; }

    public KeyReleasedEvent(Silk.NET.Input.Key keyCode)
    {
        KeyCode = keyCode;
    }

    public override EventType GetEventType() => EventType.KeyReleased;
    public override string GetName() => $"KeyReleased: {KeyCode}";
}

/// <summary>
/// Mouse button pressed event
/// </summary>
public class MouseButtonPressedEvent : Event
{
    public Silk.NET.Input.MouseButton Button { get; }

    public MouseButtonPressedEvent(Silk.NET.Input.MouseButton button)
    {
        Button = button;
    }

    public override EventType GetEventType() => EventType.MouseButtonPressed;
    public override string GetName() => $"MouseButtonPressed: {Button}";
}

/// <summary>
/// Mouse button released event
/// </summary>
public class MouseButtonReleasedEvent : Event
{
    public Silk.NET.Input.MouseButton Button { get; }

    public MouseButtonReleasedEvent(Silk.NET.Input.MouseButton button)
    {
        Button = button;
    }

    public override EventType GetEventType() => EventType.MouseButtonReleased;
    public override string GetName() => $"MouseButtonReleased: {Button}";
}

/// <summary>
/// Mouse moved event
/// </summary>
public class MouseMovedEvent : Event
{
    public float X { get; }
    public float Y { get; }

    public MouseMovedEvent(float x, float y)
    {
        X = x;
        Y = y;
    }

    public override EventType GetEventType() => EventType.MouseMoved;
    public override string GetName() => $"MouseMoved: {X}, {Y}";
}

/// <summary>
/// Mouse scrolled event
/// </summary>
public class MouseScrolledEvent : Event
{
    public float OffsetX { get; }
    public float OffsetY { get; }

    public MouseScrolledEvent(float offsetX, float offsetY)
    {
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public override EventType GetEventType() => EventType.MouseScrolled;
    public override string GetName() => $"MouseScrolled: {OffsetX}, {OffsetY}";
}

/// <summary>
/// Gamepad connected event
/// </summary>
public class GamepadConnectedEvent : Event
{
    public int GamepadIndex { get; }
    public string GamepadName { get; }

    public GamepadConnectedEvent(int gamepadIndex, string gamepadName)
    {
        GamepadIndex = gamepadIndex;
        GamepadName = gamepadName;
    }

    public override EventType GetEventType() => EventType.GamepadConnected;
    public override string GetName() => $"GamepadConnected: {GamepadName} (Index: {GamepadIndex})";
}

/// <summary>
/// Gamepad disconnected event
/// </summary>
public class GamepadDisconnectedEvent : Event
{
    public int GamepadIndex { get; }
    public string GamepadName { get; }

    public GamepadDisconnectedEvent(int gamepadIndex, string gamepadName)
    {
        GamepadIndex = gamepadIndex;
        GamepadName = gamepadName;
    }

    public override EventType GetEventType() => EventType.GamepadDisconnected;
    public override string GetName() => $"GamepadDisconnected: {GamepadName} (Index: {GamepadIndex})";
}

/// <summary>
/// Gamepad button pressed event
/// </summary>
public class GamepadButtonPressedEvent : Event
{
    public Silk.NET.Input.ButtonName Button { get; }
    public int GamepadIndex { get; }

    public GamepadButtonPressedEvent(Silk.NET.Input.ButtonName button, int gamepadIndex)
    {
        Button = button;
        GamepadIndex = gamepadIndex;
    }

    public override EventType GetEventType() => EventType.GamepadButtonPressed;
    public override string GetName() => $"GamepadButtonPressed: {Button} (Gamepad {GamepadIndex})";
}

/// <summary>
/// Gamepad button released event
/// </summary>
public class GamepadButtonReleasedEvent : Event
{
    public Silk.NET.Input.ButtonName Button { get; }
    public int GamepadIndex { get; }

    public GamepadButtonReleasedEvent(Silk.NET.Input.ButtonName button, int gamepadIndex)
    {
        Button = button;
        GamepadIndex = gamepadIndex;
    }

    public override EventType GetEventType() => EventType.GamepadButtonReleased;
    public override string GetName() => $"GamepadButtonReleased: {Button} (Gamepad {GamepadIndex})";
}
