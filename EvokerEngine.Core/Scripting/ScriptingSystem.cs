using System;
using System.Collections.Generic;

namespace EvokerEngine.Scripting;

/// <summary>
/// Base class for scripts that can be attached to entities
/// </summary>
public abstract class Script
{
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

    private readonly List<Script> _scripts = new();

    private ScriptManager() { }

    /// <summary>
    /// Register a script
    /// </summary>
    public void RegisterScript(Script script)
    {
        if (!_scripts.Contains(script))
        {
            _scripts.Add(script);
            script.OnStart();
        }
    }

    /// <summary>
    /// Unregister a script
    /// </summary>
    public void UnregisterScript(Script script)
    {
        if (_scripts.Remove(script))
        {
            script.OnDestroy();
        }
    }

    /// <summary>
    /// Update all scripts
    /// </summary>
    public void Update(float deltaTime)
    {
        foreach (var script in _scripts)
        {
            try
            {
                script.OnUpdate(deltaTime);
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
        foreach (var script in _scripts)
        {
            try
            {
                script.OnFixedUpdate(fixedDeltaTime);
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
        foreach (var script in _scripts)
        {
            script.OnDestroy();
        }
        _scripts.Clear();
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
