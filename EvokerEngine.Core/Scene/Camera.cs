using System;
using System.Numerics;
using EvokerEngine.ECS;

namespace EvokerEngine.Scene;

/// <summary>
/// Camera for viewing the scene
/// </summary>
public class Camera
{
    public Vector3 Position { get; set; } = new Vector3(0, 0, 5);
    public Vector3 Target { get; set; } = Vector3.Zero;
    public Vector3 Up { get; set; } = Vector3.UnitY;
    
    public float FieldOfView { get; set; } = 45f;
    public float AspectRatio { get; set; } = 16f / 9f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 100f;

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Target, Up);
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfView * (float)Math.PI / 180f,
            AspectRatio,
            NearPlane,
            FarPlane);
    }

    public void LookAt(Vector3 target)
    {
        Target = target;
    }

    public void MoveForward(float amount)
    {
        var forward = Vector3.Normalize(Target - Position);
        Position += forward * amount;
        Target += forward * amount;
    }

    public void MoveRight(float amount)
    {
        var forward = Vector3.Normalize(Target - Position);
        var right = Vector3.Normalize(Vector3.Cross(forward, Up));
        Position += right * amount;
        Target += right * amount;
    }

    public void MoveUp(float amount)
    {
        Position += Up * amount;
        Target += Up * amount;
    }
}
