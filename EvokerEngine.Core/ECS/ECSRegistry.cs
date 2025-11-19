using System;
using System.Collections.Generic;
using System.Linq;

namespace EvokerEngine.ECS;

/// <summary>
/// Entity Component System registry
/// </summary>
public class ECSRegistry
{
    private uint _nextEntityId = 1;
    private readonly Dictionary<Entity, List<Component>> _entityComponents = new();
    private readonly Dictionary<Type, List<Component>> _componentsByType = new();

    /// <summary>
    /// Create a new entity
    /// </summary>
    public Entity CreateEntity()
    {
        var entity = new Entity(_nextEntityId++);
        _entityComponents[entity] = new List<Component>();
        return entity;
    }

    /// <summary>
    /// Destroy an entity and all its components
    /// </summary>
    public void DestroyEntity(Entity entity)
    {
        if (_entityComponents.TryGetValue(entity, out var components))
        {
            foreach (var component in components)
            {
                var type = component.GetType();
                if (_componentsByType.TryGetValue(type, out var typeList))
                {
                    typeList.Remove(component);
                }
            }
            _entityComponents.Remove(entity);
        }
    }

    /// <summary>
    /// Add a component to an entity
    /// </summary>
    public T AddComponent<T>(Entity entity) where T : Component, new()
    {
        var component = new T { Entity = entity };
        
        if (!_entityComponents.ContainsKey(entity))
        {
            _entityComponents[entity] = new List<Component>();
        }
        
        _entityComponents[entity].Add(component);

        var type = typeof(T);
        if (!_componentsByType.ContainsKey(type))
        {
            _componentsByType[type] = new List<Component>();
        }
        _componentsByType[type].Add(component);

        return component;
    }

    /// <summary>
    /// Get a component from an entity
    /// </summary>
    public T? GetComponent<T>(Entity entity) where T : Component
    {
        if (_entityComponents.TryGetValue(entity, out var components))
        {
            return components.OfType<T>().FirstOrDefault();
        }
        return null;
    }

    /// <summary>
    /// Check if entity has a component
    /// </summary>
    public bool HasComponent<T>(Entity entity) where T : Component
    {
        if (_entityComponents.TryGetValue(entity, out var components))
        {
            return components.Any(c => c is T);
        }
        return false;
    }

    /// <summary>
    /// Remove a component from an entity
    /// </summary>
    public void RemoveComponent<T>(Entity entity) where T : Component
    {
        if (_entityComponents.TryGetValue(entity, out var components))
        {
            var component = components.OfType<T>().FirstOrDefault();
            if (component != null)
            {
                components.Remove(component);
                
                var type = typeof(T);
                if (_componentsByType.TryGetValue(type, out var typeList))
                {
                    typeList.Remove(component);
                }
            }
        }
    }

    /// <summary>
    /// Get all components of a specific type
    /// </summary>
    public IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        var type = typeof(T);
        if (_componentsByType.TryGetValue(type, out var components))
        {
            return components.Cast<T>();
        }
        return Enumerable.Empty<T>();
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    public IEnumerable<Entity> GetAllEntities()
    {
        return _entityComponents.Keys;
    }
}
