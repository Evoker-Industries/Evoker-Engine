using System;
using System.Numerics;
using Silk.NET.Input;

namespace EvokerEngine.Core;

/// <summary>
/// Input polling system
/// </summary>
public static class Input
{
    private static IInputContext? _inputContext;
    private static IKeyboard? _keyboard;
    private static IMouse? _mouse;

    internal static void Initialize(IInputContext inputContext)
    {
        _inputContext = inputContext;
        _keyboard = _inputContext.Keyboards.Count > 0 ? _inputContext.Keyboards[0] : null;
        _mouse = _inputContext.Mice.Count > 0 ? _inputContext.Mice[0] : null;
    }

    public static bool IsKeyPressed(Key key)
    {
        return _keyboard?.IsKeyPressed(key) ?? false;
    }

    public static bool IsMouseButtonPressed(MouseButton button)
    {
        return _mouse?.IsButtonPressed(button) ?? false;
    }

    public static Vector2 GetMousePosition()
    {
        return _mouse?.Position ?? Vector2.Zero;
    }

    public static float GetMouseX()
    {
        return _mouse?.Position.X ?? 0f;
    }

    public static float GetMouseY()
    {
        return _mouse?.Position.Y ?? 0f;
    }
}
