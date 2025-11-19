using EvokerEngine.Core;
using Silk.NET.Input;

namespace EvokerEngine.Demo;

class Program
{
    static void Main(string[] args)
    {
        // Create and configure the application
        var app = new Application("Evoker Engine Demo", 1280, 720);

        // Add a demo layer
        app.PushLayer(new DemoLayer());

        // Run the application
        app.Run();
    }
}

/// <summary>
/// Demo layer showcasing engine features
/// </summary>
class DemoLayer : Layer
{
    private bool _vsyncEnabled = true;
    private int _frameCount = 0;
    private float _fpsTimer = 0f;
    private float _gamepadCheckTimer = 0f;

    public DemoLayer() : base("Demo Layer")
    {
    }

    public override void OnAttach()
    {
        Logger.Info("Demo layer attached");
        Logger.Info("Controls:");
        Logger.Info("  ESC - Exit application");
        Logger.Info("  F1  - Toggle VSync");
        Logger.Info("  WASD - Move camera (logged only)");
        Logger.Info("");
        Logger.Info("Gamepad Controls:");
        Logger.Info("  A/Cross - Action");
        Logger.Info("  B/Circle - Back");
        Logger.Info("  Start - Pause");
        Logger.Info("  Left Stick - Movement");
        Logger.Info("  Right Stick - Camera");
        
        // Check for connected gamepads
        int gamepadCount = GamepadInput.GetGamepadCount();
        Logger.Info($"Connected gamepads: {gamepadCount}");
        for (int i = 0; i < gamepadCount; i++)
        {
            Logger.Info($"  Gamepad {i}: {GamepadInput.GetGamepadName(i)}");
        }
    }

    public override void OnDetach()
    {
        Logger.Info("Demo layer detached");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Count FPS
        _frameCount++;
        _fpsTimer += deltaTime;

        if (_fpsTimer >= 1.0f)
        {
            Logger.Debug($"FPS: {_frameCount} | Frame Time: {deltaTime * 1000:F2}ms");
            _frameCount = 0;
            _fpsTimer = 0f;
        }

        // Check gamepad input
        _gamepadCheckTimer += deltaTime;
        if (_gamepadCheckTimer >= 0.1f) // Check every 100ms
        {
            _gamepadCheckTimer = 0f;
            CheckGamepadInput();
        }
    }

    public override void OnRender()
    {
        // Rendering is handled by Vulkan context
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            OnKeyPressed(keyEvent);
        }
        else if (e is GamepadConnectedEvent gamepadConnected)
        {
            Logger.Info($"Gamepad connected: {gamepadConnected.GamepadName}");
        }
        else if (e is GamepadDisconnectedEvent gamepadDisconnected)
        {
            Logger.Info($"Gamepad disconnected: {gamepadDisconnected.GamepadName}");
        }
        else if (e is GamepadButtonPressedEvent gamepadButton)
        {
            OnGamepadButtonPressed(gamepadButton);
        }
    }

    private void OnKeyPressed(KeyPressedEvent e)
    {
        if (e.IsRepeat)
            return;

        switch (e.KeyCode)
        {
            case Key.Escape:
                Logger.Info("Escape pressed - closing application");
                Application.Instance.Close();
                break;

            case Key.F1:
                _vsyncEnabled = !_vsyncEnabled;
                Logger.Info($"VSync: {(_vsyncEnabled ? "Enabled" : "Disabled")}");
                break;

            case Key.W:
                Logger.Debug("W pressed - move forward");
                break;

            case Key.A:
                Logger.Debug("A pressed - move left");
                break;

            case Key.S:
                Logger.Debug("S pressed - move backward");
                break;

            case Key.D:
                Logger.Debug("D pressed - move right");
                break;
        }
    }

    private void OnGamepadButtonPressed(GamepadButtonPressedEvent e)
    {
        switch (e.Button)
        {
            case ButtonName.A:
                Logger.Info("Gamepad A/Cross pressed - Action!");
                break;

            case ButtonName.B:
                Logger.Info("Gamepad B/Circle pressed - Back");
                break;

            case ButtonName.X:
                Logger.Info("Gamepad X/Square pressed");
                break;

            case ButtonName.Y:
                Logger.Info("Gamepad Y/Triangle pressed");
                break;

            case ButtonName.Start:
                Logger.Info("Gamepad Start pressed - Pause");
                break;

            case ButtonName.Back:
                Logger.Info("Gamepad Back/Select pressed");
                break;

            case ButtonName.LeftBumper:
                Logger.Info("Left bumper pressed");
                break;

            case ButtonName.RightBumper:
                Logger.Info("Right bumper pressed");
                break;
        }
    }

    private void CheckGamepadInput()
    {
        if (!GamepadInput.IsGamepadConnected(0))
            return;

        // Check left stick
        var leftStick = GamepadInput.GetLeftStick();
        if (leftStick.Length() > 0.2f) // Deadzone
        {
            Logger.Debug($"Left Stick: X={leftStick.X:F2}, Y={leftStick.Y:F2}");
        }

        // Check right stick
        var rightStick = GamepadInput.GetRightStick();
        if (rightStick.Length() > 0.2f) // Deadzone
        {
            Logger.Debug($"Right Stick: X={rightStick.X:F2}, Y={rightStick.Y:F2}");
        }

        // Check triggers
        var leftTrigger = GamepadInput.GetLeftTrigger();
        if (leftTrigger > 0.1f)
        {
            Logger.Debug($"Left Trigger: {leftTrigger:F2}");
        }

        var rightTrigger = GamepadInput.GetRightTrigger();
        if (rightTrigger > 0.1f)
        {
            Logger.Debug($"Right Trigger: {rightTrigger:F2}");
        }

        // Check D-pad
        var dpad = GamepadInput.GetDPad();
        if (dpad.up || dpad.down || dpad.left || dpad.right)
        {
            Logger.Debug($"D-Pad: Up={dpad.up}, Down={dpad.down}, Left={dpad.left}, Right={dpad.right}");
        }
    }
}
