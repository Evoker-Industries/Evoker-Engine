using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EvokerEngine.Core;

namespace EvokerEngine.Modding;

/// <summary>
/// Manages loading and lifecycle of mods
/// </summary>
public class ModLoader
{
    private static ModLoader? _instance;
    private readonly List<Mod> _loadedMods = new();
    private readonly Dictionary<string, Mod> _modsByIdIndex = new();
    private bool _isInitialized = false;

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static ModLoader Instance => _instance ??= new ModLoader();

    /// <summary>
    /// All loaded mods
    /// </summary>
    public IReadOnlyList<Mod> LoadedMods => _loadedMods.AsReadOnly();

    /// <summary>
    /// Number of loaded mods
    /// </summary>
    public int ModCount => _loadedMods.Count;

    /// <summary>
    /// Event fired when a mod is loaded
    /// </summary>
    public event Action<Mod>? OnModLoaded;

    /// <summary>
    /// Event fired when a mod is unloaded
    /// </summary>
    public event Action<Mod>? OnModUnloaded;

    private ModLoader()
    {
    }

    /// <summary>
    /// Load a mod from an assembly
    /// </summary>
    public void LoadMod(Assembly assembly)
    {
        Logger.Info($"Scanning assembly {assembly.GetName().Name} for mods...");

        var modTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Mod)));

        foreach (var modType in modTypes)
        {
            try
            {
                var mod = (Mod?)Activator.CreateInstance(modType);
                if (mod != null)
                {
                    LoadMod(mod);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load mod from type {modType.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Load a mod instance
    /// </summary>
    public void LoadMod(Mod mod)
    {
        if (mod == null)
            throw new ArgumentNullException(nameof(mod));

        if (string.IsNullOrWhiteSpace(mod.Info.ModId))
            throw new InvalidOperationException("Mod must have a valid ModId");

        if (_modsByIdIndex.ContainsKey(mod.Info.ModId))
        {
            Logger.Warning($"Mod '{mod.Info.ModId}' is already loaded. Skipping.");
            return;
        }

        // Check dependencies
        foreach (var dependency in mod.Info.Dependencies)
        {
            if (!_modsByIdIndex.ContainsKey(dependency))
            {
                Logger.Error($"Cannot load mod '{mod.Info.ModId}': missing dependency '{dependency}'");
                return;
            }
        }

        try
        {
            mod.OnLoad();
            mod.IsLoaded = true;
            _loadedMods.Add(mod);
            _modsByIdIndex[mod.Info.ModId] = mod;

            Logger.Info($"Loaded mod: {mod.Info}");
            OnModLoaded?.Invoke(mod);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load mod '{mod.Info.ModId}': {ex.Message}");
        }
    }

    /// <summary>
    /// Load mods from a directory
    /// </summary>
    public void LoadModsFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Logger.Warning($"Mods directory not found: {directory}");
            return;
        }

        Logger.Info($"Loading mods from: {directory}");

        var dllFiles = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories);
        
        foreach (var dllFile in dllFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllFile);
                LoadMod(assembly);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load assembly {Path.GetFileName(dllFile)}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Initialize all loaded mods
    /// </summary>
    public void InitializeAll()
    {
        if (_isInitialized)
        {
            Logger.Warning("Mods are already initialized");
            return;
        }

        Logger.Info("Initializing all mods...");

        // Sort mods by dependencies (topological sort)
        var sortedMods = TopologicalSort(_loadedMods);

        // Initialize phase
        foreach (var mod in sortedMods)
        {
            try
            {
                mod.OnInitialize();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error initializing mod '{mod.Info.ModId}': {ex.Message}");
            }
        }

        // Post-initialize phase
        foreach (var mod in sortedMods)
        {
            try
            {
                mod.OnPostInitialize();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in post-initialization of mod '{mod.Info.ModId}': {ex.Message}");
            }
        }

        _isInitialized = true;
        Logger.Info($"Initialized {sortedMods.Count} mods");
    }

    /// <summary>
    /// Update all mods
    /// </summary>
    public void UpdateAll(float deltaTime)
    {
        foreach (var mod in _loadedMods)
        {
            try
            {
                mod.OnUpdate(deltaTime);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating mod '{mod.Info.ModId}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Get a mod by its ID
    /// </summary>
    public Mod? GetMod(string modId)
    {
        return _modsByIdIndex.TryGetValue(modId, out var mod) ? mod : null;
    }

    /// <summary>
    /// Check if a mod is loaded
    /// </summary>
    public bool IsModLoaded(string modId)
    {
        return _modsByIdIndex.ContainsKey(modId);
    }

    /// <summary>
    /// Unload a specific mod
    /// </summary>
    public bool UnloadMod(string modId)
    {
        if (!_modsByIdIndex.TryGetValue(modId, out var mod))
            return false;

        try
        {
            mod.OnUnload();
            mod.IsLoaded = false;
            _loadedMods.Remove(mod);
            _modsByIdIndex.Remove(modId);

            Logger.Info($"Unloaded mod: {mod.Info}");
            OnModUnloaded?.Invoke(mod);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error unloading mod '{modId}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Unload all mods
    /// </summary>
    public void UnloadAll()
    {
        Logger.Info("Unloading all mods...");

        // Unload in reverse order
        for (int i = _loadedMods.Count - 1; i >= 0; i--)
        {
            var mod = _loadedMods[i];
            try
            {
                mod.OnUnload();
                mod.IsLoaded = false;
                OnModUnloaded?.Invoke(mod);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error unloading mod '{mod.Info.ModId}': {ex.Message}");
            }
        }

        _loadedMods.Clear();
        _modsByIdIndex.Clear();
        _isInitialized = false;

        Logger.Info("All mods unloaded");
    }

    /// <summary>
    /// Topological sort of mods based on dependencies
    /// </summary>
    private List<Mod> TopologicalSort(List<Mod> mods)
    {
        var sorted = new List<Mod>();
        var visited = new HashSet<string>();
        var temp = new HashSet<string>();

        void Visit(Mod mod)
        {
            if (temp.Contains(mod.Info.ModId))
                throw new InvalidOperationException($"Circular dependency detected involving mod '{mod.Info.ModId}'");

            if (visited.Contains(mod.Info.ModId))
                return;

            temp.Add(mod.Info.ModId);

            foreach (var depId in mod.Info.Dependencies)
            {
                if (_modsByIdIndex.TryGetValue(depId, out var depMod))
                {
                    Visit(depMod);
                }
            }

            temp.Remove(mod.Info.ModId);
            visited.Add(mod.Info.ModId);
            sorted.Add(mod);
        }

        foreach (var mod in mods)
        {
            if (!visited.Contains(mod.Info.ModId))
            {
                Visit(mod);
            }
        }

        return sorted;
    }
}
