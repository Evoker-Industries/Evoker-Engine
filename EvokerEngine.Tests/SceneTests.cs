using Xunit;
using EvokerEngine.Scene;
using System.Numerics;
using System.Linq;

namespace EvokerEngine.Tests;

public class SceneTests
{
    [Fact]
    public void Scene_HasName()
    {
        // Arrange & Act
        var scene = new Scene.Scene("Test Scene");

        // Assert
        Assert.Equal("Test Scene", scene.Name);
    }

    [Fact]
    public void Scene_CreateEntity_AddsEntity()
    {
        // Arrange
        var scene = new Scene.Scene();

        // Act
        var entity = scene.CreateEntity("Test Entity");

        // Assert
        Assert.Contains(entity, scene.GetRootEntities());
    }

    [Fact]
    public void Scene_DestroyEntity_RemovesEntity()
    {
        // Arrange
        var scene = new Scene.Scene();
        var entity = scene.CreateEntity();

        // Act
        scene.DestroyEntity(entity);

        // Assert
        Assert.DoesNotContain(entity, scene.GetRootEntities());
    }

    [Fact]
    public void Scene_Registry_IsAccessible()
    {
        // Arrange
        var scene = new Scene.Scene();

        // Assert
        Assert.NotNull(scene.Registry);
    }

    [Fact]
    public void Scene_Update_DoesNotThrow()
    {
        // Arrange
        var scene = new Scene.Scene();
        var entity = scene.CreateEntity();

        // Act & Assert
        scene.Update(0.016f); // 60 FPS frame time
        Assert.True(true); // If we got here, no exception was thrown
    }
}

public class CameraTests
{
    [Fact]
    public void Camera_DefaultPosition()
    {
        // Arrange & Act
        var camera = new Camera();

        // Assert
        Assert.Equal(new Vector3(0, 0, 5), camera.Position);
        Assert.Equal(Vector3.Zero, camera.Target);
        Assert.Equal(Vector3.UnitY, camera.Up);
    }

    [Fact]
    public void Camera_CanSetProperties()
    {
        // Arrange
        var camera = new Camera();

        // Act
        camera.Position = new Vector3(10, 10, 10);
        camera.Target = new Vector3(5, 5, 5);
        camera.FieldOfView = 60f;
        camera.AspectRatio = 16f / 9f;
        camera.NearPlane = 0.5f;
        camera.FarPlane = 200f;

        // Assert
        Assert.Equal(new Vector3(10, 10, 10), camera.Position);
        Assert.Equal(new Vector3(5, 5, 5), camera.Target);
        Assert.Equal(60f, camera.FieldOfView);
        Assert.Equal(16f / 9f, camera.AspectRatio);
        Assert.Equal(0.5f, camera.NearPlane);
        Assert.Equal(200f, camera.FarPlane);
    }

    [Fact]
    public void Camera_GetViewMatrix_ReturnsMatrix()
    {
        // Arrange
        var camera = new Camera();

        // Act
        var viewMatrix = camera.GetViewMatrix();

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, viewMatrix);
    }

    [Fact]
    public void Camera_GetProjectionMatrix_ReturnsMatrix()
    {
        // Arrange
        var camera = new Camera();

        // Act
        var projectionMatrix = camera.GetProjectionMatrix();

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, projectionMatrix);
    }

    [Fact]
    public void Camera_LookAt_UpdatesTarget()
    {
        // Arrange
        var camera = new Camera();
        var newTarget = new Vector3(10, 5, 0);

        // Act
        camera.LookAt(newTarget);

        // Assert
        Assert.Equal(newTarget, camera.Target);
    }

    [Fact]
    public void Camera_MoveForward_ChangesPosition()
    {
        // Arrange
        var camera = new Camera
        {
            Position = new Vector3(0, 0, 5),
            Target = Vector3.Zero
        };
        var originalPosition = camera.Position;

        // Act
        camera.MoveForward(1.0f);

        // Assert
        Assert.NotEqual(originalPosition, camera.Position);
    }

    [Fact]
    public void Camera_MoveRight_ChangesPosition()
    {
        // Arrange
        var camera = new Camera
        {
            Position = new Vector3(0, 0, 5),
            Target = Vector3.Zero
        };
        var originalPosition = camera.Position;

        // Act
        camera.MoveRight(1.0f);

        // Assert
        Assert.NotEqual(originalPosition, camera.Position);
    }

    [Fact]
    public void Camera_MoveUp_ChangesPosition()
    {
        // Arrange
        var camera = new Camera
        {
            Position = new Vector3(0, 0, 5),
            Target = Vector3.Zero
        };
        var originalPosition = camera.Position;

        // Act
        camera.MoveUp(1.0f);

        // Assert
        Assert.NotEqual(originalPosition, camera.Position);
    }
}
