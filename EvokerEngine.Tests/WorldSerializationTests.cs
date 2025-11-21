using System;
using System.IO;
using System.Numerics;
using Xunit;
using EvokerEngine.Serialization;

namespace EvokerEngine.Tests;

public class WorldSerializationTests
{
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), "EvokerEngineTests");

    public WorldSerializationTests()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void WorldData_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var worldData = new WorldData();

        // Assert
        Assert.Equal("New World", worldData.Name);
        Assert.Equal(0, worldData.Seed);
        Assert.NotNull(worldData.Chunks);
        Assert.Empty(worldData.Chunks);
        Assert.NotNull(worldData.Metadata);
        Assert.Empty(worldData.Metadata);
    }

    [Fact]
    public void WorldSerializer_SaveAndLoad_ShouldPreserveData()
    {
        // Arrange
        var worldData = new WorldData
        {
            Name = "Test World",
            Seed = 12345,
            SpawnPosition = new Vector3Data(10, 20, 30),
            GameTime = 1000
        };
        worldData.Metadata["difficulty"] = "hard";
        worldData.Metadata["gamemode"] = "survival";

        var chunkData = new ChunkData { X = 0, Z = 0 };
        chunkData.Blocks.Add(new BlockData { X = 5, Y = 10, Z = 5, Type = 1 });
        worldData.Chunks.Add(chunkData);

        string filePath = Path.Combine(_testDirectory, "test_world.world");

        // Act - Save
        WorldSerializer.SaveWorld(worldData, filePath, compress: true);

        // Act - Load
        var loadedWorld = WorldSerializer.LoadWorld(filePath, compressed: true);

        // Assert
        Assert.NotNull(loadedWorld);
        Assert.Equal("Test World", loadedWorld.Name);
        Assert.Equal(12345, loadedWorld.Seed);
        Assert.Equal(10, loadedWorld.SpawnPosition.X);
        Assert.Equal(20, loadedWorld.SpawnPosition.Y);
        Assert.Equal(30, loadedWorld.SpawnPosition.Z);
        Assert.Equal(1000, loadedWorld.GameTime);
        Assert.Equal("hard", loadedWorld.Metadata["difficulty"]);
        Assert.Equal("survival", loadedWorld.Metadata["gamemode"]);
        Assert.Single(loadedWorld.Chunks);
        Assert.Single(loadedWorld.Chunks[0].Blocks);
        Assert.Equal(1, loadedWorld.Chunks[0].Blocks[0].Type);
    }

    [Fact]
    public void WorldSerializer_SaveUncompressed_ShouldWork()
    {
        // Arrange
        var worldData = new WorldData
        {
            Name = "Uncompressed World",
            Seed = 54321
        };

        string filePath = Path.Combine(_testDirectory, "uncompressed_world.world");

        // Act
        WorldSerializer.SaveWorld(worldData, filePath, compress: false);
        var loadedWorld = WorldSerializer.LoadWorld(filePath, compressed: false);

        // Assert
        Assert.NotNull(loadedWorld);
        Assert.Equal("Uncompressed World", loadedWorld.Name);
        Assert.Equal(54321, loadedWorld.Seed);
    }

    [Fact]
    public void WorldSerializer_SerializeToBytes_ShouldWork()
    {
        // Arrange
        var worldData = new WorldData
        {
            Name = "Byte World",
            Seed = 99999
        };

        // Act
        var bytes = WorldSerializer.SerializeWorldToBytes(worldData, compress: true);
        var loadedWorld = WorldSerializer.DeserializeWorldFromBytes(bytes, compressed: true);

        // Assert
        Assert.NotNull(loadedWorld);
        Assert.Equal("Byte World", loadedWorld.Name);
        Assert.Equal(99999, loadedWorld.Seed);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void WorldSerializer_LoadNonExistentFile_ShouldReturnNull()
    {
        // Arrange
        string filePath = Path.Combine(_testDirectory, "nonexistent.world");

        // Act
        var loadedWorld = WorldSerializer.LoadWorld(filePath);

        // Assert
        Assert.Null(loadedWorld);
    }

    [Fact]
    public void WorldSerializer_WorldExists_ShouldDetectFile()
    {
        // Arrange
        var worldData = new WorldData { Name = "Exists Test" };
        string filePath = Path.Combine(_testDirectory, "exists_test.world");
        WorldSerializer.SaveWorld(worldData, filePath);

        // Act & Assert
        Assert.True(WorldSerializer.WorldExists(filePath));
        Assert.False(WorldSerializer.WorldExists(Path.Combine(_testDirectory, "nonexistent.world")));
    }

    [Fact]
    public void WorldSerializer_DeleteWorld_ShouldRemoveFile()
    {
        // Arrange
        var worldData = new WorldData { Name = "Delete Test" };
        string filePath = Path.Combine(_testDirectory, "delete_test.world");
        WorldSerializer.SaveWorld(worldData, filePath);
        Assert.True(File.Exists(filePath));

        // Act
        WorldSerializer.DeleteWorld(filePath);

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void WorldSerializer_GetFileSize_ShouldReturnCorrectSize()
    {
        // Arrange
        var worldData = new WorldData { Name = "Size Test" };
        string filePath = Path.Combine(_testDirectory, "size_test.world");
        WorldSerializer.SaveWorld(worldData, filePath);

        // Act
        var size = WorldSerializer.GetWorldFileSize(filePath);

        // Assert
        Assert.True(size > 0);
    }

    [Fact]
    public void WorldSerializer_ListWorlds_ShouldFindWorldFiles()
    {
        // Arrange
        WorldSerializer.SaveWorld(new WorldData { Name = "World 1" }, Path.Combine(_testDirectory, "world1.world"));
        WorldSerializer.SaveWorld(new WorldData { Name = "World 2" }, Path.Combine(_testDirectory, "world2.world"));

        // Act
        var worlds = WorldSerializer.ListWorlds(_testDirectory);

        // Assert
        Assert.Equal(2, worlds.Count);
    }

    [Fact]
    public void WorldManager_ShouldManageWorlds()
    {
        // Arrange
        var manager = new WorldManager(_testDirectory);
        var worldData = new WorldData
        {
            Name = "Manager Test World",
            Seed = 11111
        };

        // Act - Save
        manager.SaveWorld(worldData, "manager_test");

        // Assert - Exists
        Assert.True(manager.WorldExists("manager_test"));

        // Act - Load
        var loadedWorld = manager.LoadWorld("manager_test");
        Assert.NotNull(loadedWorld);
        Assert.Equal("Manager Test World", loadedWorld.Name);

        // Act - List
        var worlds = manager.ListWorlds();
        Assert.Contains("manager_test", worlds);

        // Act - Delete
        manager.DeleteWorld("manager_test");
        Assert.False(manager.WorldExists("manager_test"));
    }

    [Fact]
    public void WorldManager_CreateNewWorld_ShouldGenerateWorldData()
    {
        // Arrange
        var manager = new WorldManager(_testDirectory);

        // Act
        var worldData = manager.CreateNewWorld("New World", seed: 42);

        // Assert
        Assert.Equal("New World", worldData.Name);
        Assert.Equal(42, worldData.Seed);
        Assert.Equal(0, worldData.SpawnPosition.X);
        Assert.Equal(64, worldData.SpawnPosition.Y);
        Assert.Equal(0, worldData.SpawnPosition.Z);
    }

    [Fact]
    public void WorldManager_GetMetadata_ShouldReturnMetadataWithoutFullLoad()
    {
        // Arrange
        var manager = new WorldManager(_testDirectory);
        var worldData = manager.CreateNewWorld("Metadata Test", seed: 777);
        manager.SaveWorld(worldData, "metadata_test");

        // Act
        var metadata = manager.GetWorldMetadata("metadata_test");

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("Metadata Test", metadata.Name);
        Assert.Equal(777, metadata.Seed);
        Assert.True(metadata.FileSize > 0);
    }

    [Fact]
    public void Vector3Data_ShouldConvertToAndFromVector3()
    {
        // Arrange
        var vector3 = new Vector3(1.5f, 2.5f, 3.5f);

        // Act
        var vector3Data = new Vector3Data(vector3);
        var convertedBack = vector3Data.ToVector3();

        // Assert
        Assert.Equal(1.5f, vector3Data.X);
        Assert.Equal(2.5f, vector3Data.Y);
        Assert.Equal(3.5f, vector3Data.Z);
        Assert.Equal(vector3, convertedBack);
    }

    [Fact]
    public void PlayerData_ShouldSerializeCorrectly()
    {
        // Arrange
        var worldData = new WorldData
        {
            Name = "Player Test",
            Player = new PlayerData
            {
                Name = "TestPlayer",
                Position = new Vector3Data(1, 2, 3),
                Rotation = new Vector3Data(0, 90, 0),
                Health = 75f
            }
        };

        string filePath = Path.Combine(_testDirectory, "player_test.world");

        // Act
        WorldSerializer.SaveWorld(worldData, filePath);
        var loadedWorld = WorldSerializer.LoadWorld(filePath);

        // Assert
        Assert.NotNull(loadedWorld);
        Assert.NotNull(loadedWorld.Player);
        Assert.Equal("TestPlayer", loadedWorld.Player.Name);
        Assert.Equal(1f, loadedWorld.Player.Position.X);
        Assert.Equal(75f, loadedWorld.Player.Health);
    }

    [Fact]
    public void ChunkData_WithMultipleBlocks_ShouldSerialize()
    {
        // Arrange
        var worldData = new WorldData { Name = "Multi Block Test" };
        var chunk = new ChunkData { X = 5, Z = 10 };
        
        for (int i = 0; i < 10; i++)
        {
            chunk.Blocks.Add(new BlockData
            {
                X = i,
                Y = i * 2,
                Z = i * 3,
                Type = i % 5
            });
        }
        
        worldData.Chunks.Add(chunk);
        string filePath = Path.Combine(_testDirectory, "multi_block_test.world");

        // Act
        WorldSerializer.SaveWorld(worldData, filePath);
        var loadedWorld = WorldSerializer.LoadWorld(filePath);

        // Assert
        Assert.NotNull(loadedWorld);
        Assert.Single(loadedWorld.Chunks);
        Assert.Equal(10, loadedWorld.Chunks[0].Blocks.Count);
        Assert.Equal(5, loadedWorld.Chunks[0].X);
        Assert.Equal(10, loadedWorld.Chunks[0].Z);
    }

    [Fact]
    public void WorldMetadata_GetFileSizeFormatted_ShouldFormatCorrectly()
    {
        // Arrange & Act
        var metadata = new WorldMetadata
        {
            FileSize = 512
        };
        var formatted512 = metadata.GetFileSizeFormatted();

        metadata.FileSize = 2048;
        var formatted2KB = metadata.GetFileSizeFormatted();

        metadata.FileSize = 1024 * 1024 * 5; // 5 MB
        var formatted5MB = metadata.GetFileSizeFormatted();

        // Assert
        Assert.Contains("512 B", formatted512);
        Assert.Contains("2.00 KB", formatted2KB);
        Assert.Contains("5.00 MB", formatted5MB);
    }
}
