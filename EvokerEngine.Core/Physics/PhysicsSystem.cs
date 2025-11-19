using System;
using System.Numerics;
using System.Collections.Generic;
using EvokerEngine.ECS;

namespace EvokerEngine.Physics;

/// <summary>
/// Rigidbody component for physics simulation
/// </summary>
public class RigidbodyComponent : Component
{
    public Vector3 Velocity { get; set; }
    public Vector3 AngularVelocity { get; set; }
    public float Mass { get; set; } = 1.0f;
    public float Drag { get; set; } = 0.0f;
    public float AngularDrag { get; set; } = 0.05f;
    public bool UseGravity { get; set; } = true;
    public bool IsKinematic { get; set; }
    internal Vector3 AccumulatedForce { get; set; }
    internal Vector3 AccumulatedTorque { get; set; }

    /// <summary>
    /// Apply force to the rigidbody
    /// </summary>
    public void AddForce(Vector3 force)
    {
        if (IsKinematic) return;
        AccumulatedForce += force;
    }

    /// <summary>
    /// Apply impulse (instant velocity change)
    /// </summary>
    public void AddImpulse(Vector3 impulse)
    {
        if (IsKinematic || Mass <= 0) return;
        Velocity += impulse / Mass;
    }

    /// <summary>
    /// Apply torque for rotation
    /// </summary>
    public void AddTorque(Vector3 torque)
    {
        if (IsKinematic) return;
        AccumulatedTorque += torque;
    }

    internal void ClearForces()
    {
        AccumulatedForce = Vector3.Zero;
        AccumulatedTorque = Vector3.Zero;
    }
}

/// <summary>
/// Collider base class
/// </summary>
public abstract class ColliderComponent : Component
{
    public Vector3 Center { get; set; }
    public bool IsTrigger { get; set; }
    public PhysicsMaterial? Material { get; set; }

    public abstract bool Intersects(ColliderComponent other, out Vector3 normal, out float penetration);
    public abstract Vector3 GetWorldCenter(Vector3 entityPosition);
}

/// <summary>
/// Box collider
/// </summary>
public class BoxColliderComponent : ColliderComponent
{
    public Vector3 Size { get; set; } = Vector3.One;

    public override Vector3 GetWorldCenter(Vector3 entityPosition)
    {
        return entityPosition + Center;
    }

    public override bool Intersects(ColliderComponent other, out Vector3 normal, out float penetration)
    {
        normal = Vector3.Zero;
        penetration = 0;

        if (other is BoxColliderComponent box)
        {
            return IntersectsBox(box, out normal, out penetration);
        }
        else if (other is SphereColliderComponent sphere)
        {
            return IntersectsSphere(sphere, out normal, out penetration);
        }

        return false;
    }

    private bool IntersectsBox(BoxColliderComponent other, out Vector3 normal, out float penetration)
    {
        // Get transform positions
        var thisTransform = Scene.Scene.GetActiveScene()?.Registry.GetComponent<TransformComponent>(Entity);
        var otherTransform = Scene.Scene.GetActiveScene()?.Registry.GetComponent<TransformComponent>(other.Entity);

        if (thisTransform == null || otherTransform == null)
        {
            normal = Vector3.Zero;
            penetration = 0;
            return false;
        }

        Vector3 thisMin = thisTransform.Position + Center - Size / 2;
        Vector3 thisMax = thisTransform.Position + Center + Size / 2;
        Vector3 otherMin = otherTransform.Position + other.Center - other.Size / 2;
        Vector3 otherMax = otherTransform.Position + other.Center + other.Size / 2;

        // AABB collision detection
        bool intersects = thisMin.X <= otherMax.X && thisMax.X >= otherMin.X &&
                         thisMin.Y <= otherMax.Y && thisMax.Y >= otherMin.Y &&
                         thisMin.Z <= otherMax.Z && thisMax.Z >= otherMin.Z;

        if (intersects)
        {
            // Calculate penetration and normal
            Vector3 overlapX = new Vector3(Math.Min(thisMax.X - otherMin.X, otherMax.X - thisMin.X), 0, 0);
            Vector3 overlapY = new Vector3(0, Math.Min(thisMax.Y - otherMin.Y, otherMax.Y - thisMin.Y), 0);
            Vector3 overlapZ = new Vector3(0, 0, Math.Min(thisMax.Z - otherMin.Z, otherMax.Z - thisMin.Z));

            // Find minimum overlap axis
            if (overlapX.X < overlapY.Y && overlapX.X < overlapZ.Z)
            {
                penetration = overlapX.X;
                normal = new Vector3(thisTransform.Position.X < otherTransform.Position.X ? -1 : 1, 0, 0);
            }
            else if (overlapY.Y < overlapZ.Z)
            {
                penetration = overlapY.Y;
                normal = new Vector3(0, thisTransform.Position.Y < otherTransform.Position.Y ? -1 : 1, 0);
            }
            else
            {
                penetration = overlapZ.Z;
                normal = new Vector3(0, 0, thisTransform.Position.Z < otherTransform.Position.Z ? -1 : 1);
            }
        }
        else
        {
            normal = Vector3.Zero;
            penetration = 0;
        }

        return intersects;
    }

    private bool IntersectsSphere(SphereColliderComponent sphere, out Vector3 normal, out float penetration)
    {
        // Box-sphere collision
        var thisTransform = Scene.Scene.GetActiveScene()?.Registry.GetComponent<TransformComponent>(Entity);
        var otherTransform = Scene.Scene.GetActiveScene()?.Registry.GetComponent<TransformComponent>(sphere.Entity);

        if (thisTransform == null || otherTransform == null)
        {
            normal = Vector3.Zero;
            penetration = 0;
            return false;
        }

        Vector3 boxCenter = thisTransform.Position + Center;
        Vector3 sphereCenter = otherTransform.Position + sphere.Center;

        // Find closest point on box to sphere center
        Vector3 halfSize = Size / 2;
        Vector3 closest = new Vector3(
            Math.Clamp(sphereCenter.X, boxCenter.X - halfSize.X, boxCenter.X + halfSize.X),
            Math.Clamp(sphereCenter.Y, boxCenter.Y - halfSize.Y, boxCenter.Y + halfSize.Y),
            Math.Clamp(sphereCenter.Z, boxCenter.Z - halfSize.Z, boxCenter.Z + halfSize.Z)
        );

        Vector3 diff = sphereCenter - closest;
        float distance = diff.Length();

        if (distance < sphere.Radius)
        {
            penetration = sphere.Radius - distance;
            normal = distance > 0 ? Vector3.Normalize(diff) : Vector3.UnitY;
            return true;
        }

        normal = Vector3.Zero;
        penetration = 0;
        return false;
    }
}

/// <summary>
/// Sphere collider
/// </summary>
public class SphereColliderComponent : ColliderComponent
{
    public float Radius { get; set; } = 0.5f;

    public override Vector3 GetWorldCenter(Vector3 entityPosition)
    {
        return entityPosition + Center;
    }

    public override bool Intersects(ColliderComponent other, out Vector3 normal, out float penetration)
    {
        normal = Vector3.Zero;
        penetration = 0;

        if (other is SphereColliderComponent sphere)
        {
            return IntersectsSphere(sphere, out normal, out penetration);
        }
        else if (other is BoxColliderComponent box)
        {
            bool result = box.Intersects(this, out normal, out penetration);
            normal = -normal; // Flip normal
            return result;
        }

        return false;
    }

    private bool IntersectsSphere(SphereColliderComponent other, out Vector3 normal, out float penetration)
    {
        var thisTransform = Scene.Scene.GetActiveScene()?.Registry.GetComponent<TransformComponent>(Entity);
        var otherTransform = Scene.Scene.GetActiveScene()?.Registry.GetComponent<TransformComponent>(other.Entity);

        if (thisTransform == null || otherTransform == null)
        {
            normal = Vector3.Zero;
            penetration = 0;
            return false;
        }

        Vector3 thisCenter = thisTransform.Position + Center;
        Vector3 otherCenter = otherTransform.Position + other.Center;
        Vector3 diff = otherCenter - thisCenter;
        float distance = diff.Length();
        float combinedRadius = Radius + other.Radius;

        if (distance < combinedRadius)
        {
            penetration = combinedRadius - distance;
            normal = distance > 0 ? Vector3.Normalize(diff) : Vector3.UnitY;
            return true;
        }

        normal = Vector3.Zero;
        penetration = 0;
        return false;
    }
}

/// <summary>
/// Capsule collider
/// </summary>
public class CapsuleColliderComponent : ColliderComponent
{
    public float Radius { get; set; } = 0.5f;
    public float Height { get; set; } = 2.0f;

    public override Vector3 GetWorldCenter(Vector3 entityPosition)
    {
        return entityPosition + Center;
    }

    public override bool Intersects(ColliderComponent other, out Vector3 normal, out float penetration)
    {
        // Simplified capsule collision - treat as sphere for now
        normal = Vector3.Zero;
        penetration = 0;
        Core.Logger.Warning("CapsuleCollider.Intersects using simplified sphere approximation");
        return false;
    }
}

/// <summary>
/// Physics material for surface properties
/// </summary>
public class PhysicsMaterial
{
    public float StaticFriction { get; set; } = 0.6f;
    public float DynamicFriction { get; set; } = 0.4f;
    public float Bounciness { get; set; } = 0.0f;
}

/// <summary>
/// Raycast hit information
/// </summary>
public struct RaycastHit
{
    public Vector3 Point { get; set; }
    public Vector3 Normal { get; set; }
    public float Distance { get; set; }
    public ColliderComponent? Collider { get; set; }
    public Entity Entity { get; set; }
}

/// <summary>
/// Physics system manager
/// </summary>
public class PhysicsSystem
{
    private static PhysicsSystem? _instance;
    public static PhysicsSystem Instance => _instance ??= new PhysicsSystem();

    public Vector3 Gravity { get; set; } = new Vector3(0, -9.81f, 0);
    private readonly List<Entity> _rigidbodies = new();
    private readonly List<Entity> _colliders = new();
    public bool IsInitialized { get; private set; }

    private PhysicsSystem() { }

    /// <summary>
    /// Initialize the physics system
    /// </summary>
    public void Initialize()
    {
        IsInitialized = true;
        Core.Logger.Info("Physics system initialized");
    }

    /// <summary>
    /// Register a rigidbody for physics simulation
    /// </summary>
    public void RegisterRigidbody(Entity entity)
    {
        if (!_rigidbodies.Contains(entity))
        {
            _rigidbodies.Add(entity);
        }
    }

    /// <summary>
    /// Register a collider for collision detection
    /// </summary>
    public void RegisterCollider(Entity entity)
    {
        if (!_colliders.Contains(entity))
        {
            _colliders.Add(entity);
        }
    }

    /// <summary>
    /// Update physics simulation
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!IsInitialized) return;

        var scene = Scene.Scene.GetActiveScene();
        if (scene == null) return;

        // Update rigidbodies
        foreach (var entity in _rigidbodies)
        {
            var rigidbody = scene.Registry.GetComponent<RigidbodyComponent>(entity);
            var transform = scene.Registry.GetComponent<TransformComponent>(entity);

            if (rigidbody == null || transform == null || rigidbody.IsKinematic) continue;

            // Apply gravity
            if (rigidbody.UseGravity)
            {
                rigidbody.AddForce(Gravity * rigidbody.Mass);
            }

            // Apply accumulated forces
            if (rigidbody.Mass > 0)
            {
                Vector3 acceleration = rigidbody.AccumulatedForce / rigidbody.Mass;
                rigidbody.Velocity += acceleration * deltaTime;
            }

            // Apply drag
            rigidbody.Velocity *= (1.0f - rigidbody.Drag * deltaTime);
            rigidbody.AngularVelocity *= (1.0f - rigidbody.AngularDrag * deltaTime);

            // Update position
            transform.Position += rigidbody.Velocity * deltaTime;

            // Update rotation (simplified)
            transform.Rotation += rigidbody.AngularVelocity * deltaTime;

            // Clear forces
            rigidbody.ClearForces();
        }

        // Check collisions
        DetectCollisions();
    }

    private void DetectCollisions()
    {
        var scene = Scene.Scene.GetActiveScene();
        if (scene == null) return;

        for (int i = 0; i < _colliders.Count; i++)
        {
            for (int j = i + 1; j < _colliders.Count; j++)
            {
                var entityA = _colliders[i];
                var entityB = _colliders[j];

                var colliderA = scene.Registry.GetComponent<ColliderComponent>(entityA);
                var colliderB = scene.Registry.GetComponent<ColliderComponent>(entityB);

                if (colliderA == null || colliderB == null) continue;

                if (colliderA.Intersects(colliderB, out Vector3 normal, out float penetration))
                {
                    if (!colliderA.IsTrigger && !colliderB.IsTrigger)
                    {
                        ResolveCollision(entityA, entityB, normal, penetration);
                    }
                }
            }
        }
    }

    private void ResolveCollision(Entity entityA, Entity entityB, Vector3 normal, float penetration)
    {
        var scene = Scene.Scene.GetActiveScene();
        if (scene == null) return;

        var rigidbodyA = scene.Registry.GetComponent<RigidbodyComponent>(entityA);
        var rigidbodyB = scene.Registry.GetComponent<RigidbodyComponent>(entityB);
        var transformA = scene.Registry.GetComponent<TransformComponent>(entityA);
        var transformB = scene.Registry.GetComponent<TransformComponent>(entityB);

        if (transformA == null || transformB == null) return;

        // Separate objects
        if (rigidbodyA != null && !rigidbodyA.IsKinematic && rigidbodyB != null && !rigidbodyB.IsKinematic)
        {
            // Both dynamic - separate equally
            transformA.Position -= normal * (penetration / 2);
            transformB.Position += normal * (penetration / 2);

            // Simple elastic collision response
            Vector3 relativeVelocity = rigidbodyB.Velocity - rigidbodyA.Velocity;
            float velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);

            if (velocityAlongNormal < 0) return; // Objects moving apart

            float restitution = 0.5f; // Bounciness
            float impulseScalar = -(1 + restitution) * velocityAlongNormal;
            impulseScalar /= (1 / rigidbodyA.Mass + 1 / rigidbodyB.Mass);

            Vector3 impulse = impulseScalar * normal;
            rigidbodyA.Velocity -= impulse / rigidbodyA.Mass;
            rigidbodyB.Velocity += impulse / rigidbodyB.Mass;
        }
        else if (rigidbodyA != null && !rigidbodyA.IsKinematic)
        {
            // Only A is dynamic
            transformA.Position -= normal * penetration;
            rigidbodyA.Velocity -= Vector3.Dot(rigidbodyA.Velocity, normal) * normal * 1.5f; // Bounce off
        }
        else if (rigidbodyB != null && !rigidbodyB.IsKinematic)
        {
            // Only B is dynamic
            transformB.Position += normal * penetration;
            rigidbodyB.Velocity -= Vector3.Dot(rigidbodyB.Velocity, -normal) * (-normal) * 1.5f; // Bounce off
        }
    }

    /// <summary>
    /// Perform a raycast
    /// </summary>
    public bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = float.MaxValue)
    {
        hit = default;
        var scene = Scene.Scene.GetActiveScene();
        if (scene == null) return false;

        direction = Vector3.Normalize(direction);
        float closestDistance = maxDistance;
        bool foundHit = false;

        foreach (var entity in _colliders)
        {
            var collider = scene.Registry.GetComponent<ColliderComponent>(entity);
            var transform = scene.Registry.GetComponent<TransformComponent>(entity);

            if (collider == null || transform == null) continue;

            // Simple sphere ray intersection for all colliders (simplified)
            Vector3 center = collider.GetWorldCenter(transform.Position);
            float radius = collider is SphereColliderComponent sphere ? sphere.Radius : 
                          collider is BoxColliderComponent box ? box.Size.Length() / 2 : 1.0f;

            Vector3 oc = origin - center;
            float a = Vector3.Dot(direction, direction);
            float b = 2.0f * Vector3.Dot(oc, direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - 4 * a * c;

            if (discriminant >= 0)
            {
                float t = (-b - MathF.Sqrt(discriminant)) / (2.0f * a);
                if (t > 0 && t < closestDistance)
                {
                    closestDistance = t;
                    Vector3 hitPoint = origin + direction * t;
                    hit = new RaycastHit
                    {
                        Point = hitPoint,
                        Normal = Vector3.Normalize(hitPoint - center),
                        Distance = t,
                        Collider = collider,
                        Entity = entity
                    };
                    foundHit = true;
                }
            }
        }

        return foundHit;
    }

    /// <summary>
    /// Check if a sphere overlaps with any colliders
    /// </summary>
    public bool CheckSphere(Vector3 position, float radius)
    {
        var scene = Scene.Scene.GetActiveScene();
        if (scene == null) return false;

        foreach (var entity in _colliders)
        {
            var collider = scene.Registry.GetComponent<ColliderComponent>(entity);
            var transform = scene.Registry.GetComponent<TransformComponent>(entity);

            if (collider == null || transform == null) continue;

            Vector3 center = collider.GetWorldCenter(transform.Position);
            float distance = Vector3.Distance(position, center);
            float colliderRadius = collider is SphereColliderComponent sphere ? sphere.Radius : 1.0f;

            if (distance < radius + colliderRadius)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Shutdown the physics system
    /// </summary>
    public void Shutdown()
    {
        _rigidbodies.Clear();
        _colliders.Clear();
        IsInitialized = false;
        Core.Logger.Info("Physics system shutdown");
    }
}

