using System;
using System.Collections.Generic;
using System.Linq;
using EvokerEngine.Core;

namespace EvokerEngine.Inventory;

/// <summary>
/// Inventory system for managing items
/// </summary>
public class Inventory
{
    private readonly List<ItemStack> _slots;
    private readonly int _capacity;

    /// <summary>
    /// Maximum number of slots in this inventory
    /// </summary>
    public int Capacity => _capacity;

    /// <summary>
    /// Number of occupied slots
    /// </summary>
    public int OccupiedSlots => _slots.Count(s => !s.IsEmpty);

    /// <summary>
    /// Number of empty slots
    /// </summary>
    public int EmptySlots => _capacity - OccupiedSlots;

    /// <summary>
    /// Whether the inventory is full
    /// </summary>
    public bool IsFull => EmptySlots == 0;

    /// <summary>
    /// Whether the inventory is empty
    /// </summary>
    public bool IsEmpty => OccupiedSlots == 0;

    /// <summary>
    /// Maximum weight capacity (0 = unlimited)
    /// </summary>
    public float MaxWeight { get; set; } = 0f;

    /// <summary>
    /// Current total weight of items
    /// </summary>
    public float CurrentWeight => _slots.Sum(s => s.Item.Weight * s.Quantity);

    /// <summary>
    /// Whether weight limit is enabled
    /// </summary>
    public bool HasWeightLimit => MaxWeight > 0f;

    /// <summary>
    /// Remaining weight capacity
    /// </summary>
    public float RemainingWeight => HasWeightLimit ? Math.Max(0, MaxWeight - CurrentWeight) : float.MaxValue;

    /// <summary>
    /// Event fired when inventory changes
    /// </summary>
    public event Action<Inventory>? OnInventoryChanged;

    public Inventory(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));

        _capacity = capacity;
        _slots = new List<ItemStack>(capacity);
    }

    /// <summary>
    /// Add an item to the inventory
    /// </summary>
    /// <returns>True if item was fully added, false if rejected or partial</returns>
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
            return false;

        // Check weight limit
        var totalWeight = item.Weight * quantity;
        if (HasWeightLimit && CurrentWeight + totalWeight > MaxWeight)
            return false;

        var remainingQuantity = quantity;

        // Try to stack with existing items
        if (item.MaxStackSize > 1)
        {
            foreach (var stack in _slots.Where(s => s.CanStackWith(item)))
            {
                var overflow = stack.Add(remainingQuantity);
                remainingQuantity = overflow;
                
                if (remainingQuantity == 0)
                {
                    OnInventoryChanged?.Invoke(this);
                    return true;
                }
            }
        }

        // Add to new slots
        while (remainingQuantity > 0 && _slots.Count < _capacity)
        {
            var stackSize = Math.Min(remainingQuantity, item.MaxStackSize);
            _slots.Add(new ItemStack(item.Clone(), stackSize));
            remainingQuantity -= stackSize;
        }

        if (remainingQuantity < quantity)
            OnInventoryChanged?.Invoke(this);

        return remainingQuantity == 0;
    }

    /// <summary>
    /// Remove an item from the inventory
    /// </summary>
    /// <returns>Amount actually removed</returns>
    public int RemoveItem(ResourceKey itemId, int quantity = 1)
    {
        if (quantity <= 0)
            return 0;

        var removedTotal = 0;
        var slotsToRemove = new List<ItemStack>();

        foreach (var stack in _slots.Where(s => s.Item.Id == itemId))
        {
            var toRemove = Math.Min(quantity - removedTotal, stack.Quantity);
            stack.Remove(toRemove);
            removedTotal += toRemove;

            if (stack.IsEmpty)
                slotsToRemove.Add(stack);

            if (removedTotal >= quantity)
                break;
        }

        foreach (var slot in slotsToRemove)
            _slots.Remove(slot);

        if (removedTotal > 0)
            OnInventoryChanged?.Invoke(this);

        return removedTotal;
    }

    /// <summary>
    /// Get total quantity of a specific item
    /// </summary>
    public int GetItemCount(ResourceKey itemId)
    {
        return _slots.Where(s => s.Item.Id == itemId).Sum(s => s.Quantity);
    }

    /// <summary>
    /// Check if inventory contains at least the specified quantity of an item
    /// </summary>
    public bool HasItem(ResourceKey itemId, int quantity = 1)
    {
        return GetItemCount(itemId) >= quantity;
    }

    /// <summary>
    /// Get all items in the inventory
    /// </summary>
    public IReadOnlyList<ItemStack> GetAllItems()
    {
        return _slots.AsReadOnly();
    }

    /// <summary>
    /// Get item at specific slot
    /// </summary>
    public ItemStack? GetItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count)
            return null;
        return _slots[slotIndex];
    }

    /// <summary>
    /// Move item from one slot to another
    /// </summary>
    public bool MoveItem(int fromSlot, int toSlot)
    {
        if (fromSlot < 0 || fromSlot >= _slots.Count ||
            toSlot < 0 || toSlot >= _capacity)
            return false;

        if (fromSlot == toSlot)
            return true;

        var fromStack = _slots[fromSlot];

        // If target slot is empty
        if (toSlot >= _slots.Count)
        {
            _slots.Insert(toSlot, fromStack);
            _slots.RemoveAt(fromSlot < toSlot ? fromSlot : fromSlot + 1);
            OnInventoryChanged?.Invoke(this);
            return true;
        }

        var toStack = _slots[toSlot];

        // Try to merge stacks
        if (fromStack.CanStackWith(toStack))
        {
            var merged = toStack.Merge(fromStack);
            if (fromStack.IsEmpty)
                _slots.RemoveAt(fromSlot);
            OnInventoryChanged?.Invoke(this);
            return true;
        }

        // Swap slots
        _slots[fromSlot] = toStack;
        _slots[toSlot] = fromStack;
        OnInventoryChanged?.Invoke(this);
        return true;
    }

    /// <summary>
    /// Clear all items from inventory
    /// </summary>
    public void Clear()
    {
        _slots.Clear();
        OnInventoryChanged?.Invoke(this);
    }

    /// <summary>
    /// Sort inventory by item name
    /// </summary>
    public void Sort(Comparison<ItemStack>? comparison = null)
    {
        if (comparison == null)
            _slots.Sort((a, b) => string.Compare(a.Item.Name, b.Item.Name, StringComparison.Ordinal));
        else
            _slots.Sort(comparison);
        
        OnInventoryChanged?.Invoke(this);
    }
}
