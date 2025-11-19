using System;
using System.Collections.Generic;
using EvokerEngine.ECS;

namespace EvokerEngine.Scene;

/// <summary>
/// Scene containing entities and systems
/// </summary>
public class Scene
{
    private readonly ECSRegistry _registry = new();
    private readonly List<Entity> _rootEntities = new();
    private static Scene? _activeScene;
    
    public string Name { get; set; }
    public ECSRegistry Registry => _registry;

    public Scene(string name = "Untitled Scene")
    {
        Name = name;
        if (_activeScene == null)
            _activeScene = this;
    }

    /// <summary>
    /// Get the active scene
    /// </summary>
    public static Scene? GetActiveScene() => _activeScene;

    /// <summary>
    /// Set the active scene
    /// </summary>
    public static void SetActiveScene(Scene scene) => _activeScene = scene;

    /// <summary>
    /// Create an entity in the scene
    /// </summary>
    public Entity CreateEntity(string name = "Entity")
    {
        var entity = _registry.CreateEntity();
        _rootEntities.Add(entity);
        return entity;
    }

    /// <summary>
    /// Destroy an entity in the scene
    /// </summary>
    public void DestroyEntity(Entity entity)
    {
        _rootEntities.Remove(entity);
        _registry.DestroyEntity(entity);
    }

    /// <summary>
    /// Update the scene
    /// </summary>
    public void Update(float deltaTime)
    {
        // Update systems here
    }

    /// <summary>
    /// Get all root entities
    /// </summary>
    public IEnumerable<Entity> GetRootEntities()
    {
        return _rootEntities;
    }
}
