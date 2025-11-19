using Xunit;
using EvokerEngine.Inventory;
using EvokerEngine.Core;

namespace EvokerEngine.Tests;

public class InventoryTests
{
    [Fact]
    public void Inventory_CanAddItem()
    {
        // Arrange
        var inventory = new Inventory.Inventory(10);
        var item = new SimpleItem("test", "item", "Test Item");

        // Act
        var success = inventory.AddItem(item, 1);

        // Assert
        Assert.True(success);
        Assert.Equal(1, inventory.OccupiedSlots);
    }

    [Fact]
    public void Inventory_StacksItemsCorrectly()
    {
        // Arrange
        var inventory = new Inventory.Inventory(10);
        var item = new SimpleItem("test", "item", "Test Item");
        item.MaxStackSize = 64;

        // Act
        inventory.AddItem(item, 32);
        inventory.AddItem(item, 16);

        // Assert
        Assert.Equal(1, inventory.OccupiedSlots);
        Assert.Equal(48, inventory.GetItemCount(item.Id));
    }

    [Fact]
    public void Inventory_RemovesItems()
    {
        // Arrange
        var inventory = new Inventory.Inventory(10);
        var item = new SimpleItem("test", "item", "Test Item");
        inventory.AddItem(item, 5);

        // Act
        var removed = inventory.RemoveItem(item.Id, 3);

        // Assert
        Assert.Equal(3, removed);
        Assert.Equal(2, inventory.GetItemCount(item.Id));
    }

    [Fact]
    public void Inventory_RespectsWeightLimit()
    {
        // Arrange
        var inventory = new Inventory.Inventory(10);
        inventory.MaxWeight = 10f;
        
        var item = new SimpleItem("test", "item", "Heavy Item");
        item.Weight = 5f;

        // Act
        var success1 = inventory.AddItem(item, 1);
        var success2 = inventory.AddItem(item, 1);
        var success3 = inventory.AddItem(item, 1);  // Should fail

        // Assert
        Assert.True(success1);
        Assert.True(success2);
        Assert.False(success3);
        Assert.Equal(10f, inventory.CurrentWeight);
    }

    [Fact]
    public void Inventory_CanCheckIfHasItem()
    {
        // Arrange
        var inventory = new Inventory.Inventory(10);
        var item = new SimpleItem("test", "item", "Test Item");
        inventory.AddItem(item, 5);

        // Assert
        Assert.True(inventory.HasItem(item.Id, 5));
        Assert.True(inventory.HasItem(item.Id, 3));
        Assert.False(inventory.HasItem(item.Id, 6));
    }

    [Fact]
    public void ItemStack_MergesCorrectly()
    {
        // Arrange
        var item = new SimpleItem("test", "item", "Test Item");
        item.MaxStackSize = 64;
        
        var stack1 = new ItemStack(item, 30);
        var stack2 = new ItemStack(item, 20);

        // Act
        var success = stack1.Merge(stack2);

        // Assert
        Assert.True(success);
        Assert.Equal(50, stack1.Quantity);
        Assert.Equal(0, stack2.Quantity);
    }

    [Fact]
    public void ItemStack_SplitsCorrectly()
    {
        // Arrange
        var item = new SimpleItem("test", "item", "Test Item");
        item.MaxStackSize = 64;
        var stack = new ItemStack(item, 50);

        // Act
        var newStack = stack.Split(20);

        // Assert
        Assert.NotNull(newStack);
        Assert.Equal(30, stack.Quantity);
        Assert.Equal(20, newStack.Quantity);
    }
}
