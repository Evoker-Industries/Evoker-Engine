using System;
using System.Numerics;

namespace EvokerEngine.Rendering;

/// <summary>
/// 2D sprite component for rendering sprites
/// </summary>
public class SpriteComponent : ECS.Component
{
    /// <summary>
    /// Texture resource ID
    /// </summary>
    public string TextureId { get; set; } = string.Empty;

    /// <summary>
    /// Sprite color tint
    /// </summary>
    public Vector4 Color { get; set; } = Vector4.One;

    /// <summary>
    /// Sprite size in world units
    /// </summary>
    public Vector2 Size { get; set; } = Vector2.One;

    /// <summary>
    /// UV coordinates (for sprite sheets)
    /// </summary>
    public Vector4 UV { get; set; } = new Vector4(0, 0, 1, 1);

    /// <summary>
    /// Rendering layer/order
    /// </summary>
    public int SortingLayer { get; set; } = 0;

    /// <summary>
    /// Order within the sorting layer
    /// </summary>
    public int OrderInLayer { get; set; } = 0;

    /// <summary>
    /// Whether the sprite is visible
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Flip the sprite horizontally
    /// </summary>
    public bool FlipX { get; set; } = false;

    /// <summary>
    /// Flip the sprite vertically
    /// </summary>
    public bool FlipY { get; set; } = false;
}

/// <summary>
/// Animated sprite component for 2D animations
/// </summary>
public class AnimatedSpriteComponent : SpriteComponent
{
    /// <summary>
    /// Animation frames (sprite sheet coordinates)
    /// </summary>
    public Vector4[] Frames { get; set; } = Array.Empty<Vector4>();

    /// <summary>
    /// Current frame index
    /// </summary>
    public int CurrentFrame { get; set; } = 0;

    /// <summary>
    /// Frames per second
    /// </summary>
    public float FrameRate { get; set; } = 10f;

    /// <summary>
    /// Whether to loop the animation
    /// </summary>
    public bool Loop { get; set; } = true;

    /// <summary>
    /// Whether the animation is playing
    /// </summary>
    public bool IsPlaying { get; set; } = true;

    private float _frameTimer = 0f;

    /// <summary>
    /// Update the animation
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!IsPlaying || Frames.Length == 0) return;

        _frameTimer += deltaTime;
        var frameDuration = 1f / FrameRate;

        while (_frameTimer >= frameDuration)
        {
            _frameTimer -= frameDuration;
            CurrentFrame++;

            if (CurrentFrame >= Frames.Length)
            {
                if (Loop)
                {
                    CurrentFrame = 0;
                }
                else
                {
                    CurrentFrame = Frames.Length - 1;
                    IsPlaying = false;
                }
            }
        }

        if (CurrentFrame < Frames.Length)
        {
            UV = Frames[CurrentFrame];
        }
    }

    /// <summary>
    /// Play the animation from the beginning
    /// </summary>
    public void Play()
    {
        IsPlaying = true;
        CurrentFrame = 0;
        _frameTimer = 0f;
    }

    /// <summary>
    /// Stop the animation
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
    }

    /// <summary>
    /// Pause the animation
    /// </summary>
    public void Pause()
    {
        IsPlaying = false;
    }

    /// <summary>
    /// Resume the animation
    /// </summary>
    public void Resume()
    {
        IsPlaying = true;
    }
}

/// <summary>
/// 2D Tilemap component
/// </summary>
public class TilemapComponent : ECS.Component
{
    /// <summary>
    /// Tileset texture ID
    /// </summary>
    public string TilesetId { get; set; } = string.Empty;

    /// <summary>
    /// Tile size in pixels
    /// </summary>
    public Vector2 TileSize { get; set; } = new Vector2(32, 32);

    /// <summary>
    /// Map width in tiles
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Map height in tiles
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Tile data (tile indices)
    /// </summary>
    public int[,] Tiles { get; set; } = new int[0, 0];

    /// <summary>
    /// Initialize tilemap with dimensions
    /// </summary>
    public void Initialize(int width, int height)
    {
        Width = width;
        Height = height;
        Tiles = new int[height, width];
    }

    /// <summary>
    /// Set a tile at position
    /// </summary>
    public void SetTile(int x, int y, int tileIndex)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            Tiles[y, x] = tileIndex;
        }
    }

    /// <summary>
    /// Get a tile at position
    /// </summary>
    public int GetTile(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return Tiles[y, x];
        }
        return -1;
    }
}

/// <summary>
/// 2D Camera component
/// </summary>
public class Camera2DComponent : ECS.Component
{
    /// <summary>
    /// Camera position in 2D space
    /// </summary>
    public Vector2 Position { get; set; } = Vector2.Zero;

    /// <summary>
    /// Camera zoom level
    /// </summary>
    public float Zoom { get; set; } = 1f;

    /// <summary>
    /// Camera rotation in degrees
    /// </summary>
    public float Rotation { get; set; } = 0f;

    /// <summary>
    /// Viewport size
    /// </summary>
    public Vector2 ViewportSize { get; set; } = new Vector2(1280, 720);

    /// <summary>
    /// Whether this is the primary camera
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Get view matrix for 2D rendering
    /// </summary>
    public Matrix4x4 GetViewMatrix()
    {
        var translation = Matrix4x4.CreateTranslation(-Position.X, -Position.Y, 0);
        var rotation = Matrix4x4.CreateRotationZ(Rotation * (float)Math.PI / 180f);
        var scale = Matrix4x4.CreateScale(Zoom, Zoom, 1f);
        
        return translation * rotation * scale;
    }

    /// <summary>
    /// Get orthographic projection matrix
    /// </summary>
    public Matrix4x4 GetProjectionMatrix()
    {
        var halfWidth = ViewportSize.X / 2f;
        var halfHeight = ViewportSize.Y / 2f;
        
        return Matrix4x4.CreateOrthographic(
            ViewportSize.X,
            ViewportSize.Y,
            -1f,
            1f
        );
    }

    /// <summary>
    /// Convert screen position to world position
    /// </summary>
    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        var centerOffset = new Vector2(ViewportSize.X / 2f, ViewportSize.Y / 2f);
        var relativePos = screenPos - centerOffset;
        
        return Position + relativePos / Zoom;
    }

    /// <summary>
    /// Convert world position to screen position
    /// </summary>
    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        var relativePos = (worldPos - Position) * Zoom;
        var centerOffset = new Vector2(ViewportSize.X / 2f, ViewportSize.Y / 2f);
        
        return relativePos + centerOffset;
    }
}
