using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using EvokerEngine.Core;

namespace EvokerEngine.UI;

/// <summary>
/// Manages all HUD elements and provides easy-to-use API for drawing HUDs
/// </summary>
public class HudManager
{
    private readonly List<HudElement> _rootElements;
    private static HudManager? _instance;

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static HudManager Instance => _instance ??= new HudManager();

    /// <summary>
    /// Screen width in pixels
    /// </summary>
    public float ScreenWidth { get; set; } = 1920;

    /// <summary>
    /// Screen height in pixels
    /// </summary>
    public float ScreenHeight { get; set; } = 1080;

    private HudManager()
    {
        _rootElements = new List<HudElement>();
    }

    /// <summary>
    /// Add a HUD element to the root
    /// </summary>
    public void AddElement(HudElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        _rootElements.Add(element);
        Logger.Info($"Added HUD element: {element.GetType().Name} at {element.Position}");
    }

    /// <summary>
    /// Remove a HUD element from the root
    /// </summary>
    public void RemoveElement(HudElement element)
    {
        if (element == null)
            return;

        _rootElements.Remove(element);
    }

    /// <summary>
    /// Find element by tag
    /// </summary>
    public HudElement? FindByTag(string tag)
    {
        return _rootElements.FirstOrDefault(e => e.Tag == tag);
    }

    /// <summary>
    /// Get all root elements
    /// </summary>
    public IReadOnlyList<HudElement> GetRootElements() => _rootElements.AsReadOnly();

    /// <summary>
    /// Clear all HUD elements
    /// </summary>
    public void Clear()
    {
        _rootElements.Clear();
    }

    /// <summary>
    /// Update all HUD elements
    /// </summary>
    public void Update(float deltaTime)
    {
        foreach (var element in _rootElements.Where(e => e.Enabled))
        {
            element.Update(deltaTime);
        }
    }

    /// <summary>
    /// Render all HUD elements
    /// </summary>
    public void Render()
    {
        // Sort by layer and render
        var sortedElements = _rootElements.OrderBy(e => e.Layer);
        foreach (var element in sortedElements.Where(e => e.Visible))
        {
            element.Render();
        }
    }

    /// <summary>
    /// Helper: Create and add a text element
    /// </summary>
    public HudText AddText(string text, Vector2 position, float fontSize = 16.0f, Vector4? color = null)
    {
        var textElement = new HudText
        {
            Text = text,
            Position = position,
            FontSize = fontSize,
            Color = color ?? new Vector4(1, 1, 1, 1)
        };
        AddElement(textElement);
        return textElement;
    }

    /// <summary>
    /// Helper: Create and add an image element
    /// </summary>
    public HudImage AddImage(string textureId, Vector2 position, Vector2 size, Vector4? tint = null)
    {
        var imageElement = new HudImage
        {
            TextureId = textureId,
            Position = position,
            Size = size,
            Tint = tint ?? Vector4.One
        };
        AddElement(imageElement);
        return imageElement;
    }

    /// <summary>
    /// Helper: Create and add a health bar
    /// </summary>
    public HudBar AddHealthBar(Vector2 position, Vector2 size, float currentHealth, float maxHealth)
    {
        var bar = new HudBar
        {
            Position = position,
            Size = size,
            Value = currentHealth,
            MaxValue = maxHealth,
            FillColor = new Vector4(0, 1, 0, 1), // Green
            BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 0.8f)
        };
        AddElement(bar);
        return bar;
    }

    /// <summary>
    /// Helper: Create and add a mana/stamina bar
    /// </summary>
    public HudBar AddBar(Vector2 position, Vector2 size, float currentValue, float maxValue, Vector4 fillColor)
    {
        var bar = new HudBar
        {
            Position = position,
            Size = size,
            Value = currentValue,
            MaxValue = maxValue,
            FillColor = fillColor,
            BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 0.8f)
        };
        AddElement(bar);
        return bar;
    }

    /// <summary>
    /// Helper: Create and add a panel
    /// </summary>
    public HudPanel AddPanel(Vector2 position, Vector2 size, Vector4? backgroundColor = null)
    {
        var panel = new HudPanel
        {
            Position = position,
            Size = size,
            BackgroundColor = backgroundColor ?? new Vector4(0, 0, 0, 0.5f)
        };
        AddElement(panel);
        return panel;
    }

    /// <summary>
    /// Helper: Create and add a button
    /// </summary>
    public HudButton AddButton(string text, Vector2 position, Vector2 size, Action? onClick = null)
    {
        var button = new HudButton
        {
            Text = text,
            Position = position,
            Size = size
        };
        
        if (onClick != null)
        {
            button.OnClick += onClick;
        }
        
        AddElement(button);
        return button;
    }

    /// <summary>
    /// Helper: Create a quick debug display with multiple lines of text
    /// </summary>
    public HudPanel CreateDebugPanel(Vector2 position, params string[] lines)
    {
        var panel = AddPanel(position, new Vector2(300, 20 + lines.Length * 20), new Vector4(0, 0, 0, 0.7f));
        
        for (int i = 0; i < lines.Length; i++)
        {
            var text = new HudText
            {
                Text = lines[i],
                Position = new Vector2(10, 10 + i * 20),
                FontSize = 14,
                Color = new Vector4(1, 1, 1, 1)
            };
            panel.AddChild(text);
        }
        
        return panel;
    }

    /// <summary>
    /// Helper: Create a simple inventory display
    /// </summary>
    public HudPanel CreateInventoryDisplay(Vector2 position, int slots = 9, float slotSize = 50f)
    {
        var panel = AddPanel(position, new Vector2((slotSize + 5) * slots + 5, slotSize + 10), 
            new Vector4(0.1f, 0.1f, 0.1f, 0.9f));
        
        for (int i = 0; i < slots; i++)
        {
            var slot = new HudPanel
            {
                Position = new Vector2(5 + i * (slotSize + 5), 5),
                Size = new Vector2(slotSize, slotSize),
                BackgroundColor = new Vector4(0.3f, 0.3f, 0.3f, 1),
                BorderColor = new Vector4(1, 1, 1, 1),
                ShowBorder = true,
                Tag = $"inventory_slot_{i}"
            };
            panel.AddChild(slot);
        }
        
        return panel;
    }

    /// <summary>
    /// Helper: Create a player stats HUD (health, mana, stamina)
    /// </summary>
    public HudPanel CreatePlayerStatsHUD(Vector2 position, float health, float maxHealth, 
        float mana, float maxMana, float stamina, float maxStamina)
    {
        var panel = AddPanel(position, new Vector2(250, 90), new Vector4(0, 0, 0, 0.3f));
        
        // Health bar
        var healthBar = new HudBar
        {
            Position = new Vector2(10, 10),
            Size = new Vector2(230, 20),
            Value = health,
            MaxValue = maxHealth,
            FillColor = new Vector4(1, 0, 0, 1), // Red
            Tag = "health_bar"
        };
        panel.AddChild(healthBar);
        
        // Mana bar
        var manaBar = new HudBar
        {
            Position = new Vector2(10, 35),
            Size = new Vector2(230, 20),
            Value = mana,
            MaxValue = maxMana,
            FillColor = new Vector4(0, 0, 1, 1), // Blue
            Tag = "mana_bar"
        };
        panel.AddChild(manaBar);
        
        // Stamina bar
        var staminaBar = new HudBar
        {
            Position = new Vector2(10, 60),
            Size = new Vector2(230, 20),
            Value = stamina,
            MaxValue = maxStamina,
            FillColor = new Vector4(0, 1, 0, 1), // Green
            Tag = "stamina_bar"
        };
        panel.AddChild(staminaBar);
        
        return panel;
    }

    /// <summary>
    /// Helper: Create a minimap display
    /// </summary>
    public HudPanel CreateMinimap(Vector2 position, float size = 150f)
    {
        var panel = AddPanel(position, new Vector2(size, size), new Vector4(0, 0, 0, 0.5f));
        panel.Tag = "minimap";
        panel.BorderThickness = 3;
        panel.ShowBorder = true;
        
        return panel;
    }

    /// <summary>
    /// Helper: Create a crosshair at screen center
    /// </summary>
    public void CreateCrosshair(float size = 20f, Vector4? color = null)
    {
        var center = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
        var crosshairColor = color ?? new Vector4(1, 1, 1, 0.8f);
        
        // Horizontal line
        var horizontal = new HudPanel
        {
            Position = new Vector2(center.X - size / 2, center.Y - 1),
            Size = new Vector2(size, 2),
            BackgroundColor = crosshairColor,
            ShowBorder = false,
            Tag = "crosshair_h"
        };
        AddElement(horizontal);
        
        // Vertical line
        var vertical = new HudPanel
        {
            Position = new Vector2(center.X - 1, center.Y - size / 2),
            Size = new Vector2(2, size),
            BackgroundColor = crosshairColor,
            ShowBorder = false,
            Tag = "crosshair_v"
        };
        AddElement(vertical);
    }
}
