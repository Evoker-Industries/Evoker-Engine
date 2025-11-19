using System;
using System.Collections.Generic;
using EvokerEngine.Core;

namespace EvokerEngine.Modding;

/// <summary>
/// Metadata about a mod
/// </summary>
public class ModInfo
{
    /// <summary>
    /// Unique identifier for the mod (namespace)
    /// </summary>
    public string ModId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the mod
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mod version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Mod author(s)
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Description of the mod
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Mods that this mod depends on
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Custom metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    public override string ToString()
    {
        return $"{Name} ({ModId}) v{Version}";
    }
}

/// <summary>
/// Base class for all mods
/// </summary>
public abstract class Mod
{
    /// <summary>
    /// Information about this mod
    /// </summary>
    public ModInfo Info { get; protected set; } = new();

    /// <summary>
    /// Whether the mod is currently loaded
    /// </summary>
    public bool IsLoaded { get; internal set; }

    /// <summary>
    /// Logger instance for this mod
    /// </summary>
    protected void Log(string message) => Logger.Info($"[{Info.ModId}] {message}");
    protected void LogWarning(string message) => Logger.Warning($"[{Info.ModId}] {message}");
    protected void LogError(string message) => Logger.Error($"[{Info.ModId}] {message}");

    /// <summary>
    /// Called when the mod is loaded
    /// </summary>
    public virtual void OnLoad()
    {
        Log($"Loading mod: {Info.Name} v{Info.Version}");
    }

    /// <summary>
    /// Called to initialize the mod (register blocks, items, recipes, etc.)
    /// </summary>
    public virtual void OnInitialize()
    {
        Log("Initializing mod");
    }

    /// <summary>
    /// Called after all mods are initialized
    /// </summary>
    public virtual void OnPostInitialize()
    {
        Log("Post-initialization complete");
    }

    /// <summary>
    /// Called when the mod is unloaded
    /// </summary>
    public virtual void OnUnload()
    {
        Log("Unloading mod");
    }

    /// <summary>
    /// Called every frame for mods that need updates
    /// </summary>
    public virtual void OnUpdate(float deltaTime)
    {
    }

    /// <summary>
    /// Get a resource key for this mod's namespace
    /// </summary>
    protected ResourceKey GetResourceKey(string key)
    {
        return new ResourceKey(Info.ModId, key);
    }

    public override string ToString()
    {
        return Info.ToString();
    }
}
