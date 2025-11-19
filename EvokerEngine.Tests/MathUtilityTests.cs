using Xunit;
using EvokerEngine.Utilities;
using System;
using System.Numerics;

namespace EvokerEngine.Tests;

public class MathHelperTests
{
    [Fact]
    public void MathHelper_Clamp_ConstrainsValue()
    {
        // Act & Assert
        Assert.Equal(5f, MathHelper.Clamp(5f, 0f, 10f));
        Assert.Equal(0f, MathHelper.Clamp(-5f, 0f, 10f));
        Assert.Equal(10f, MathHelper.Clamp(15f, 0f, 10f));
    }

    [Fact]
    public void MathHelper_Clamp01_ConstrainsBetween0And1()
    {
        // Act & Assert
        Assert.Equal(0.5f, MathHelper.Clamp01(0.5f));
        Assert.Equal(0f, MathHelper.Clamp01(-1f));
        Assert.Equal(1f, MathHelper.Clamp01(2f));
    }

    [Fact]
    public void MathHelper_Lerp_InterpolatesCorrectly()
    {
        // Act & Assert
        Assert.Equal(5f, MathHelper.Lerp(0f, 10f, 0.5f));
        Assert.Equal(0f, MathHelper.Lerp(0f, 10f, 0f));
        Assert.Equal(10f, MathHelper.Lerp(0f, 10f, 1f));
    }

    [Fact]
    public void MathHelper_InverseLerp_CalculatesT()
    {
        // Act
        var t = MathHelper.InverseLerp(0f, 10f, 5f);

        // Assert
        Assert.Equal(0.5f, t);
    }

    [Fact]
    public void MathHelper_SmoothStep_ProducesSmoothCurve()
    {
        // Act
        var linear = MathHelper.Lerp(0f, 10f, 0.5f);
        var smooth = MathHelper.SmoothStep(0f, 10f, 0.5f);

        // Assert
        Assert.Equal(5f, linear);
        Assert.Equal(5f, smooth);  // At 0.5, should still be at midpoint
    }

    [Fact]
    public void MathHelper_MoveTowards_MovesCorrectly()
    {
        // Act
        var result1 = MathHelper.MoveTowards(0f, 10f, 5f);
        var result2 = MathHelper.MoveTowards(0f, 10f, 15f);

        // Assert
        Assert.Equal(5f, result1);
        Assert.Equal(10f, result2);  // Shouldn't overshoot
    }

    [Fact]
    public void MathHelper_Repeat_WrapsValue()
    {
        // Act & Assert
        Assert.Equal(2f, MathHelper.Repeat(12f, 10f));
        Assert.Equal(5f, MathHelper.Repeat(5f, 10f));
    }

    [Fact]
    public void MathHelper_PingPong_BouncesValue()
    {
        // Act
        var result1 = MathHelper.PingPong(2f, 4f);
        var result2 = MathHelper.PingPong(6f, 4f);

        // Assert
        Assert.Equal(2f, result1);
        Assert.Equal(2f, result2);  // Should bounce back
    }

    [Fact]
    public void MathHelper_Approximately_ChecksEquality()
    {
        // Assert
        Assert.True(MathHelper.Approximately(1.0f, 1.00001f));
        Assert.False(MathHelper.Approximately(1.0f, 1.1f));
    }
}

public class VectorHelperTests
{
    [Fact]
    public void VectorHelper_Lerp_InterpolatesVectors()
    {
        // Arrange
        var a = new Vector3(0, 0, 0);
        var b = new Vector3(10, 10, 10);

        // Act
        var result = VectorHelper.Lerp(a, b, 0.5f);

        // Assert
        Assert.Equal(new Vector3(5, 5, 5), result);
    }

    [Fact]
    public void VectorHelper_MoveTowards_MovesCorrectly()
    {
        // Arrange
        var current = new Vector3(0, 0, 0);
        var target = new Vector3(10, 0, 0);

        // Act
        var result = VectorHelper.MoveTowards(current, target, 5f);

        // Assert
        Assert.Equal(new Vector3(5, 0, 0), result);
    }

    [Fact]
    public void VectorHelper_Direction_CalculatesDirection()
    {
        // Arrange
        var from = new Vector3(0, 0, 0);
        var to = new Vector3(10, 0, 0);

        // Act
        var direction = VectorHelper.Direction(from, to);

        // Assert
        Assert.Equal(new Vector3(1, 0, 0), direction);
    }

    [Fact]
    public void VectorHelper_Project_ProjectsVectorCorrectly()
    {
        // Arrange
        var vector = new Vector3(1, 1, 0);
        var onNormal = new Vector3(1, 0, 0);

        // Act
        var projected = VectorHelper.Project(vector, onNormal);

        // Assert
        Assert.Equal(new Vector3(1, 0, 0), projected);
    }
}

public class QuaternionHelperTests
{
    [Fact]
    public void QuaternionHelper_FromEuler_CreatesQuaternion()
    {
        // Act
        var rotation = QuaternionHelper.FromEuler(new Vector3(0, 90, 0));

        // Assert
        Assert.NotEqual(Quaternion.Identity, rotation);
    }

    [Fact]
    public void QuaternionHelper_ToEuler_ConvertsBack()
    {
        // Arrange
        var original = new Vector3(30, 45, 0);  // Avoid gimbal lock angles
        var quaternion = QuaternionHelper.FromEuler(original);

        // Act
        var euler = QuaternionHelper.ToEuler(quaternion);

        // Assert - Allow larger tolerance for Euler conversion
        Assert.True(MathHelper.Approximately(original.X, euler.X, 1f));
        Assert.True(MathHelper.Approximately(original.Y, euler.Y, 1f));
        Assert.True(MathHelper.Approximately(original.Z, euler.Z, 1f));
    }

    [Fact]
    public void QuaternionHelper_Slerp_InterpolatesRotations()
    {
        // Arrange
        var a = QuaternionHelper.FromEuler(new Vector3(0, 0, 0));
        var b = QuaternionHelper.FromEuler(new Vector3(0, 180, 0));

        // Act
        var interpolated = QuaternionHelper.Slerp(a, b, 0.5f);

        // Assert
        Assert.NotEqual(a, interpolated);
        Assert.NotEqual(b, interpolated);
    }
}

public class RandomHelperTests
{
    [Fact]
    public void RandomHelper_Value_ReturnsBetween0And1()
    {
        // Act
        var value = RandomHelper.Value;

        // Assert
        Assert.InRange(value, 0f, 1f);
    }

    [Fact]
    public void RandomHelper_Range_ReturnsWithinRange()
    {
        // Act
        var value = RandomHelper.Range(-10f, 10f);

        // Assert
        Assert.InRange(value, -10f, 10f);
    }

    [Fact]
    public void RandomHelper_RangeInt_ReturnsWithinRange()
    {
        // Act
        var value = RandomHelper.Range(0, 100);

        // Assert
        Assert.InRange(value, 0, 99);  // Max is exclusive
    }

    [Fact]
    public void RandomHelper_InsideUnitCircle_ReturnsWithinCircle()
    {
        // Act
        var point = RandomHelper.InsideUnitCircle();

        // Assert
        Assert.True(point.Length() <= 1f);
    }

    [Fact]
    public void RandomHelper_InsideUnitSphere_ReturnsWithinSphere()
    {
        // Act
        var point = RandomHelper.InsideUnitSphere();

        // Assert
        Assert.True(point.Length() <= 1f);
    }

    [Fact]
    public void RandomHelper_OnUnitSphere_ReturnsOnSurface()
    {
        // Act
        var point = RandomHelper.OnUnitSphere();

        // Assert
        Assert.True(MathHelper.Approximately(point.Length(), 1f, 0.01f));
    }

    [Fact]
    public void RandomHelper_ColorRGBA_ReturnsValidColor()
    {
        // Act
        var color = RandomHelper.ColorRGBA();

        // Assert
        Assert.InRange(color.X, 0f, 1f);
        Assert.InRange(color.Y, 0f, 1f);
        Assert.InRange(color.Z, 0f, 1f);
        Assert.Equal(1f, color.W);  // Alpha should be 1
    }
}

public class TransformHelperTests
{
    [Fact]
    public void TransformHelper_LookAt_CreatesLookAtMatrix()
    {
        // Arrange
        var position = new Vector3(0, 0, 10);
        var target = new Vector3(0, 0, 0);
        var up = Vector3.UnitY;

        // Act
        var matrix = TransformHelper.LookAt(position, target, up);

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, matrix);
    }

    [Fact]
    public void TransformHelper_CreateTRS_CreatesTransformMatrix()
    {
        // Arrange
        var translation = new Vector3(1, 2, 3);
        var rotation = Quaternion.Identity;
        var scale = Vector3.One;

        // Act
        var matrix = TransformHelper.CreateTRS(translation, rotation, scale);

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, matrix);
    }

    [Fact]
    public void TransformHelper_Decompose_ExtractsComponents()
    {
        // Arrange
        var originalTranslation = new Vector3(1, 2, 3);
        var originalRotation = Quaternion.Identity;
        var originalScale = new Vector3(2, 2, 2);
        var matrix = TransformHelper.CreateTRS(originalTranslation, originalRotation, originalScale);

        // Act
        var success = TransformHelper.Decompose(matrix, out var translation, out var rotation, out var scale);

        // Assert
        Assert.True(success);
        Assert.Equal(originalTranslation, translation);
        Assert.Equal(originalScale, scale);
    }
}
