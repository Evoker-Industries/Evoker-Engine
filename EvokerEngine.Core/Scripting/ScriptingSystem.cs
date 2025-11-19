using System;
using System.Collections.Generic;
using EvokerEngine.ECS;

namespace EvokerEngine.Scripting;

/// <summary>
/// Base class for scripts that can be attached to entities
/// </summary>
public abstract class Script
{
    /// <summary>
    /// The entity this script is attached to
    /// </summary>
    public Entity? Entity { get; internal set; }

    /// <summary>
    /// Get a component from the entity
    /// </summary>
    protected T? GetComponent<T>() where T : Component
    {
        if (Entity == null) return null;
        return Scene.Scene.GetActiveScene()?.Registry.GetComponent<T>(Entity.Value);
    }

    /// <summary>
    /// Called when the script is first initialized
    /// </summary>
    public virtual void OnStart() { }

    /// <summary>
    /// Called every frame
    /// </summary>
    public virtual void OnUpdate(float deltaTime) { }

    /// <summary>
    /// Called at fixed time intervals for physics
    /// </summary>
    public virtual void OnFixedUpdate(float fixedDeltaTime) { }

    /// <summary>
    /// Called when the script is destroyed
    /// </summary>
    public virtual void OnDestroy() { }

    /// <summary>
    /// Called when a collision occurs (if rigidbody attached)
    /// </summary>
    public virtual void OnCollision(Collision collision) { }

    /// <summary>
    /// Called when entering a trigger (if collider attached)
    /// </summary>
    public virtual void OnTriggerEnter(Collider other) { }

    /// <summary>
    /// Called when exiting a trigger (if collider attached)
    /// </summary>
    public virtual void OnTriggerExit(Collider other) { }
}

/// <summary>
/// Component that holds a script instance
/// </summary>
public class ScriptComponent : Component
{
    public Script? Script { get; set; }
    private bool _started = false;

    public void Update(float deltaTime)
    {
        if (Script == null) return;

        if (!_started)
        {
            Script.Entity = Entity;
            Script.OnStart();
            _started = true;
        }

        Script.OnUpdate(deltaTime);
    }

    public void FixedUpdate(float fixedDeltaTime)
    {
        Script?.OnFixedUpdate(fixedDeltaTime);
    }

    public void Destroy()
    {
        Script?.OnDestroy();
        Script = null;
    }
}

/// <summary>
/// Collision information
/// </summary>
public class Collision
{
    public Collider? Collider { get; set; }
    public List<ContactPoint> Contacts { get; set; } = new();
    public System.Numerics.Vector3 RelativeVelocity { get; set; }
}

/// <summary>
/// Contact point information
/// </summary>
public struct ContactPoint
{
    public System.Numerics.Vector3 Point { get; set; }
    public System.Numerics.Vector3 Normal { get; set; }
    public float Separation { get; set; }
}

/// <summary>
/// Collider placeholder for scripting
/// </summary>
public class Collider
{
    public string Tag { get; set; } = string.Empty;
}

/// <summary>
/// Script manager for handling script execution
/// </summary>
public class ScriptManager
{
    private static ScriptManager? _instance;
    public static ScriptManager Instance => _instance ??= new ScriptManager();

    private readonly List<ScriptComponent> _scriptComponents = new();

    private ScriptManager() { }

    /// <summary>
    /// Register a script component
    /// </summary>
    public void RegisterScriptComponent(ScriptComponent component)
    {
        if (!_scriptComponents.Contains(component))
        {
            _scriptComponents.Add(component);
        }
    }

    /// <summary>
    /// Unregister a script component
    /// </summary>
    public void UnregisterScriptComponent(ScriptComponent component)
    {
        if (_scriptComponents.Remove(component))
        {
            component.Destroy();
        }
    }

    /// <summary>
    /// Update all scripts
    /// </summary>
    public void Update(float deltaTime)
    {
        foreach (var component in _scriptComponents)
        {
            try
            {
                component.Update(deltaTime);
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Error in script Update: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Fixed update for physics
    /// </summary>
    public void FixedUpdate(float fixedDeltaTime)
    {
        foreach (var component in _scriptComponents)
        {
            try
            {
                component.FixedUpdate(fixedDeltaTime);
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Error in script FixedUpdate: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clear all scripts
    /// </summary>
    public void Clear()
    {
        foreach (var component in _scriptComponents)
        {
            component.Destroy();
        }
        _scriptComponents.Clear();
    }
}

/// <summary>
/// Hot reload support for scripts (future feature)
/// </summary>
public class ScriptHotReload
{
    // TODO: Implement hot reload functionality
    // - Watch script files for changes
    // - Recompile changed scripts
    // - Reload scripts without stopping the game
    // - Preserve script state where possible

    public static void Initialize()
    {
        Core.Logger.Info("Script hot reload not yet implemented");
    }

    public static void EnableHotReload(string scriptsPath)
    {
        Core.Logger.Warning($"Script hot reload not yet implemented for path: {scriptsPath}");
    }
}

/// <summary>
/// Example script for demonstration
/// </summary>
public class ExampleScript : Script
{
    private float _timer = 0f;

    public override void OnStart()
    {
        Core.Logger.Info("ExampleScript started");
    }

    public override void OnUpdate(float deltaTime)
    {
        _timer += deltaTime;
        
        if (_timer >= 1.0f)
        {
            Core.Logger.Info($"ExampleScript running for {(int)_timer} seconds");
            _timer = 0f;
        }
    }

    public override void OnDestroy()
    {
        Core.Logger.Info("ExampleScript destroyed");
    }
}

/// <summary>
/// Example movement script
/// </summary>
public class SimpleMovementScript : Script
{
    public float Speed { get; set; } = 5.0f;

    public override void OnUpdate(float deltaTime)
    {
        var transform = GetComponent<TransformComponent>();
        if (transform == null) return;

        // Simple WASD movement
        if (Core.Input.IsKeyPressed(Silk.NET.Input.Key.W))
            transform.Position += new System.Numerics.Vector3(0, 0, -Speed * deltaTime);
        if (Core.Input.IsKeyPressed(Silk.NET.Input.Key.S))
            transform.Position += new System.Numerics.Vector3(0, 0, Speed * deltaTime);
        if (Core.Input.IsKeyPressed(Silk.NET.Input.Key.A))
            transform.Position += new System.Numerics.Vector3(-Speed * deltaTime, 0, 0);
        if (Core.Input.IsKeyPressed(Silk.NET.Input.Key.D))
            transform.Position += new System.Numerics.Vector3(Speed * deltaTime, 0, 0);
    }
}

/// <summary>
/// Example rotation script
/// </summary>
public class SimpleRotationScript : Script
{
    public float RotationSpeed { get; set; } = 45.0f; // degrees per second

    public override void OnUpdate(float deltaTime)
    {
        var transform = GetComponent<TransformComponent>();
        if (transform == null) return;

        transform.Rotation += new System.Numerics.Vector3(0, RotationSpeed * deltaTime, 0);
    }
}
