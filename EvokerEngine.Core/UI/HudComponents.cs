using System;
using System.Numerics;
using EvokerEngine.Core;

namespace EvokerEngine.UI;

/// <summary>
/// HUD element for displaying text
/// </summary>
public class HudText : HudElement
{
    /// <summary>
    /// Text content to display
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Font size
    /// </summary>
    public float FontSize { get; set; } = 16.0f;

    /// <summary>
    /// Text color
    /// </summary>
    public Vector4 Color { get; set; } = new Vector4(1, 1, 1, 1); // White

    /// <summary>
    /// Text alignment
    /// </summary>
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;

    /// <summary>
    /// Whether to draw a shadow
    /// </summary>
    public bool DrawShadow { get; set; } = false;

    /// <summary>
    /// Shadow offset
    /// </summary>
    public Vector2 ShadowOffset { get; set; } = new Vector2(2, 2);

    /// <summary>
    /// Shadow color
    /// </summary>
    public Vector4 ShadowColor { get; set; } = new Vector4(0, 0, 0, 0.5f);

    public override void Render()
    {
        if (!Visible || string.IsNullOrEmpty(Text))
            return;

        var screenPos = GetScreenPosition();
        
        // Placeholder for actual rendering
        // In a real implementation, this would call a rendering API
        Logger.Debug($"Rendering text '{Text}' at {screenPos} with size {FontSize}");
    }
}

/// <summary>
/// Text alignment options
/// </summary>
public enum TextAlignment
{
    Left,
    Center,
    Right
}

/// <summary>
/// HUD element for displaying images/sprites
/// </summary>
public class HudImage : HudElement
{
    /// <summary>
    /// Texture/image ID
    /// </summary>
    public string TextureId { get; set; } = string.Empty;

    /// <summary>
    /// Tint color (white = no tint)
    /// </summary>
    public Vector4 Tint { get; set; } = Vector4.One;

    /// <summary>
    /// UV coordinates for texture sampling (for sprite sheets)
    /// </summary>
    public Vector4 UVRect { get; set; } = new Vector4(0, 0, 1, 1);

    public override void Render()
    {
        if (!Visible || string.IsNullOrEmpty(TextureId))
            return;

        var screenPos = GetScreenPosition();
        Logger.Debug($"Rendering image '{TextureId}' at {screenPos} with size {Size}");
    }
}

/// <summary>
/// HUD element for displaying a health/progress bar
/// </summary>
public class HudBar : HudElement
{
    /// <summary>
    /// Current value (0.0 to 1.0)
    /// </summary>
    public float Value { get; set; } = 1.0f;

    /// <summary>
    /// Maximum value
    /// </summary>
    public float MaxValue { get; set; } = 1.0f;

    /// <summary>
    /// Background color
    /// </summary>
    public Vector4 BackgroundColor { get; set; } = new Vector4(0.2f, 0.2f, 0.2f, 0.8f);

    /// <summary>
    /// Foreground/fill color
    /// </summary>
    public Vector4 FillColor { get; set; } = new Vector4(0, 1, 0, 1); // Green

    /// <summary>
    /// Border color
    /// </summary>
    public Vector4 BorderColor { get; set; } = new Vector4(1, 1, 1, 1);

    /// <summary>
    /// Border thickness
    /// </summary>
    public float BorderThickness { get; set; } = 2.0f;

    /// <summary>
    /// Whether to show border
    /// </summary>
    public bool ShowBorder { get; set; } = true;

    /// <summary>
    /// Fill direction
    /// </summary>
    public BarFillDirection FillDirection { get; set; } = BarFillDirection.LeftToRight;

    public override void Render()
    {
        if (!Visible)
            return;

        var screenPos = GetScreenPosition();
        var fillPercent = Value / MaxValue;
        Logger.Debug($"Rendering bar at {screenPos} with fill {fillPercent * 100}%");
    }
}

/// <summary>
/// Bar fill direction
/// </summary>
public enum BarFillDirection
{
    LeftToRight,
    RightToLeft,
    BottomToTop,
    TopToBottom
}

/// <summary>
/// HUD element for displaying a panel/container
/// </summary>
public class HudPanel : HudElement
{
    /// <summary>
    /// Background color
    /// </summary>
    public Vector4 BackgroundColor { get; set; } = new Vector4(0, 0, 0, 0.5f);

    /// <summary>
    /// Border color
    /// </summary>
    public Vector4 BorderColor { get; set; } = new Vector4(1, 1, 1, 1);

    /// <summary>
    /// Border thickness
    /// </summary>
    public float BorderThickness { get; set; } = 2.0f;

    /// <summary>
    /// Corner radius for rounded corners
    /// </summary>
    public float CornerRadius { get; set; } = 0.0f;

    /// <summary>
    /// Whether to show border
    /// </summary>
    public bool ShowBorder { get; set; } = true;

    public override void Render()
    {
        if (!Visible)
            return;

        var screenPos = GetScreenPosition();
        
        // Render background
        Logger.Debug($"Rendering panel at {screenPos} with size {Size}");
        
        // Render children
        foreach (var child in Children.OrderBy(c => c.Layer))
        {
            if (child.Visible)
            {
                child.Render();
            }
        }
    }
}

/// <summary>
/// HUD element for a button
/// </summary>
public class HudButton : HudElement
{
    /// <summary>
    /// Button text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Normal state color
    /// </summary>
    public Vector4 NormalColor { get; set; } = new Vector4(0.3f, 0.3f, 0.3f, 1);

    /// <summary>
    /// Hover state color
    /// </summary>
    public Vector4 HoverColor { get; set; } = new Vector4(0.4f, 0.4f, 0.4f, 1);

    /// <summary>
    /// Pressed state color
    /// </summary>
    public Vector4 PressedColor { get; set; } = new Vector4(0.2f, 0.2f, 0.2f, 1);

    /// <summary>
    /// Text color
    /// </summary>
    public Vector4 TextColor { get; set; } = new Vector4(1, 1, 1, 1);

    /// <summary>
    /// Whether the button is currently hovered
    /// </summary>
    public bool IsHovered { get; set; }

    /// <summary>
    /// Whether the button is currently pressed
    /// </summary>
    public bool IsPressed { get; set; }

    /// <summary>
    /// Event fired when button is clicked
    /// </summary>
    public event Action? OnClick;

    public void Click()
    {
        if (Enabled)
        {
            OnClick?.Invoke();
        }
    }

    public override void Render()
    {
        if (!Visible)
            return;

        var screenPos = GetScreenPosition();
        var color = IsPressed ? PressedColor : (IsHovered ? HoverColor : NormalColor);
        
        Logger.Debug($"Rendering button '{Text}' at {screenPos}");
    }
}
