using Xunit;
using EvokerEngine.World;
using EvokerEngine.Core;
using System.Numerics;

namespace EvokerEngine.Tests;

public class DimensionTests
{
    [Fact]
    public void Dimension_CanCreateSimpleDimension()
    {
        // Act
        var dimension = new SimpleDimension("test", "custom", "Custom Dimension");

        // Assert
        Assert.Equal("test:custom", dimension.Id.FullKey);
        Assert.Equal("Custom Dimension", dimension.Name);
    }

    [Fact]
    public void Dimension_HasCorrectDefaultProperties()
    {
        // Arrange & Act
        var dimension = new SimpleDimension("test", "world", "Test World");

        // Assert
        Assert.Equal(DimensionType.Custom, dimension.Type);
        Assert.Equal(15, dimension.AmbientLight);
        Assert.True(dimension.HasSky);
        Assert.False(dimension.HasCeiling);
        Assert.Equal(1.0f, dimension.CoordinateScale);
        Assert.True(dimension.BedsWork);
    }

    [Fact]
    public void Dimension_CanSetProperties()
    {
        // Arrange
        var dimension = new SimpleDimension("test", "nether", "Test Nether");

        // Act
        dimension.Type = DimensionType.Nether;
        dimension.HasCeiling = true;
        dimension.CoordinateScale = 8.0f;
        dimension.WaterEvaporates = true;

        // Assert
        Assert.Equal(DimensionType.Nether, dimension.Type);
        Assert.True(dimension.HasCeiling);
        Assert.Equal(8.0f, dimension.CoordinateScale);
        Assert.True(dimension.WaterEvaporates);
    }

    [Fact]
    public void Dimension_ConvertsCoordinatesBetweenDimensions()
    {
        // Arrange
        var overworld = new SimpleDimension("test", "overworld", "Overworld");
        overworld.CoordinateScale = 1.0f;

        var nether = new SimpleDimension("test", "nether", "Nether");
        nether.CoordinateScale = 8.0f;

        var overworldPos = new Vector3(800, 64, 800);

        // Act
        var netherPos = overworld.ConvertCoordinates(overworldPos, nether);

        // Assert
        Assert.Equal(100f, netherPos.X);
        Assert.Equal(8f, netherPos.Y);
        Assert.Equal(100f, netherPos.Z);
    }

    [Fact]
    public void Dimension_ChecksHeightLimits()
    {
        // Arrange
        var dimension = new SimpleDimension("test", "world", "World");
        dimension.MinHeight = -64;
        dimension.MaxHeight = 320;

        // Assert
        Assert.True(dimension.IsWithinHeightLimits(0));
        Assert.True(dimension.IsWithinHeightLimits(-64));
        Assert.True(dimension.IsWithinHeightLimits(320));
        Assert.False(dimension.IsWithinHeightLimits(-65));
        Assert.False(dimension.IsWithinHeightLimits(321));
    }

    [Fact]
    public void Dimension_CalculatesHeightCorrectly()
    {
        // Arrange
        var dimension = new SimpleDimension("test", "world", "World");
        dimension.MinHeight = -64;
        dimension.MaxHeight = 320;

        // Act
        var height = dimension.Height;

        // Assert
        Assert.Equal(384, height);
    }

    [Fact]
    public void DimensionRegistry_RegistersDimensions()
    {
        // Arrange
        var registry = DimensionRegistry.Instance;
        var dimension = new SimpleDimension("testdim", "custom", "Custom");

        // Act
        registry.Register(dimension);

        // Assert
        Assert.True(registry.IsRegistered(dimension.Id));
        Assert.Equal(dimension, registry.Get(dimension.Id));
    }

    [Fact]
    public void DimensionRegistry_HasDefaultDimensions()
    {
        // Arrange
        var registry = DimensionRegistry.Instance;

        // Assert
        Assert.True(registry.IsRegistered("evoker:overworld"));
        Assert.True(registry.IsRegistered("evoker:nether"));
        Assert.True(registry.IsRegistered("evoker:end"));
    }

    [Fact]
    public void DimensionRegistry_CanGetDimensionsByType()
    {
        // Arrange
        var registry = DimensionRegistry.Instance;

        // Act
        var overworldDimensions = registry.GetDimensionsByType(DimensionType.Overworld);
        var netherDimensions = registry.GetDimensionsByType(DimensionType.Nether);

        // Assert
        Assert.NotEmpty(overworldDimensions);
        Assert.NotEmpty(netherDimensions);
    }

    [Fact]
    public void DimensionRegistry_HasDefaultDimension()
    {
        // Arrange
        var registry = DimensionRegistry.Instance;

        // Assert
        Assert.NotNull(registry.DefaultDimension);
        Assert.Equal("evoker:overworld", registry.DefaultDimension.Id.FullKey);
    }
}
