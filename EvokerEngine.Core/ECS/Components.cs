using System;
using System.Collections.Generic;
using System.Numerics;

namespace EvokerEngine.ECS;

/// <summary>
/// Transform component for entity position, rotation, and scale
/// </summary>
public class TransformComponent : Component
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public Matrix4x4 GetTransformMatrix()
    {
        return Matrix4x4.CreateScale(Scale) *
               Matrix4x4.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
               Matrix4x4.CreateTranslation(Position);
    }
}

/// <summary>
/// Mesh renderer component
/// </summary>
public class MeshRendererComponent : Component
{
    public int MeshId { get; set; }
    public int MaterialId { get; set; }
}

/// <summary>
/// Camera component
/// </summary>
public class CameraComponent : Component
{
    public float FieldOfView { get; set; } = 45f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 100f;
    public bool IsPrimary { get; set; } = false;

    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfView * (float)Math.PI / 180f,
            aspectRatio,
            NearPlane,
            FarPlane);
    }
}
