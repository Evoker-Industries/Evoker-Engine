using Xunit;
using EvokerEngine.Blocks;
using EvokerEngine.Core;

namespace EvokerEngine.Tests;

public class BlockTests
{
    [Fact]
    public void Block_CanCreateSimpleBlock()
    {
        // Act
        var block = new SimpleBlock("test", "stone", "Test Stone");

        // Assert
        Assert.Equal("test:stone", block.Id.FullKey);
        Assert.Equal("Test Stone", block.Name);
    }

    [Fact]
    public void BlockRegistry_RegistersBlocks()
    {
        // Arrange
        var registry = BlockRegistry.Instance;
        var block = new SimpleBlock("testmod", "custom_block", "Custom Block");

        // Act
        registry.Register(block);

        // Assert
        Assert.True(registry.IsRegistered(block.Id));
        Assert.Equal(block, registry.Get(block.Id));
    }

    [Fact]
    public void BlockRegistry_GetBlockByString()
    {
        // Arrange
        var registry = BlockRegistry.Instance;
        var block = new SimpleBlock("testmod", "test_block", "Test Block");
        registry.Register(block);

        // Act
        var retrieved = registry.Get("testmod:test_block");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(block.Id, retrieved.Id);
    }

    [Fact]
    public void BlockRegistry_CanGetBlocksInNamespace()
    {
        // Arrange
        var registry = BlockRegistry.Instance;
        registry.Register(new SimpleBlock("mymod", "block1", "Block 1"));
        registry.Register(new SimpleBlock("mymod", "block2", "Block 2"));
        registry.Register(new SimpleBlock("othermod", "block3", "Block 3"));

        // Act
        var mymodBlocks = registry.GetBlocksInNamespace("mymod");

        // Assert
        Assert.Equal(2, mymodBlocks.Count());
    }

    [Fact]
    public void Block_HasCorrectDefaultProperties()
    {
        // Arrange & Act
        var block = new SimpleBlock("test", "stone", "Stone");

        // Assert
        Assert.Equal(1.0f, block.Hardness);
        Assert.Equal(1.0f, block.Resistance);
        Assert.Equal(0, block.LightLevel);
        Assert.True(block.IsSolid);
        Assert.False(block.IsTransparent);
        Assert.True(block.IsBreakable);
    }

    [Fact]
    public void Block_CanSetProperties()
    {
        // Arrange
        var block = new SimpleBlock("test", "glass", "Glass");

        // Act
        block.Hardness = 0.3f;
        block.IsTransparent = true;
        block.LightLevel = 0;

        // Assert
        Assert.Equal(0.3f, block.Hardness);
        Assert.True(block.IsTransparent);
    }
}
