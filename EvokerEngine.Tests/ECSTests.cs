using Xunit;
using EvokerEngine.ECS;
using System.Linq;

namespace EvokerEngine.Tests;

public class ECSTests
{
    [Fact]
    public void Entity_HasUniqueId()
    {
        // Arrange & Act
        var entity1 = new Entity(1);
        var entity2 = new Entity(2);

        // Assert
        Assert.NotEqual(entity1.Id, entity2.Id);
    }

    [Fact]
    public void Entity_EqualityWorks()
    {
        // Arrange
        var entity1 = new Entity(1);
        var entity2 = new Entity(1);
        var entity3 = new Entity(2);

        // Assert
        Assert.Equal(entity1, entity2);
        Assert.NotEqual(entity1, entity3);
        Assert.True(entity1 == entity2);
        Assert.True(entity1 != entity3);
    }

    [Fact]
    public void ECSRegistry_CreateEntity_ReturnsUniqueEntities()
    {
        // Arrange
        var registry = new ECSRegistry();

        // Act
        var entity1 = registry.CreateEntity();
        var entity2 = registry.CreateEntity();

        // Assert
        Assert.NotEqual(entity1.Id, entity2.Id);
    }

    [Fact]
    public void ECSRegistry_AddComponent_AttachesComponent()
    {
        // Arrange
        var registry = new ECSRegistry();
        var entity = registry.CreateEntity();

        // Act
        var transform = registry.AddComponent<TransformComponent>(entity);

        // Assert
        Assert.NotNull(transform);
        Assert.Equal(entity, transform.Entity);
    }

    [Fact]
    public void ECSRegistry_GetComponent_ReturnsComponent()
    {
        // Arrange
        var registry = new ECSRegistry();
        var entity = registry.CreateEntity();
        var addedTransform = registry.AddComponent<TransformComponent>(entity);

        // Act
        var retrievedTransform = registry.GetComponent<TransformComponent>(entity);

        // Assert
        Assert.NotNull(retrievedTransform);
        Assert.Equal(addedTransform, retrievedTransform);
    }

    [Fact]
    public void ECSRegistry_HasComponent_ReturnsTrue()
    {
        // Arrange
        var registry = new ECSRegistry();
        var entity = registry.CreateEntity();
        registry.AddComponent<TransformComponent>(entity);

        // Act
        var hasComponent = registry.HasComponent<TransformComponent>(entity);

        // Assert
        Assert.True(hasComponent);
    }

    [Fact]
    public void ECSRegistry_RemoveComponent_RemovesComponent()
    {
        // Arrange
        var registry = new ECSRegistry();
        var entity = registry.CreateEntity();
        registry.AddComponent<TransformComponent>(entity);

        // Act
        registry.RemoveComponent<TransformComponent>(entity);

        // Assert
        Assert.False(registry.HasComponent<TransformComponent>(entity));
        Assert.Null(registry.GetComponent<TransformComponent>(entity));
    }

    [Fact]
    public void ECSRegistry_GetAllComponents_ReturnsAllComponentsOfType()
    {
        // Arrange
        var registry = new ECSRegistry();
        var entity1 = registry.CreateEntity();
        var entity2 = registry.CreateEntity();
        registry.AddComponent<TransformComponent>(entity1);
        registry.AddComponent<TransformComponent>(entity2);

        // Act
        var components = registry.GetAllComponents<TransformComponent>().ToList();

        // Assert
        Assert.Equal(2, components.Count);
    }

    [Fact]
    public void ECSRegistry_DestroyEntity_RemovesEntityAndComponents()
    {
        // Arrange
        var registry = new ECSRegistry();
        var entity = registry.CreateEntity();
        registry.AddComponent<TransformComponent>(entity);

        // Act
        registry.DestroyEntity(entity);

        // Assert
        Assert.False(registry.HasComponent<TransformComponent>(entity));
        Assert.DoesNotContain(entity, registry.GetAllEntities());
    }

    [Fact]
    public void ECSRegistry_GetAllEntities_ReturnsAllEntities()
    {
        // Arrange
        var registry = new ECSRegistry();
        var entity1 = registry.CreateEntity();
        var entity2 = registry.CreateEntity();
        var entity3 = registry.CreateEntity();

        // Act
        var entities = registry.GetAllEntities().ToList();

        // Assert
        Assert.Equal(3, entities.Count);
        Assert.Contains(entity1, entities);
        Assert.Contains(entity2, entities);
        Assert.Contains(entity3, entities);
    }
}
