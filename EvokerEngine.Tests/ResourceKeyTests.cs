using Xunit;
using EvokerEngine.Core;

namespace EvokerEngine.Tests;

public class ResourceKeyTests
{
    [Fact]
    public void ResourceKey_CanCreateWithNamespaceAndKey()
    {
        // Act
        var key = new ResourceKey("mymod", "custom_item");

        // Assert
        Assert.Equal("mymod", key.Namespace);
        Assert.Equal("custom_item", key.Key);
        Assert.Equal("mymod:custom_item", key.FullKey);
    }

    [Fact]
    public void ResourceKey_CanParseFromString()
    {
        // Act
        var key = ResourceKey.Parse("minecraft:stone");

        // Assert
        Assert.Equal("minecraft", key.Namespace);
        Assert.Equal("stone", key.Key);
    }

    [Fact]
    public void ResourceKey_UsesDefaultNamespaceWhenMissing()
    {
        // Act
        var key = ResourceKey.Parse("stone");

        // Assert
        Assert.Equal("evoker", key.Namespace);
        Assert.Equal("stone", key.Key);
    }

    [Fact]
    public void ResourceKey_TryParseSucceedsForValidKey()
    {
        // Act
        var success = ResourceKey.TryParse("mymod:item", out var key);

        // Assert
        Assert.True(success);
        Assert.Equal("mymod", key.Namespace);
        Assert.Equal("item", key.Key);
    }

    [Fact]
    public void ResourceKey_IsValidChecksFormat()
    {
        // Assert
        Assert.True(ResourceKey.IsValid("mymod:item"));
        Assert.True(ResourceKey.IsValid("minecraft:stone"));
        Assert.False(ResourceKey.IsValid(""));
        Assert.False(ResourceKey.IsValid("invalid"));
    }

    [Fact]
    public void ResourceKey_EqualityWorks()
    {
        // Arrange
        var key1 = new ResourceKey("mymod", "item");
        var key2 = new ResourceKey("mymod", "item");
        var key3 = new ResourceKey("other", "item");

        // Assert
        Assert.Equal(key1, key2);
        Assert.NotEqual(key1, key3);
        Assert.True(key1 == key2);
        Assert.True(key1 != key3);
    }

    [Fact]
    public void ResourceKey_IsInNamespaceWorks()
    {
        // Arrange
        var key = new ResourceKey("mymod", "item");

        // Assert
        Assert.True(key.IsInNamespace("mymod"));
        Assert.False(key.IsInNamespace("othermod"));
    }

    [Fact]
    public void ResourceKey_ImplicitConversionFromString()
    {
        // Act
        ResourceKey key = "mymod:item";

        // Assert
        Assert.Equal("mymod", key.Namespace);
        Assert.Equal("item", key.Key);
    }

    [Fact]
    public void ResourceKey_ImplicitConversionToString()
    {
        // Arrange
        var key = new ResourceKey("mymod", "item");

        // Act
        string str = key;

        // Assert
        Assert.Equal("mymod:item", str);
    }
}
