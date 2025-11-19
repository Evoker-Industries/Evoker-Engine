using System;
using System.Numerics;

namespace EvokerEngine.Rendering;

/// <summary>
/// 3D Mesh component for rendering 3D models
/// </summary>
public class MeshComponent : ECS.Component
{
    /// <summary>
    /// Mesh resource ID
    /// </summary>
    public string MeshId { get; set; } = string.Empty;

    /// <summary>
    /// Material resource ID
    /// </summary>
    public string MaterialId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to cast shadows
    /// </summary>
    public bool CastShadows { get; set; } = true;

    /// <summary>
    /// Whether to receive shadows
    /// </summary>
    public bool ReceiveShadows { get; set; } = true;

    /// <summary>
    /// Whether the mesh is visible
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Rendering layer mask
    /// </summary>
    public int LayerMask { get; set; } = 1;
}

/// <summary>
/// Light component for 3D lighting
/// </summary>
public class LightComponent : ECS.Component
{
    /// <summary>
    /// Type of light
    /// </summary>
    public LightType Type { get; set; } = LightType.Point;

    /// <summary>
    /// Light color
    /// </summary>
    public Vector3 Color { get; set; } = Vector3.One;

    /// <summary>
    /// Light intensity
    /// </summary>
    public float Intensity { get; set; } = 1f;

    /// <summary>
    /// Light range (for point and spot lights)
    /// </summary>
    public float Range { get; set; } = 10f;

    /// <summary>
    /// Spot angle in degrees (for spot lights)
    /// </summary>
    public float SpotAngle { get; set; } = 45f;

    /// <summary>
    /// Whether to cast shadows
    /// </summary>
    public bool CastShadows { get; set; } = true;

    /// <summary>
    /// Shadow strength
    /// </summary>
    public float ShadowStrength { get; set; } = 1f;
}

/// <summary>
/// Types of lights
/// </summary>
public enum LightType
{
    Directional,
    Point,
    Spot,
    Area
}

/// <summary>
/// 3D Camera component with advanced features
/// </summary>
public class Camera3DComponent : ECS.Component
{
    /// <summary>
    /// Field of view in degrees
    /// </summary>
    public float FieldOfView { get; set; } = 60f;

    /// <summary>
    /// Near clipping plane
    /// </summary>
    public float NearPlane { get; set; } = 0.1f;

    /// <summary>
    /// Far clipping plane
    /// </summary>
    public float FarPlane { get; set; } = 1000f;

    /// <summary>
    /// Aspect ratio (width / height)
    /// </summary>
    public float AspectRatio { get; set; } = 16f / 9f;

    /// <summary>
    /// Whether this is the primary camera
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Camera clear color
    /// </summary>
    public Vector4 ClearColor { get; set; } = new Vector4(0.1f, 0.1f, 0.1f, 1f);

    /// <summary>
    /// Whether to use orthographic projection
    /// </summary>
    public bool Orthographic { get; set; } = false;

    /// <summary>
    /// Orthographic size (half-height)
    /// </summary>
    public float OrthographicSize { get; set; } = 5f;

    /// <summary>
    /// Get perspective projection matrix
    /// </summary>
    public Matrix4x4 GetPerspectiveMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfView * (float)Math.PI / 180f,
            AspectRatio,
            NearPlane,
            FarPlane
        );
    }

    /// <summary>
    /// Get orthographic projection matrix
    /// </summary>
    public Matrix4x4 GetOrthographicMatrix()
    {
        var width = OrthographicSize * AspectRatio;
        var height = OrthographicSize;
        
        return Matrix4x4.CreateOrthographic(
            width * 2f,
            height * 2f,
            NearPlane,
            FarPlane
        );
    }

    /// <summary>
    /// Get projection matrix (perspective or orthographic)
    /// </summary>
    public Matrix4x4 GetProjectionMatrix()
    {
        return Orthographic ? GetOrthographicMatrix() : GetPerspectiveMatrix();
    }
}

/// <summary>
/// Skybox component for rendering environment
/// </summary>
public class SkyboxComponent : ECS.Component
{
    /// <summary>
    /// Cubemap texture ID
    /// </summary>
    public string CubemapId { get; set; } = string.Empty;

    /// <summary>
    /// Skybox tint color
    /// </summary>
    public Vector3 Tint { get; set; } = Vector3.One;

    /// <summary>
    /// Exposure multiplier
    /// </summary>
    public float Exposure { get; set; } = 1f;

    /// <summary>
    /// Rotation of the skybox
    /// </summary>
    public float Rotation { get; set; } = 0f;
}

/// <summary>
/// Particle system component for visual effects
/// </summary>
public class ParticleSystemComponent : ECS.Component
{
    /// <summary>
    /// Maximum number of particles
    /// </summary>
    public int MaxParticles { get; set; } = 1000;

    /// <summary>
    /// Particles per second
    /// </summary>
    public float EmissionRate { get; set; } = 100f;

    /// <summary>
    /// Particle lifetime in seconds
    /// </summary>
    public float Lifetime { get; set; } = 1f;

    /// <summary>
    /// Start size of particles
    /// </summary>
    public float StartSize { get; set; } = 1f;

    /// <summary>
    /// End size of particles
    /// </summary>
    public float EndSize { get; set; } = 0f;

    /// <summary>
    /// Start color
    /// </summary>
    public Vector4 StartColor { get; set; } = Vector4.One;

    /// <summary>
    /// End color
    /// </summary>
    public Vector4 EndColor { get; set; } = new Vector4(1, 1, 1, 0);

    /// <summary>
    /// Initial velocity
    /// </summary>
    public Vector3 Velocity { get; set; } = new Vector3(0, 5, 0);

    /// <summary>
    /// Velocity randomness
    /// </summary>
    public Vector3 VelocityVariation { get; set; } = new Vector3(1, 1, 1);

    /// <summary>
    /// Gravity effect
    /// </summary>
    public Vector3 Gravity { get; set; } = new Vector3(0, -9.8f, 0);

    /// <summary>
    /// Whether the system is playing
    /// </summary>
    public bool IsPlaying { get; set; } = true;

    /// <summary>
    /// Whether to loop the system
    /// </summary>
    public bool Loop { get; set; } = true;
}

/// <summary>
/// Rigidbody component for physics
/// </summary>
public class RigidbodyComponent : ECS.Component
{
    /// <summary>
    /// Mass of the object
    /// </summary>
    public float Mass { get; set; } = 1f;

    /// <summary>
    /// Linear velocity
    /// </summary>
    public Vector3 Velocity { get; set; } = Vector3.Zero;

    /// <summary>
    /// Angular velocity
    /// </summary>
    public Vector3 AngularVelocity { get; set; } = Vector3.Zero;

    /// <summary>
    /// Linear drag
    /// </summary>
    public float Drag { get; set; } = 0f;

    /// <summary>
    /// Angular drag
    /// </summary>
    public float AngularDrag { get; set; } = 0.05f;

    /// <summary>
    /// Whether gravity affects this object
    /// </summary>
    public bool UseGravity { get; set; } = true;

    /// <summary>
    /// Whether this is kinematic (not affected by forces)
    /// </summary>
    public bool IsKinematic { get; set; } = false;

    /// <summary>
    /// Apply force to the rigidbody
    /// </summary>
    public void AddForce(Vector3 force)
    {
        if (!IsKinematic)
        {
            Velocity += force / Mass;
        }
    }

    /// <summary>
    /// Apply impulse to the rigidbody
    /// </summary>
    public void AddImpulse(Vector3 impulse)
    {
        if (!IsKinematic)
        {
            Velocity += impulse / Mass;
        }
    }
}

/// <summary>
/// Collider component for collision detection
/// </summary>
public class ColliderComponent : ECS.Component
{
    /// <summary>
    /// Type of collider
    /// </summary>
    public ColliderType Type { get; set; } = ColliderType.Box;

    /// <summary>
    /// Size for box collider
    /// </summary>
    public Vector3 Size { get; set; } = Vector3.One;

    /// <summary>
    /// Radius for sphere collider
    /// </summary>
    public float Radius { get; set; } = 0.5f;

    /// <summary>
    /// Height for capsule collider
    /// </summary>
    public float Height { get; set; } = 2f;

    /// <summary>
    /// Whether this is a trigger (no physical collision)
    /// </summary>
    public bool IsTrigger { get; set; } = false;

    /// <summary>
    /// Collision layer
    /// </summary>
    public int Layer { get; set; } = 0;
}

/// <summary>
/// Types of colliders
/// </summary>
public enum ColliderType
{
    Box,
    Sphere,
    Capsule,
    Mesh
}
