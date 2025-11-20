using System.Numerics;
using Xunit;
using EvokerEngine.UI;

namespace EvokerEngine.Tests;

public class HudTests
{
    [Fact]
    public void HudText_CanBeCreated()
    {
        // Arrange & Act
        var text = new HudText
        {
            Text = "Hello, World!",
            Position = new Vector2(100, 50),
            FontSize = 24,
            Color = new Vector4(1, 1, 1, 1)
        };

        // Assert
        Assert.Equal("Hello, World!", text.Text);
        Assert.Equal(24, text.FontSize);
        Assert.True(text.Visible);
    }

    [Fact]
    public void HudImage_CanBeCreated()
    {
        // Arrange & Act
        var image = new HudImage
        {
            TextureId = "player_icon",
            Position = new Vector2(50, 50),
            Size = new Vector2(64, 64)
        };

        // Assert
        Assert.Equal("player_icon", image.TextureId);
        Assert.Equal(64, image.Size.X);
        Assert.Equal(64, image.Size.Y);
    }

    [Fact]
    public void HudBar_CanBeCreated()
    {
        // Arrange & Act
        var bar = new HudBar
        {
            Position = new Vector2(10, 10),
            Size = new Vector2(200, 20),
            Value = 75,
            MaxValue = 100
        };

        // Assert
        Assert.Equal(75, bar.Value);
        Assert.Equal(100, bar.MaxValue);
        Assert.Equal(BarFillDirection.LeftToRight, bar.FillDirection);
    }

    [Fact]
    public void HudPanel_CanBeCreated()
    {
        // Arrange & Act
        var panel = new HudPanel
        {
            Position = new Vector2(0, 0),
            Size = new Vector2(300, 200),
            BackgroundColor = new Vector4(0, 0, 0, 0.5f)
        };

        // Assert
        Assert.Equal(300, panel.Size.X);
        Assert.Equal(200, panel.Size.Y);
        Assert.True(panel.ShowBorder);
    }

    [Fact]
    public void HudButton_CanBeCreated()
    {
        // Arrange & Act
        var clicked = false;
        var button = new HudButton
        {
            Text = "Click Me",
            Position = new Vector2(100, 100),
            Size = new Vector2(150, 50)
        };
        button.OnClick += () => clicked = true;

        // Assert
        Assert.Equal("Click Me", button.Text);
        Assert.False(clicked);
        
        button.Click();
        Assert.True(clicked);
    }

    [Fact]
    public void HudElement_SupportsHierarchy()
    {
        // Arrange
        var parent = new HudPanel
        {
            Position = new Vector2(100, 100)
        };
        var child = new HudText
        {
            Text = "Child",
            Position = new Vector2(10, 10)
        };

        // Act
        parent.AddChild(child);

        // Assert
        Assert.Equal(parent, child.Parent);
        Assert.Single(parent.GetChildren());
        Assert.Equal(child, parent.GetChildren()[0]);
    }

    [Fact]
    public void HudElement_CalculatesScreenPosition()
    {
        // Arrange
        var parent = new HudPanel
        {
            Position = new Vector2(100, 100)
        };
        var child = new HudText
        {
            Text = "Child",
            Position = new Vector2(10, 10)
        };
        parent.AddChild(child);

        // Act
        var screenPos = child.GetScreenPosition();

        // Assert
        Assert.Equal(110, screenPos.X);
        Assert.Equal(110, screenPos.Y);
    }

    [Fact]
    public void HudElement_ContainsPoint_Works()
    {
        // Arrange
        var element = new HudPanel
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(200, 150)
        };

        // Assert
        Assert.True(element.ContainsPoint(new Vector2(150, 150)));
        Assert.True(element.ContainsPoint(new Vector2(100, 100)));
        Assert.True(element.ContainsPoint(new Vector2(299, 249)));
        Assert.False(element.ContainsPoint(new Vector2(50, 50)));
        Assert.False(element.ContainsPoint(new Vector2(350, 300)));
    }

    [Fact]
    public void HudManager_CanAddElements()
    {
        // Arrange
        var manager = HudManager.Instance;
        manager.Clear();
        
        var text = new HudText { Text = "Test" };

        // Act
        manager.AddElement(text);

        // Assert
        Assert.Single(manager.GetRootElements());
        Assert.Equal(text, manager.GetRootElements()[0]);
        
        // Cleanup
        manager.Clear();
    }

    [Fact]
    public void HudManager_CanRemoveElements()
    {
        // Arrange
        var manager = HudManager.Instance;
        manager.Clear();
        
        var text = new HudText { Text = "Test" };
        manager.AddElement(text);

        // Act
        manager.RemoveElement(text);

        // Assert
        Assert.Empty(manager.GetRootElements());
        
        // Cleanup
        manager.Clear();
    }

    [Fact]
    public void HudManager_CanFindByTag()
    {
        // Arrange
        var manager = HudManager.Instance;
        manager.Clear();
        
        var text = new HudText { Text = "Test", Tag = "test_label" };
        manager.AddElement(text);

        // Act
        var found = manager.FindByTag("test_label");

        // Assert
        Assert.NotNull(found);
        Assert.Equal(text, found);
        
        // Cleanup
        manager.Clear();
    }

    [Fact]
    public void HudManager_AddText_Helper()
    {
        // Arrange
        var manager = HudManager.Instance;
        manager.Clear();

        // Act
        var text = manager.AddText("Hello", new Vector2(10, 10), 20);

        // Assert
        Assert.Equal("Hello", text.Text);
        Assert.Equal(20, text.FontSize);
        Assert.Single(manager.GetRootElements());
        
        // Cleanup
        manager.Clear();
    }

    [Fact]
    public void HudManager_AddHealthBar_Helper()
    {
        // Arrange
        var manager = HudManager.Instance;
        manager.Clear();

        // Act
        var bar = manager.AddHealthBar(new Vector2(10, 10), new Vector2(200, 20), 75, 100);

        // Assert
        Assert.Equal(75, bar.Value);
        Assert.Equal(100, bar.MaxValue);
        Assert.Single(manager.GetRootElements());
        
        // Cleanup
        manager.Clear();
    }

    [Fact]
    public void HudManager_CreateDebugPanel_Helper()
    {
        // Arrange
        var manager = HudManager.Instance;
        manager.Clear();

        // Act
        var panel = manager.CreateDebugPanel(new Vector2(10, 10), "Line 1", "Line 2", "Line 3");

        // Assert
        Assert.Equal(3, panel.GetChildren().Count);
        Assert.Single(manager.GetRootElements());
        
        // Cleanup
        manager.Clear();
    }

    [Fact]
    public void HudManager_CreateInventoryDisplay_Helper()
    {
        // Arrange
        var manager = HudManager.Instance;
        manager.Clear();

        // Act
        var panel = manager.CreateInventoryDisplay(new Vector2(10, 10), slots: 9);

        // Assert
        Assert.Equal(9, panel.GetChildren().Count);
        Assert.Single(manager.GetRootElements());
        
        // Cleanup
        manager.Clear();
    }

    [Fact]
    public void HudManager_CreatePlayerStatsHUD_Helper()
    {
        // Arrange
        var manager = HudManager.Instance;
        manager.Clear();

        // Act
        var panel = manager.CreatePlayerStatsHUD(new Vector2(10, 10), 100, 100, 50, 100, 80, 100);

        // Assert
        Assert.Equal(3, panel.GetChildren().Count); // 3 bars
        var healthBar = panel.GetChildren().FirstOrDefault(c => c.Tag == "health_bar") as HudBar;
        Assert.NotNull(healthBar);
        Assert.Equal(100, healthBar.Value);
        
        // Cleanup
        manager.Clear();
    }

    [Fact]
    public void HudManager_CreateCrosshair_Helper()
    {
        // Arrange
        var manager = HudManager.Instance;
        manager.Clear();

        // Act
        manager.CreateCrosshair(20f);

        // Assert
        Assert.Equal(2, manager.GetRootElements().Count); // Horizontal and vertical lines
        Assert.NotNull(manager.FindByTag("crosshair_h"));
        Assert.NotNull(manager.FindByTag("crosshair_v"));
        
        // Cleanup
        manager.Clear();
    }

    [Fact]
    public void HudElement_VisibilityAndEnabled()
    {
        // Arrange
        var element = new HudText { Text = "Test" };

        // Assert defaults
        Assert.True(element.Visible);
        Assert.True(element.Enabled);

        // Act
        element.Visible = false;
        element.Enabled = false;

        // Assert
        Assert.False(element.Visible);
        Assert.False(element.Enabled);
    }

    [Fact]
    public void HudElement_LayerOrdering()
    {
        // Arrange
        var element1 = new HudText { Text = "Back", Layer = 0 };
        var element2 = new HudText { Text = "Front", Layer = 10 };

        // Assert
        Assert.True(element2.Layer > element1.Layer);
    }
}
