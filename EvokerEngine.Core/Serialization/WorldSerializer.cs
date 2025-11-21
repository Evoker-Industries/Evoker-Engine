using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Numerics;
using System.IO.Compression;

namespace EvokerEngine.Serialization;

/// <summary>
/// World data container for serialization
/// </summary>
public class WorldData
{
    /// <summary>
    /// World name
    /// </summary>
    public string Name { get; set; } = "New World";
    
    /// <summary>
    /// World seed for generation
    /// </summary>
    public int Seed { get; set; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last saved timestamp
    /// </summary>
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// World version
    /// </summary>
    public string Version { get; set; } = "1.0.0";
    
    /// <summary>
    /// Spawn position
    /// </summary>
    public Vector3Data SpawnPosition { get; set; } = new Vector3Data();
    
    /// <summary>
    /// Game time (in ticks)
    /// </summary>
    public long GameTime { get; set; }
    
    /// <summary>
    /// Custom metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    
    /// <summary>
    /// List of chunks
    /// </summary>
    public List<ChunkData> Chunks { get; set; } = new List<ChunkData>();
    
    /// <summary>
    /// Player data
    /// </summary>
    public PlayerData? Player { get; set; }
}

/// <summary>
/// Chunk data for serialization
/// </summary>
public class ChunkData
{
    /// <summary>
    /// Chunk X coordinate
    /// </summary>
    public int X { get; set; }
    
    /// <summary>
    /// Chunk Z coordinate
    /// </summary>
    public int Z { get; set; }
    
    /// <summary>
    /// Block data (compressed)
    /// </summary>
    public List<BlockData> Blocks { get; set; } = new List<BlockData>();
}

/// <summary>
/// Block data for serialization
/// </summary>
public class BlockData
{
    /// <summary>
    /// Block X position (relative to chunk)
    /// </summary>
    public int X { get; set; }
    
    /// <summary>
    /// Block Y position
    /// </summary>
    public int Y { get; set; }
    
    /// <summary>
    /// Block Z position (relative to chunk)
    /// </summary>
    public int Z { get; set; }
    
    /// <summary>
    /// Block type/ID
    /// </summary>
    public int Type { get; set; }
    
    /// <summary>
    /// Block metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Player data for serialization
/// </summary>
public class PlayerData
{
    /// <summary>
    /// Player name
    /// </summary>
    public string Name { get; set; } = "Player";
    
    /// <summary>
    /// Player position
    /// </summary>
    public Vector3Data Position { get; set; } = new Vector3Data();
    
    /// <summary>
    /// Player rotation
    /// </summary>
    public Vector3Data Rotation { get; set; } = new Vector3Data();
    
    /// <summary>
    /// Player health
    /// </summary>
    public float Health { get; set; } = 100f;
    
    /// <summary>
    /// Player inventory
    /// </summary>
    public List<ItemData>? Inventory { get; set; }
    
    /// <summary>
    /// Custom player data
    /// </summary>
    public Dictionary<string, string>? CustomData { get; set; }
}

/// <summary>
/// Item data for serialization
/// </summary>
public class ItemData
{
    /// <summary>
    /// Item type/ID
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Stack count
    /// </summary>
    public int Count { get; set; } = 1;
    
    /// <summary>
    /// Item metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Vector3 data for serialization (System.Numerics.Vector3 is not serializable by default)
/// </summary>
public class Vector3Data
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    
    public Vector3Data() { }
    
    public Vector3Data(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public Vector3Data(Vector3 vector)
    {
        X = vector.X;
        Y = vector.Y;
        Z = vector.Z;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }
}

/// <summary>
/// World serializer for saving and loading worlds
/// </summary>
public class WorldSerializer
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    /// <summary>
    /// Save world to file
    /// </summary>
    /// <param name="worldData">World data to save</param>
    /// <param name="filePath">Path to save file</param>
    /// <param name="compress">Whether to compress the data</param>
    public static void SaveWorld(WorldData worldData, string filePath, bool compress = true)
    {
        try
        {
            worldData.LastSaved = DateTime.UtcNow;
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string json = JsonSerializer.Serialize(worldData, _jsonOptions);
            
            if (compress)
            {
                // Save compressed
                using var fileStream = File.Create(filePath);
                using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
                using var writer = new StreamWriter(gzipStream);
                writer.Write(json);
            }
            else
            {
                // Save as plain JSON
                File.WriteAllText(filePath, json);
            }
            
            Core.Logger.Info($"World saved to: {filePath}");
        }
        catch (Exception ex)
        {
            Core.Logger.Error($"Failed to save world: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Load world from file
    /// </summary>
    /// <param name="filePath">Path to world file</param>
    /// <param name="compressed">Whether the file is compressed</param>
    /// <returns>Loaded world data</returns>
    public static WorldData? LoadWorld(string filePath, bool compressed = true)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Core.Logger.Error($"World file not found: {filePath}");
                return null;
            }
            
            string json;
            
            if (compressed)
            {
                // Load compressed
                using var fileStream = File.OpenRead(filePath);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream);
                json = reader.ReadToEnd();
            }
            else
            {
                // Load plain JSON
                json = File.ReadAllText(filePath);
            }
            
            var worldData = JsonSerializer.Deserialize<WorldData>(json, _jsonOptions);
            
            Core.Logger.Info($"World loaded from: {filePath}");
            return worldData;
        }
        catch (Exception ex)
        {
            Core.Logger.Error($"Failed to load world: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Save world to byte array (for network transmission or custom storage)
    /// </summary>
    public static byte[] SerializeWorldToBytes(WorldData worldData, bool compress = true)
    {
        string json = JsonSerializer.Serialize(worldData, _jsonOptions);
        
        if (compress)
        {
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
            using (var writer = new StreamWriter(gzipStream))
            {
                writer.Write(json);
            }
            return memoryStream.ToArray();
        }
        else
        {
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
    
    /// <summary>
    /// Deserialize world from byte array
    /// </summary>
    public static WorldData? DeserializeWorldFromBytes(byte[] data, bool compressed = true)
    {
        string json;
        
        if (compressed)
        {
            using var memoryStream = new MemoryStream(data);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream);
            json = reader.ReadToEnd();
        }
        else
        {
            json = System.Text.Encoding.UTF8.GetString(data);
        }
        
        return JsonSerializer.Deserialize<WorldData>(json, _jsonOptions);
    }
    
    /// <summary>
    /// Check if a world file exists
    /// </summary>
    public static bool WorldExists(string filePath)
    {
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// Delete a world file
    /// </summary>
    public static void DeleteWorld(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Core.Logger.Info($"World deleted: {filePath}");
        }
    }
    
    /// <summary>
    /// Get world file size in bytes
    /// </summary>
    public static long GetWorldFileSize(string filePath)
    {
        if (File.Exists(filePath))
        {
            return new FileInfo(filePath).Length;
        }
        return 0;
    }
    
    /// <summary>
    /// List all world files in a directory
    /// </summary>
    public static List<string> ListWorlds(string directory, string pattern = "*.world")
    {
        if (!Directory.Exists(directory))
        {
            return new List<string>();
        }
        
        var files = Directory.GetFiles(directory, pattern);
        return new List<string>(files);
    }
}

/// <summary>
/// Helper class for world management
/// </summary>
public class WorldManager
{
    private string _worldsDirectory;
    
    public WorldManager(string worldsDirectory = "./worlds")
    {
        _worldsDirectory = worldsDirectory;
        
        // Ensure directory exists
        if (!Directory.Exists(_worldsDirectory))
        {
            Directory.CreateDirectory(_worldsDirectory);
        }
    }
    
    /// <summary>
    /// Save a world with a given name
    /// </summary>
    public void SaveWorld(WorldData worldData, string worldName)
    {
        string fileName = SanitizeFileName(worldName) + ".world";
        string filePath = Path.Combine(_worldsDirectory, fileName);
        WorldSerializer.SaveWorld(worldData, filePath, compress: true);
    }
    
    /// <summary>
    /// Load a world by name
    /// </summary>
    public WorldData? LoadWorld(string worldName)
    {
        string fileName = SanitizeFileName(worldName) + ".world";
        string filePath = Path.Combine(_worldsDirectory, fileName);
        return WorldSerializer.LoadWorld(filePath, compressed: true);
    }
    
    /// <summary>
    /// Check if a world exists
    /// </summary>
    public bool WorldExists(string worldName)
    {
        string fileName = SanitizeFileName(worldName) + ".world";
        string filePath = Path.Combine(_worldsDirectory, fileName);
        return WorldSerializer.WorldExists(filePath);
    }
    
    /// <summary>
    /// Delete a world
    /// </summary>
    public void DeleteWorld(string worldName)
    {
        string fileName = SanitizeFileName(worldName) + ".world";
        string filePath = Path.Combine(_worldsDirectory, fileName);
        WorldSerializer.DeleteWorld(filePath);
    }
    
    /// <summary>
    /// List all available worlds
    /// </summary>
    public List<string> ListWorlds()
    {
        var files = WorldSerializer.ListWorlds(_worldsDirectory);
        var worldNames = new List<string>();
        
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            worldNames.Add(name);
        }
        
        return worldNames;
    }
    
    /// <summary>
    /// Get world metadata without loading full world
    /// </summary>
    public WorldMetadata? GetWorldMetadata(string worldName)
    {
        string fileName = SanitizeFileName(worldName) + ".world";
        string filePath = Path.Combine(_worldsDirectory, fileName);
        
        if (!File.Exists(filePath))
            return null;
        
        try
        {
            var worldData = WorldSerializer.LoadWorld(filePath, compressed: true);
            if (worldData == null)
                return null;
            
            return new WorldMetadata
            {
                Name = worldData.Name,
                Seed = worldData.Seed,
                CreatedAt = worldData.CreatedAt,
                LastSaved = worldData.LastSaved,
                Version = worldData.Version,
                FileSize = WorldSerializer.GetWorldFileSize(filePath)
            };
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Create a new world with default settings
    /// </summary>
    public WorldData CreateNewWorld(string worldName, int? seed = null)
    {
        var worldData = new WorldData
        {
            Name = worldName,
            Seed = seed ?? new Random().Next(),
            CreatedAt = DateTime.UtcNow,
            LastSaved = DateTime.UtcNow,
            SpawnPosition = new Vector3Data(0, 64, 0)
        };
        
        return worldData;
    }
    
    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized;
    }
}

/// <summary>
/// World metadata for quick world info without full load
/// </summary>
public class WorldMetadata
{
    public string Name { get; set; } = string.Empty;
    public int Seed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSaved { get; set; }
    public string Version { get; set; } = string.Empty;
    public long FileSize { get; set; }
    
    public string GetFileSizeFormatted()
    {
        if (FileSize < 1024)
            return $"{FileSize} B";
        else if (FileSize < 1024 * 1024)
            return $"{FileSize / 1024.0:F2} KB";
        else if (FileSize < 1024 * 1024 * 1024)
            return $"{FileSize / (1024.0 * 1024.0):F2} MB";
        else
            return $"{FileSize / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}
