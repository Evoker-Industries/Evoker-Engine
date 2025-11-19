# Inventory System

The Inventory system in Evoker Engine provides a flexible item and inventory management system with support for stacking, weight limits, sorting, and custom item properties.

## Overview

The inventory system consists of:

- **Items**: Base objects representing in-game items
- **ItemStack**: Handles stackable items with quantities
- **Inventory**: Container for managing multiple item stacks
- **Item Rarity**: Classification system for items

## Items

### Item Base Class

```csharp
public abstract class Item
{
    public ResourceKey Id { get; set; }        // Unique identifier (namespace:key)
    public string Name { get; set; }           // Display name
    public string Description { get; set; }    // Item description
    public string IconId { get; set; }         // Icon/sprite ID
    public int MaxStackSize { get; set; }      // Maximum stack size
    public int Value { get; set; }             // Base value/price
    public float Weight { get; set; }          // Weight per item
    public ItemRarity Rarity { get; set; }     // Rarity level
    
    public virtual bool OnUse(object? context = null);
    public virtual void OnEquip(object? context = null);
    public virtual void OnUnequip(object? context = null);
    public virtual Item Clone();
}
```

### Creating Items

```csharp
using EvokerEngine.Inventory;
using EvokerEngine.Core;

// Simple item
var coin = new SimpleItem("game:gold_coin", "Gold Coin")
{
    Description = "A shiny gold coin",
    MaxStackSize = 999,
    Value = 1,
    Weight = 0.01f,
    Rarity = ItemRarity.Common
};

// Custom item with behavior
public class HealthPotion : Item
{
    public float HealAmount { get; set; } = 50f;

    public HealthPotion()
    {
        Id = new ResourceKey("game", "health_potion");
        Name = "Health Potion";
        Description = "Restores 50 health points";
        MaxStackSize = 10;
        Value = 25;
        Weight = 0.5f;
        Rarity = ItemRarity.Uncommon;
    }

    public override bool OnUse(object? context = null)
    {
        // Heal the player
        if (context is Player player)
        {
            player.Heal(HealAmount);
            Logger.Info($"Healed {HealAmount} HP");
            return true; // Consumed
        }
        return false;
    }
}
```

### Item Rarity

```csharp
public enum ItemRarity
{
    Common,      // White/Gray
    Uncommon,    // Green
    Rare,        // Blue
    Epic,        // Purple
    Legendary,   // Orange
    Mythic       // Red/Rainbow
}
```

**Example:**
```csharp
var sword = new SimpleItem("game:legendary_sword", "Excalibur")
{
    Rarity = ItemRarity.Legendary,
    Value = 10000,
    MaxStackSize = 1
};
```

## ItemStack

Represents a stack of items with quantity.

```csharp
public class ItemStack
{
    public Item Item { get; }
    public int Quantity { get; }
    public int MaxQuantity { get; }
    public bool IsFull { get; }
    public bool IsEmpty { get; }
    public int RemainingSpace { get; }
    
    public int Add(int amount);
    public int Remove(int amount);
    public ItemStack Split(int splitAmount);
    public bool CanStackWith(Item otherItem);
    public bool CanMergeWith(ItemStack otherStack);
    public int MergeWith(ItemStack otherStack);
}
```

### Working with ItemStacks

```csharp
// Create a stack
var stack = new ItemStack(healthPotion, 5);

// Add to stack
var overflow = stack.Add(3);  // Returns 0 if all added
if (overflow > 0)
{
    Logger.Info($"Could not add {overflow} items");
}

// Remove from stack
var removed = stack.Remove(2);  // Returns actual amount removed

// Split stack
var newStack = stack.Split(2);  // Take 2 items into new stack

// Check if can stack
if (stack.CanStackWith(healthPotion))
{
    // Can add this item to the stack
}

// Merge stacks
var otherStack = new ItemStack(healthPotion, 3);
var leftover = stack.MergeWith(otherStack);
```

## Inventory

Container for managing multiple item stacks.

```csharp
public class Inventory
{
    public int Capacity { get; }              // Maximum slots
    public int OccupiedSlots { get; }         // Used slots
    public int EmptySlots { get; }            // Free slots
    public bool IsFull { get; }               // Is inventory full
    public bool IsEmpty { get; }              // Is inventory empty
    public float MaxWeight { get; set; }      // Weight limit (0 = unlimited)
    public float CurrentWeight { get; }       // Current total weight
    public float RemainingWeight { get; }     // Remaining capacity
    
    public event Action<Inventory>? OnInventoryChanged;
    
    public bool AddItem(Item item, int quantity = 1);
    public bool RemoveItem(ResourceKey itemId, int quantity = 1);
    public ItemStack? GetSlot(int index);
    public void SetSlot(int index, ItemStack? stack);
    public void Clear();
    public void Sort(InventorySortMode mode);
}
```

### Creating and Using Inventories

```csharp
using EvokerEngine.Inventory;

// Create inventory with 20 slots
var inventory = new Inventory(20);

// Optional: Set weight limit
inventory.MaxWeight = 100f;  // 100kg limit

// Add items
if (inventory.AddItem(sword, 1))
{
    Logger.Info("Sword added to inventory");
}

if (!inventory.AddItem(healthPotion, 10))
{
    Logger.Info("Not enough space or too heavy");
}

// Remove items
if (inventory.RemoveItem(sword.Id, 1))
{
    Logger.Info("Sword removed");
}

// Check contents
for (int i = 0; i < inventory.Capacity; i++)
{
    var stack = inventory.GetSlot(i);
    if (stack != null && !stack.IsEmpty)
    {
        Logger.Info($"Slot {i}: {stack.Item.Name} x{stack.Quantity}");
    }
}

// Clear inventory
inventory.Clear();
```

### Inventory Events

```csharp
var inventory = new Inventory(20);

inventory.OnInventoryChanged += (inv) =>
{
    Logger.Info("Inventory changed!");
    UpdateUI();
};

inventory.AddItem(sword, 1);  // Triggers event
```

### Sorting Inventory

```csharp
// Sort by name
inventory.Sort(InventorySortMode.Name);

// Sort by value
inventory.Sort(InventorySortMode.Value);

// Sort by rarity
inventory.Sort(InventorySortMode.Rarity);

// Sort by quantity
inventory.Sort(InventorySortMode.Quantity);
```

## Weight System

```csharp
// Create inventory with weight limit
var backpack = new Inventory(30);
backpack.MaxWeight = 50f;  // 50kg capacity

// Check if item can be added
var heavyArmor = new SimpleItem("game:plate_armor", "Plate Armor")
{
    Weight = 20f
};

if (backpack.RemainingWeight >= heavyArmor.Weight)
{
    backpack.AddItem(heavyArmor, 1);
}
else
{
    Logger.Info("Too heavy to carry!");
}

// Check current weight
Logger.Info($"Carrying {backpack.CurrentWeight}kg / {backpack.MaxWeight}kg");
```

## InventoryComponent (ECS Integration)

Add inventory to entities:

```csharp
using EvokerEngine.ECS;
using EvokerEngine.Core;

var scene = Application.Instance.ActiveScene;
var player = scene.CreateEntity("Player");

// Add inventory component
var inventoryComp = scene.Registry.AddComponent<InventoryComponent>(player);
// Default: 20 slots, no weight limit

// Access the inventory
var inventory = inventoryComp.Inventory;
inventory.AddItem(sword, 1);

// Create with custom capacity and weight
public class CustomInventoryComponent : Component
{
    public Inventory Inventory { get; }
    
    public CustomInventoryComponent()
    {
        Inventory = new Inventory(40)  // 40 slots
        {
            MaxWeight = 100f  // 100kg limit
        };
    }
}
```

## Complete Examples

### Example 1: Player Inventory System

```csharp
public class PlayerInventoryLayer : Layer
{
    private Entity _player;
    private Inventory _inventory;

    public override void OnAttach()
    {
        var scene = Application.Instance.ActiveScene;
        _player = scene.CreateEntity("Player");
        
        var inventoryComp = scene.Registry.AddComponent<InventoryComponent>(_player);
        _inventory = inventoryComp.Inventory;
        _inventory.MaxWeight = 75f;
        
        _inventory.OnInventoryChanged += OnInventoryChanged;
        
        // Add starting items
        var sword = new SimpleItem("game:iron_sword", "Iron Sword")
        {
            Value = 100,
            Weight = 2f,
            Rarity = ItemRarity.Common
        };
        
        var potion = new HealthPotion();
        
        _inventory.AddItem(sword, 1);
        _inventory.AddItem(potion, 5);
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.I)
            {
                ShowInventoryUI();
            }
            else if (keyEvent.KeyCode == Key.Number1)
            {
                UseItem(0);  // Use item in slot 0
            }
        }
    }

    private void UseItem(int slotIndex)
    {
        var stack = _inventory.GetSlot(slotIndex);
        if (stack != null && !stack.IsEmpty)
        {
            if (stack.Item.OnUse(_player))
            {
                // Item was consumed
                _inventory.RemoveItem(stack.Item.Id, 1);
            }
        }
    }

    private void OnInventoryChanged(Inventory inv)
    {
        Logger.Info($"Inventory: {inv.OccupiedSlots}/{inv.Capacity} slots");
        Logger.Info($"Weight: {inv.CurrentWeight:F1}/{inv.MaxWeight:F1} kg");
    }

    private void ShowInventoryUI()
    {
        Logger.Info("=== INVENTORY ===");
        for (int i = 0; i < _inventory.Capacity; i++)
        {
            var stack = _inventory.GetSlot(i);
            if (stack != null && !stack.IsEmpty)
            {
                Logger.Info($"[{i}] {stack.Item.Name} x{stack.Quantity} ({stack.Item.Rarity})");
            }
        }
    }
}
```

### Example 2: Loot System

```csharp
public class LootSystem
{
    public static void DropLoot(Vector3 position, params (Item item, int quantity)[] loot)
    {
        foreach (var (item, quantity) in loot)
        {
            var scene = Application.Instance.ActiveScene;
            var lootEntity = scene.CreateEntity($"Loot_{item.Name}");
            
            var transform = scene.Registry.AddComponent<TransformComponent>(lootEntity);
            transform.Position = position + new Vector3(
                Random.Shared.NextSingle() * 2 - 1,
                1,
                Random.Shared.NextSingle() * 2 - 1
            );
            
            var inventoryComp = scene.Registry.AddComponent<InventoryComponent>(lootEntity);
            inventoryComp.Inventory.AddItem(item, quantity);
        }
    }

    public static void GiveRandomLoot(Inventory inventory)
    {
        var lootTable = new[]
        {
            (new SimpleItem("game:gold_coin", "Gold Coin") { MaxStackSize = 999 }, 50),
            (new HealthPotion(), 3),
            (new SimpleItem("game:iron_sword", "Iron Sword"), 1)
        };
        
        var roll = Random.Shared.Next(lootTable.Length);
        var (item, quantity) = lootTable[roll];
        
        inventory.AddItem(item, quantity);
        Logger.Info($"Found: {item.Name} x{quantity}");
    }
}
```

### Example 3: Trading System

```csharp
public class TradingSystem
{
    public static bool Trade(Inventory seller, Inventory buyer, 
                            ResourceKey itemId, int quantity, int price)
    {
        // Check if seller has the item
        var sellerHasItem = false;
        // Implementation to check...
        
        // Check if buyer has enough gold
        var goldCoin = new ResourceKey("game", "gold_coin");
        var buyerGold = GetItemCount(buyer, goldCoin);
        
        if (buyerGold < price)
        {
            Logger.Info("Not enough gold!");
            return false;
        }
        
        // Perform trade
        buyer.RemoveItem(goldCoin, price);
        seller.AddItem(new SimpleItem(goldCoin, "Gold Coin"), price);
        
        // Transfer item
        // Implementation...
        
        return true;
    }

    private static int GetItemCount(Inventory inv, ResourceKey itemId)
    {
        int count = 0;
        for (int i = 0; i < inv.Capacity; i++)
        {
            var stack = inv.GetSlot(i);
            if (stack != null && stack.Item.Id == itemId)
            {
                count += stack.Quantity;
            }
        }
        return count;
    }
}
```

## Best Practices

### ✅ Do's

- Use ResourceKey for item IDs (namespace:key format)
- Set appropriate MaxStackSize for items
- Implement OnUse() for consumable items
- Subscribe to OnInventoryChanged for UI updates
- Use weight limits for realistic gameplay

### ❌ Don'ts

- Don't exceed MaxStackSize when creating ItemStacks
- Don't forget to check AddItem() return value
- Don't modify ItemStack.Quantity directly
- Don't share Item instances between stacks (use Clone())

## Performance Tips

1. **Cache frequently used items** - Don't create new instances every frame
2. **Use ResourceKey comparisons** - Faster than string comparisons
3. **Minimize inventory events** - Batch operations when possible
4. **Set reasonable capacities** - Don't create massive inventories

## See Also

- [ECS](../core/ecs.md) - Entity Component System
- [Blocks](blocks.md) - Block system
- [Crafting](crafting.md) - Recipe and crafting system
