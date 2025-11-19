using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Input;

namespace EvokerEngine.Core;

/// <summary>
/// Controller/Gamepad input system
/// </summary>
public static class GamepadInput
{
    private static IInputContext? _inputContext;
    private static readonly Dictionary<int, IGamepad> _gamepads = new();
    private static readonly List<IGamepad> _connectedGamepads = new();

    internal static void Initialize(IInputContext inputContext)
    {
        _inputContext = inputContext;
        
        // Register existing gamepads
        for (int i = 0; i < _inputContext.Gamepads.Count; i++)
        {
            var gamepad = _inputContext.Gamepads[i];
            RegisterGamepad(gamepad);
        }

        // Listen for gamepad connection events
        _inputContext.ConnectionChanged += OnConnectionChanged;
    }

    private static void OnConnectionChanged(IInputDevice device, bool connected)
    {
        if (device is IGamepad gamepad)
        {
            if (connected)
            {
                RegisterGamepad(gamepad);
                Logger.Info($"Gamepad connected: {gamepad.Name} (Index: {gamepad.Index})");
            }
            else
            {
                UnregisterGamepad(gamepad);
                Logger.Info($"Gamepad disconnected: {gamepad.Name} (Index: {gamepad.Index})");
            }
        }
    }

    private static void RegisterGamepad(IGamepad gamepad)
    {
        if (!_gamepads.ContainsKey(gamepad.Index))
        {
            _gamepads[gamepad.Index] = gamepad;
            _connectedGamepads.Add(gamepad);
        }
    }

    private static void UnregisterGamepad(IGamepad gamepad)
    {
        _gamepads.Remove(gamepad.Index);
        _connectedGamepads.Remove(gamepad);
    }

    /// <summary>
    /// Get the number of connected gamepads
    /// </summary>
    public static int GetGamepadCount() => _connectedGamepads.Count;

    /// <summary>
    /// Check if a gamepad is connected at the specified index
    /// </summary>
    public static bool IsGamepadConnected(int index = 0)
    {
        return index >= 0 && index < _connectedGamepads.Count && _connectedGamepads[index].IsConnected;
    }

    /// <summary>
    /// Get gamepad by index (0 = first connected gamepad)
    /// </summary>
    private static IGamepad? GetGamepad(int index = 0)
    {
        if (index >= 0 && index < _connectedGamepads.Count)
        {
            return _connectedGamepads[index];
        }
        return null;
    }

    /// <summary>
    /// Check if a button is pressed on the specified gamepad
    /// </summary>
    public static bool IsButtonPressed(ButtonName button, int gamepadIndex = 0)
    {
        var gamepad = GetGamepad(gamepadIndex);
        if (gamepad == null) return false;

        foreach (var btn in gamepad.Buttons)
        {
            if (btn.Name == button && btn.Pressed)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get the value of a thumbstick axis on the specified gamepad
    /// </summary>
    public static float GetAxis(int axisIndex, int gamepadIndex = 0)
    {
        var gamepad = GetGamepad(gamepadIndex);
        if (gamepad == null)
            return 0f;

        // Map axis indices to thumbsticks
        if (axisIndex == 0 && gamepad.Thumbsticks.Count > 0)
            return gamepad.Thumbsticks[0].X; // Left stick X
        if (axisIndex == 1 && gamepad.Thumbsticks.Count > 0)
            return gamepad.Thumbsticks[0].Y; // Left stick Y
        if (axisIndex == 2 && gamepad.Thumbsticks.Count > 1)
            return gamepad.Thumbsticks[1].X; // Right stick X
        if (axisIndex == 3 && gamepad.Thumbsticks.Count > 1)
            return gamepad.Thumbsticks[1].Y; // Right stick Y

        return 0f;
    }

    /// <summary>
    /// Get left stick position (X, Y) on the specified gamepad
    /// </summary>
    public static Vector2 GetLeftStick(int gamepadIndex = 0)
    {
        var gamepad = GetGamepad(gamepadIndex);
        if (gamepad == null || gamepad.Thumbsticks.Count < 1)
            return Vector2.Zero;

        var stick = gamepad.Thumbsticks[0];
        return new Vector2(stick.X, stick.Y);
    }

    /// <summary>
    /// Get right stick position (X, Y) on the specified gamepad
    /// </summary>
    public static Vector2 GetRightStick(int gamepadIndex = 0)
    {
        var gamepad = GetGamepad(gamepadIndex);
        if (gamepad == null || gamepad.Thumbsticks.Count < 2)
            return Vector2.Zero;

        var stick = gamepad.Thumbsticks[1];
        return new Vector2(stick.X, stick.Y);
    }

    /// <summary>
    /// Get left trigger value (0.0 to 1.0) on the specified gamepad
    /// </summary>
    public static float GetLeftTrigger(int gamepadIndex = 0)
    {
        var gamepad = GetGamepad(gamepadIndex);
        if (gamepad == null || gamepad.Triggers.Count < 1)
            return 0f;

        return gamepad.Triggers[0].Position;
    }

    /// <summary>
    /// Get right trigger value (0.0 to 1.0) on the specified gamepad
    /// </summary>
    public static float GetRightTrigger(int gamepadIndex = 0)
    {
        var gamepad = GetGamepad(gamepadIndex);
        if (gamepad == null || gamepad.Triggers.Count < 2)
            return 0f;

        return gamepad.Triggers[1].Position;
    }

    /// <summary>
    /// Get D-pad state
    /// </summary>
    public static (bool up, bool down, bool left, bool right) GetDPad(int gamepadIndex = 0)
    {
        return (
            IsButtonPressed(ButtonName.DPadUp, gamepadIndex),
            IsButtonPressed(ButtonName.DPadDown, gamepadIndex),
            IsButtonPressed(ButtonName.DPadLeft, gamepadIndex),
            IsButtonPressed(ButtonName.DPadRight, gamepadIndex)
        );
    }

    /// <summary>
    /// Get the name of the gamepad at the specified index
    /// </summary>
    public static string GetGamepadName(int gamepadIndex = 0)
    {
        var gamepad = GetGamepad(gamepadIndex);
        return gamepad?.Name ?? "No Gamepad";
    }

    /// <summary>
    /// Set vibration/rumble on the gamepad (if supported)
    /// </summary>
    public static void SetVibration(float leftMotor, float rightMotor, int gamepadIndex = 0)
    {
        var gamepad = GetGamepad(gamepadIndex);
        if (gamepad == null) return;

        // Check if gamepad supports vibration
        if (gamepad.VibrationMotors.Count >= 2)
        {
            gamepad.VibrationMotors[0].Speed = Math.Clamp(leftMotor, 0f, 1f);
            gamepad.VibrationMotors[1].Speed = Math.Clamp(rightMotor, 0f, 1f);
        }
    }

    /// <summary>
    /// Stop all vibration on the gamepad
    /// </summary>
    public static void StopVibration(int gamepadIndex = 0)
    {
        SetVibration(0f, 0f, gamepadIndex);
    }
}
