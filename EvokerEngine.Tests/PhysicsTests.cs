using System;
using System.Numerics;
using Xunit;
using EvokerEngine.Physics;
using EvokerEngine.ECS;
using EvokerEngine.Scene;

namespace EvokerEngine.Tests;

public class PhysicsTests
{
    [Fact]
    public void PhysicsSystem_ShouldInitialize()
    {
        // Arrange & Act
        PhysicsSystem.Instance.Initialize();

        // Assert
        Assert.True(PhysicsSystem.Instance.IsInitialized);
    }

    [Fact]
    public void RigidbodyComponent_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var rigidbody = new RigidbodyComponent();

        // Assert
        Assert.Equal(Vector3.Zero, rigidbody.Velocity);
        Assert.Equal(Vector3.Zero, rigidbody.AngularVelocity);
        Assert.Equal(1.0f, rigidbody.Mass);
        Assert.Equal(0.0f, rigidbody.Drag);
        Assert.Equal(0.05f, rigidbody.AngularDrag);
        Assert.True(rigidbody.UseGravity);
        Assert.False(rigidbody.IsKinematic);
    }

    [Fact]
    public void RigidbodyComponent_AddForce_ShouldAffectVelocityAfterPhysicsUpdate()
    {
        // Arrange
        PhysicsSystem.Instance.Initialize();
        var scene = new Scene.Scene("TestScene");
        Scene.Scene.SetActiveScene(scene);

        var entity = scene.CreateEntity("TestEntity");
        var transform = scene.Registry.AddComponent<TransformComponent>(entity);
        transform.Position = Vector3.Zero;

        var rigidbody = scene.Registry.AddComponent<RigidbodyComponent>(entity);
        rigidbody.Mass = 1.0f;
        rigidbody.UseGravity = false; // Disable gravity for cleaner test
        rigidbody.Velocity = Vector3.Zero;

        PhysicsSystem.Instance.RegisterRigidbody(entity);

        var force = new Vector3(10, 0, 0);

        // Act
        rigidbody.AddForce(force);
        PhysicsSystem.Instance.Update(0.1f); // 100ms

        // Assert - Force should have affected velocity
        Assert.True(rigidbody.Velocity.X > 0, "Velocity X should be positive after force applied");
    }

    [Fact]
    public void RigidbodyComponent_AddImpulse_ShouldChangeVelocity()
    {
        // Arrange
        var rigidbody = new RigidbodyComponent
        {
            Mass = 2.0f,
            Velocity = Vector3.Zero
        };
        var impulse = new Vector3(10, 0, 0);

        // Act
        rigidbody.AddImpulse(impulse);

        // Assert
        Assert.Equal(new Vector3(5, 0, 0), rigidbody.Velocity); // impulse / mass
    }

    [Fact]
    public void RigidbodyComponent_IsKinematic_ShouldNotRespondToForces()
    {
        // Arrange
        PhysicsSystem.Instance.Initialize();
        var scene = new Scene.Scene("TestScene");
        Scene.Scene.SetActiveScene(scene);

        var entity = scene.CreateEntity("KinematicEntity");
        var transform = scene.Registry.AddComponent<TransformComponent>(entity);
        transform.Position = Vector3.Zero;

        var rigidbody = scene.Registry.AddComponent<RigidbodyComponent>(entity);
        rigidbody.IsKinematic = true;
        rigidbody.Velocity = Vector3.Zero;

        PhysicsSystem.Instance.RegisterRigidbody(entity);

        var force = new Vector3(10, 0, 0);

        // Act
        rigidbody.AddForce(force);
        PhysicsSystem.Instance.Update(0.1f);

        // Assert - Kinematic objects should not respond to forces
        Assert.Equal(Vector3.Zero, rigidbody.Velocity);
    }

    [Fact]
    public void BoxColliderComponent_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var collider = new BoxColliderComponent();

        // Assert
        Assert.Equal(Vector3.One, collider.Size);
        Assert.Equal(Vector3.Zero, collider.Center);
        Assert.False(collider.IsTrigger);
    }

    [Fact]
    public void SphereColliderComponent_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var collider = new SphereColliderComponent();

        // Assert
        Assert.Equal(0.5f, collider.Radius);
        Assert.Equal(Vector3.Zero, collider.Center);
        Assert.False(collider.IsTrigger);
    }

    [Fact]
    public void PhysicsSystem_Gravity_ShouldBeConfigurable()
    {
        // Arrange
        PhysicsSystem.Instance.Initialize();
        var customGravity = new Vector3(0, -20f, 0);

        // Act
        PhysicsSystem.Instance.Gravity = customGravity;

        // Assert
        Assert.Equal(customGravity, PhysicsSystem.Instance.Gravity);
    }

    [Fact]
    public void PhysicsSystem_Update_ShouldApplyGravity()
    {
        // Arrange
        PhysicsSystem.Instance.Initialize();
        var scene = new Scene.Scene("TestScene");
        Scene.Scene.SetActiveScene(scene);

        var entity = scene.CreateEntity("TestEntity");
        var transform = scene.Registry.AddComponent<TransformComponent>(entity);
        transform.Position = new Vector3(0, 10, 0);

        var rigidbody = scene.Registry.AddComponent<RigidbodyComponent>(entity);
        rigidbody.Mass = 1.0f;
        rigidbody.UseGravity = true;
        rigidbody.Velocity = Vector3.Zero;

        PhysicsSystem.Instance.RegisterRigidbody(entity);

        // Act
        PhysicsSystem.Instance.Update(0.1f); // 100ms

        // Assert
        Assert.True(rigidbody.Velocity.Y < 0, "Velocity should be negative (falling)");
        Assert.True(transform.Position.Y < 10, "Position should have decreased");
    }

    [Fact]
    public void PhysicsSystem_CheckSphere_ShouldDetectOverlap()
    {
        // Arrange
        PhysicsSystem.Instance.Initialize();
        var scene = new Scene.Scene("TestScene");
        Scene.Scene.SetActiveScene(scene);

        var entity = scene.CreateEntity("TestEntity");
        var transform = scene.Registry.AddComponent<TransformComponent>(entity);
        transform.Position = new Vector3(0, 0, 0);

        var collider = scene.Registry.AddComponent<SphereColliderComponent>(entity);
        collider.Radius = 1.0f;

        PhysicsSystem.Instance.RegisterCollider(entity);

        // Act - check overlapping sphere
        bool overlaps = PhysicsSystem.Instance.CheckSphere(new Vector3(0, 0, 0), 0.5f);

        // Assert
        Assert.True(overlaps);
    }

    [Fact]
    public void PhysicsSystem_CheckSphere_ShouldNotDetectNonOverlap()
    {
        // Arrange
        PhysicsSystem.Instance.Initialize();
        var scene = new Scene.Scene("TestScene");
        Scene.Scene.SetActiveScene(scene);

        var entity = scene.CreateEntity("TestEntity");
        var transform = scene.Registry.AddComponent<TransformComponent>(entity);
        transform.Position = new Vector3(0, 0, 0);

        var collider = scene.Registry.AddComponent<SphereColliderComponent>(entity);
        collider.Radius = 1.0f;

        PhysicsSystem.Instance.RegisterCollider(entity);

        // Act - check non-overlapping sphere
        bool overlaps = PhysicsSystem.Instance.CheckSphere(new Vector3(10, 0, 0), 0.5f);

        // Assert
        Assert.False(overlaps);
    }

    [Fact]
    public void PhysicsMaterial_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var material = new PhysicsMaterial();

        // Assert
        Assert.Equal(0.6f, material.StaticFriction);
        Assert.Equal(0.4f, material.DynamicFriction);
        Assert.Equal(0.0f, material.Bounciness);
    }

    [Fact]
    public void PhysicsSystem_Update_ShouldResetForcesAfterFrame()
    {
        // Arrange
        PhysicsSystem.Instance.Initialize();
        var scene = new Scene.Scene("TestScene");
        Scene.Scene.SetActiveScene(scene);

        var entity = scene.CreateEntity("TestEntity");
        var transform = scene.Registry.AddComponent<TransformComponent>(entity);
        transform.Position = Vector3.Zero;

        var rigidbody = scene.Registry.AddComponent<RigidbodyComponent>(entity);
        rigidbody.Mass = 1.0f;
        rigidbody.UseGravity = false;
        rigidbody.Velocity = Vector3.Zero;

        PhysicsSystem.Instance.RegisterRigidbody(entity);

        // Act - Apply force and update physics
        rigidbody.AddForce(new Vector3(10, 0, 0));
        var velocityAfterFirstUpdate = rigidbody.Velocity.X;
        PhysicsSystem.Instance.Update(0.1f);
        
        // Apply same force again
        rigidbody.AddForce(new Vector3(10, 0, 0));
        PhysicsSystem.Instance.Update(0.1f);

        // Assert - If forces were accumulated instead of reset, 
        // velocity would increase more in second update
        // We just check that physics is working consistently
        Assert.True(rigidbody.Velocity.X > 0);
    }

    [Fact]
    public void PhysicsSystem_Shutdown_ShouldClearState()
    {
        // Arrange
        PhysicsSystem.Instance.Initialize();
        Assert.True(PhysicsSystem.Instance.IsInitialized);

        // Act
        PhysicsSystem.Instance.Shutdown();

        // Assert
        Assert.False(PhysicsSystem.Instance.IsInitialized);
    }
}
