using System;

namespace EvokerEngine.Inventory;

/// <summary>
/// Represents a stack of items with quantity
/// </summary>
public class ItemStack
{
    /// <summary>
    /// The item type in this stack
    /// </summary>
    public Item Item { get; private set; }

    /// <summary>
    /// Current quantity in this stack
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Maximum quantity this stack can hold
    /// </summary>
    public int MaxQuantity => Item.MaxStackSize;

    /// <summary>
    /// Whether this stack is at maximum capacity
    /// </summary>
    public bool IsFull => Quantity >= MaxQuantity;

    /// <summary>
    /// Whether this stack is empty
    /// </summary>
    public bool IsEmpty => Quantity <= 0;

    /// <summary>
    /// Remaining space in this stack
    /// </summary>
    public int RemainingSpace => MaxQuantity - Quantity;

    public ItemStack(Item item, int quantity = 1)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Quantity = Math.Max(0, Math.Min(quantity, item.MaxStackSize));
    }

    /// <summary>
    /// Add items to this stack
    /// </summary>
    /// <param name="amount">Amount to add</param>
    /// <returns>Amount that couldn't be added (overflow)</returns>
    public int Add(int amount)
    {
        if (amount <= 0) return 0;

        var spaceAvailable = RemainingSpace;
        var amountToAdd = Math.Min(amount, spaceAvailable);
        var overflow = amount - amountToAdd;

        Quantity += amountToAdd;
        return overflow;
    }

    /// <summary>
    /// Remove items from this stack
    /// </summary>
    /// <param name="amount">Amount to remove</param>
    /// <returns>Amount actually removed</returns>
    public int Remove(int amount)
    {
        if (amount <= 0) return 0;

        var amountToRemove = Math.Min(amount, Quantity);
        Quantity -= amountToRemove;
        return amountToRemove;
    }

    /// <summary>
    /// Split this stack into two stacks
    /// </summary>
    /// <param name="splitAmount">Amount to split off</param>
    /// <returns>New stack with split amount, or null if invalid</returns>
    public ItemStack? Split(int splitAmount)
    {
        if (splitAmount <= 0 || splitAmount >= Quantity)
            return null;

        var removed = Remove(splitAmount);
        return new ItemStack(Item.Clone(), removed);
    }

    /// <summary>
    /// Merge another stack into this one
    /// </summary>
    /// <param name="other">Stack to merge</param>
    /// <returns>True if fully merged, false if partial/failed</returns>
    public bool Merge(ItemStack other)
    {
        if (other == null || other.Item.Id != Item.Id)
            return false;

        var overflow = Add(other.Quantity);
        other.Remove(other.Quantity - overflow);
        
        return overflow == 0;
    }

    /// <summary>
    /// Check if this stack can stack with another item
    /// </summary>
    public bool CanStackWith(Item item)
    {
        return item != null && item.Id == Item.Id && !IsFull;
    }

    /// <summary>
    /// Check if this stack can stack with another stack
    /// </summary>
    public bool CanStackWith(ItemStack stack)
    {
        return stack != null && CanStackWith(stack.Item);
    }

    public override string ToString()
    {
        return $"{Item.Name} x{Quantity}";
    }
}
