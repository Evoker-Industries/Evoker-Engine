using System;
using System.Numerics;

namespace EvokerEngine.UI;

/// <summary>
/// Base class for all HUD elements
/// </summary>
public abstract class HudElement
{
    /// <summary>
    /// Position of the HUD element on screen (in pixels or normalized coordinates)
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Size of the HUD element (in pixels or normalized coordinates)
    /// </summary>
    public Vector2 Size { get; set; }

    /// <summary>
    /// Whether the element is visible
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Whether the element is enabled (can receive input)
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Anchor point for positioning (0,0 = top-left, 1,1 = bottom-right)
    /// </summary>
    public Vector2 Anchor { get; set; } = Vector2.Zero;

    /// <summary>
    /// Pivot point for rotation and scaling (0,0 = top-left, 0.5,0.5 = center)
    /// </summary>
    public Vector2 Pivot { get; set; } = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// Rotation in degrees
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// Scale factor
    /// </summary>
    public Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Alpha/opacity (0.0 = transparent, 1.0 = opaque)
    /// </summary>
    public float Alpha { get; set; } = 1.0f;

    /// <summary>
    /// Layer/Z-order for rendering (higher values drawn on top)
    /// </summary>
    public int Layer { get; set; }

    /// <summary>
    /// Tag for identification
    /// </summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    /// Parent element (null if root element)
    /// </summary>
    public HudElement? Parent { get; set; }

    /// <summary>
    /// Child elements
    /// </summary>
    protected List<HudElement> Children { get; } = new();

    /// <summary>
    /// Add a child element
    /// </summary>
    public void AddChild(HudElement child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        child.Parent = this;
        Children.Add(child);
    }

    /// <summary>
    /// Remove a child element
    /// </summary>
    public void RemoveChild(HudElement child)
    {
        if (child == null)
            return;

        child.Parent = null;
        Children.Remove(child);
    }

    /// <summary>
    /// Get all children
    /// </summary>
    public IReadOnlyList<HudElement> GetChildren() => Children.AsReadOnly();

    /// <summary>
    /// Get screen position accounting for parent hierarchy
    /// </summary>
    public Vector2 GetScreenPosition()
    {
        var pos = Position;
        if (Parent != null)
        {
            pos += Parent.GetScreenPosition();
        }
        return pos;
    }

    /// <summary>
    /// Update the element
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        foreach (var child in Children)
        {
            if (child.Enabled)
            {
                child.Update(deltaTime);
            }
        }
    }

    /// <summary>
    /// Render the element
    /// </summary>
    public abstract void Render();

    /// <summary>
    /// Check if a point is inside the element bounds
    /// </summary>
    public virtual bool ContainsPoint(Vector2 point)
    {
        var screenPos = GetScreenPosition();
        return point.X >= screenPos.X && point.X <= screenPos.X + Size.X &&
               point.Y >= screenPos.Y && point.Y <= screenPos.Y + Size.Y;
    }
}
