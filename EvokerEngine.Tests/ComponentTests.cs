using Xunit;
using EvokerEngine.ECS;
using System.Numerics;

namespace EvokerEngine.Tests;

public class ComponentTests
{
    [Fact]
    public void TransformComponent_DefaultValues()
    {
        // Arrange & Act
        var transform = new TransformComponent();

        // Assert
        Assert.Equal(Vector3.Zero, transform.Position);
        Assert.Equal(Vector3.Zero, transform.Rotation);
        Assert.Equal(Vector3.One, transform.Scale);
    }

    [Fact]
    public void TransformComponent_CanSetValues()
    {
        // Arrange
        var transform = new TransformComponent();

        // Act
        transform.Position = new Vector3(1, 2, 3);
        transform.Rotation = new Vector3(0, 90, 0);
        transform.Scale = new Vector3(2, 2, 2);

        // Assert
        Assert.Equal(new Vector3(1, 2, 3), transform.Position);
        Assert.Equal(new Vector3(0, 90, 0), transform.Rotation);
        Assert.Equal(new Vector3(2, 2, 2), transform.Scale);
    }

    [Fact]
    public void TransformComponent_GetTransformMatrix_ReturnsMatrix()
    {
        // Arrange
        var transform = new TransformComponent
        {
            Position = new Vector3(1, 2, 3),
            Rotation = Vector3.Zero,
            Scale = Vector3.One
        };

        // Act
        var matrix = transform.GetTransformMatrix();

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, matrix);
    }

    [Fact]
    public void MeshRendererComponent_CanSetMeshAndMaterial()
    {
        // Arrange
        var renderer = new MeshRendererComponent();

        // Act
        renderer.MeshId = 42;
        renderer.MaterialId = 99;

        // Assert
        Assert.Equal(42, renderer.MeshId);
        Assert.Equal(99, renderer.MaterialId);
    }

    [Fact]
    public void CameraComponent_DefaultValues()
    {
        // Arrange & Act
        var camera = new CameraComponent();

        // Assert
        Assert.Equal(45f, camera.FieldOfView);
        Assert.Equal(0.1f, camera.NearPlane);
        Assert.Equal(100f, camera.FarPlane);
        Assert.False(camera.IsPrimary);
    }

    [Fact]
    public void CameraComponent_CanSetValues()
    {
        // Arrange
        var camera = new CameraComponent();

        // Act
        camera.FieldOfView = 60f;
        camera.NearPlane = 0.5f;
        camera.FarPlane = 200f;
        camera.IsPrimary = true;

        // Assert
        Assert.Equal(60f, camera.FieldOfView);
        Assert.Equal(0.5f, camera.NearPlane);
        Assert.Equal(200f, camera.FarPlane);
        Assert.True(camera.IsPrimary);
    }

    [Fact]
    public void CameraComponent_GetProjectionMatrix_ReturnsMatrix()
    {
        // Arrange
        var camera = new CameraComponent();

        // Act
        var matrix = camera.GetProjectionMatrix(16f / 9f);

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, matrix);
    }
}
