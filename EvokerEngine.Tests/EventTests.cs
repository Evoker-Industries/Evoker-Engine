using Xunit;
using EvokerEngine.Core;

namespace EvokerEngine.Tests;

public class EventTests
{
    [Fact]
    public void Event_CanBeHandled()
    {
        // Arrange
        var evt = new WindowCloseEvent();

        // Act
        evt.Handled = true;

        // Assert
        Assert.True(evt.Handled);
    }

    [Fact]
    public void WindowResizeEvent_StoresSize()
    {
        // Arrange & Act
        var evt = new WindowResizeEvent(1920, 1080);

        // Assert
        Assert.Equal(1920, evt.Width);
        Assert.Equal(1080, evt.Height);
        Assert.Equal(EventType.WindowResize, evt.GetEventType());
    }

    [Fact]
    public void KeyPressedEvent_StoresKeyAndRepeat()
    {
        // Arrange & Act
        var evt = new KeyPressedEvent(Silk.NET.Input.Key.W, true);

        // Assert
        Assert.Equal(Silk.NET.Input.Key.W, evt.KeyCode);
        Assert.True(evt.IsRepeat);
        Assert.Equal(EventType.KeyPressed, evt.GetEventType());
    }

    [Fact]
    public void KeyReleasedEvent_StoresKey()
    {
        // Arrange & Act
        var evt = new KeyReleasedEvent(Silk.NET.Input.Key.A);

        // Assert
        Assert.Equal(Silk.NET.Input.Key.A, evt.KeyCode);
        Assert.Equal(EventType.KeyReleased, evt.GetEventType());
    }

    [Fact]
    public void MouseButtonPressedEvent_StoresButton()
    {
        // Arrange & Act
        var evt = new MouseButtonPressedEvent(Silk.NET.Input.MouseButton.Left);

        // Assert
        Assert.Equal(Silk.NET.Input.MouseButton.Left, evt.Button);
        Assert.Equal(EventType.MouseButtonPressed, evt.GetEventType());
    }

    [Fact]
    public void MouseMovedEvent_StoresPosition()
    {
        // Arrange & Act
        var evt = new MouseMovedEvent(100.5f, 200.5f);

        // Assert
        Assert.Equal(100.5f, evt.X);
        Assert.Equal(200.5f, evt.Y);
        Assert.Equal(EventType.MouseMoved, evt.GetEventType());
    }

    [Fact]
    public void MouseScrolledEvent_StoresOffset()
    {
        // Arrange & Act
        var evt = new MouseScrolledEvent(1.0f, -1.0f);

        // Assert
        Assert.Equal(1.0f, evt.OffsetX);
        Assert.Equal(-1.0f, evt.OffsetY);
        Assert.Equal(EventType.MouseScrolled, evt.GetEventType());
    }

    [Fact]
    public void GamepadConnectedEvent_StoresInfo()
    {
        // Arrange & Act
        var evt = new GamepadConnectedEvent(0, "Xbox Controller");

        // Assert
        Assert.Equal(0, evt.GamepadIndex);
        Assert.Equal("Xbox Controller", evt.GamepadName);
        Assert.Equal(EventType.GamepadConnected, evt.GetEventType());
    }

    [Fact]
    public void GamepadButtonPressedEvent_StoresButton()
    {
        // Arrange & Act
        var evt = new GamepadButtonPressedEvent(Silk.NET.Input.ButtonName.A, 0);

        // Assert
        Assert.Equal(Silk.NET.Input.ButtonName.A, evt.Button);
        Assert.Equal(0, evt.GamepadIndex);
        Assert.Equal(EventType.GamepadButtonPressed, evt.GetEventType());
    }

    [Fact]
    public void Event_ToString_ReturnsName()
    {
        // Arrange
        var evt = new WindowCloseEvent();

        // Act
        var str = evt.ToString();

        // Assert
        Assert.Equal(evt.GetName(), str);
    }
}
