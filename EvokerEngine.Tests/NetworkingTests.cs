using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using EvokerEngine.Networking;
using EvokerEngine.Core;

namespace EvokerEngine.Tests;

public class NetworkingTests
{
    [Fact]
    public void ResourceKey_SupportsColonNotation()
    {
        // Test that items, dimensions, and inventories use namespace:key format
        var itemKey = new ResourceKey("game", "sword");
        Assert.Equal("game:sword", itemKey.FullKey);
        Assert.Equal("game", itemKey.Namespace);
        Assert.Equal("sword", itemKey.Key);
    }

    [Fact]
    public void ResourceKey_ParsesColonNotation()
    {
        var key = ResourceKey.Parse("minecraft:stone");
        Assert.Equal("minecraft", key.Namespace);
        Assert.Equal("stone", key.Key);
    }

    [Fact]
    public void NetworkMessage_ConnectMessage_SerializesCorrectly()
    {
        // Arrange
        var message = new ConnectMessage
        {
            PlayerName = "TestPlayer",
            Version = "1.0.0"
        };

        // Act
        var data = message.Serialize();
        var deserialized = new ConnectMessage();
        deserialized.Deserialize(data);

        // Assert
        Assert.Equal(MessageType.Connect, message.Type);
        Assert.Equal("TestPlayer", deserialized.PlayerName);
        Assert.Equal("1.0.0", deserialized.Version);
    }

    [Fact]
    public void NetworkMessage_DisconnectMessage_SerializesCorrectly()
    {
        // Arrange
        var message = new DisconnectMessage
        {
            Reason = "Connection timeout"
        };

        // Act
        var data = message.Serialize();
        var deserialized = new DisconnectMessage();
        deserialized.Deserialize(data);

        // Assert
        Assert.Equal(MessageType.Disconnect, message.Type);
        Assert.Equal("Connection timeout", deserialized.Reason);
    }

    [Fact]
    public void NetworkMessage_PingMessage_SerializesCorrectly()
    {
        // Arrange
        var message = new PingMessage
        {
            Timestamp = 1234567890
        };

        // Act
        var data = message.Serialize();
        var deserialized = new PingMessage();
        deserialized.Deserialize(data);

        // Assert
        Assert.Equal(MessageType.Ping, message.Type);
        Assert.Equal(1234567890, deserialized.Timestamp);
    }

    [Fact]
    public void NetworkMessage_ChatMessage_SerializesCorrectly()
    {
        // Arrange
        var message = new ChatMessage
        {
            Sender = "Player1",
            Message = "Hello, World!",
            Timestamp = 1234567890
        };

        // Act
        var data = message.Serialize();
        var deserialized = new ChatMessage();
        deserialized.Deserialize(data);

        // Assert
        Assert.Equal(MessageType.ChatMessage, message.Type);
        Assert.Equal("Player1", deserialized.Sender);
        Assert.Equal("Hello, World!", deserialized.Message);
        Assert.Equal(1234567890, deserialized.Timestamp);
    }

    [Fact]
    public void NetworkMessage_EntityUpdateMessage_SerializesCorrectly()
    {
        // Arrange
        var message = new EntityUpdateMessage
        {
            EntityId = 123,
            Position = new System.Numerics.Vector3(1.0f, 2.0f, 3.0f),
            Rotation = new System.Numerics.Vector3(0.0f, 90.0f, 0.0f),
            Velocity = new System.Numerics.Vector3(0.5f, 0.0f, 0.5f)
        };

        // Act
        var data = message.Serialize();
        var deserialized = new EntityUpdateMessage();
        deserialized.Deserialize(data);

        // Assert
        Assert.Equal(MessageType.EntityUpdate, message.Type);
        Assert.Equal(123u, deserialized.EntityId);
        Assert.Equal(1.0f, deserialized.Position.X, 3);
        Assert.Equal(2.0f, deserialized.Position.Y, 3);
        Assert.Equal(3.0f, deserialized.Position.Z, 3);
    }

    [Fact]
    public void NetworkMessage_InventoryUpdateMessage_SerializesCorrectly()
    {
        // Arrange
        var message = new InventoryUpdateMessage
        {
            EntityId = 456,
            SlotIndex = 5,
            ItemId = "game:diamond_sword",
            Quantity = 1
        };

        // Act
        var data = message.Serialize();
        var deserialized = new InventoryUpdateMessage();
        deserialized.Deserialize(data);

        // Assert
        Assert.Equal(MessageType.InventoryUpdate, message.Type);
        Assert.Equal(456u, deserialized.EntityId);
        Assert.Equal(5, deserialized.SlotIndex);
        Assert.Equal("game:diamond_sword", deserialized.ItemId);
        Assert.Equal(1, deserialized.Quantity);
    }

    [Fact]
    public void NetworkMessage_DimensionChangeMessage_SerializesCorrectly()
    {
        // Arrange
        var message = new DimensionChangeMessage
        {
            EntityId = 789,
            DimensionId = "evoker:nether",
            Position = new System.Numerics.Vector3(100.0f, 64.0f, 200.0f)
        };

        // Act
        var data = message.Serialize();
        var deserialized = new DimensionChangeMessage();
        deserialized.Deserialize(data);

        // Assert
        Assert.Equal(MessageType.DimensionChange, message.Type);
        Assert.Equal(789u, deserialized.EntityId);
        Assert.Equal("evoker:nether", deserialized.DimensionId);
        Assert.Equal(100.0f, deserialized.Position.X, 3);
        Assert.Equal(64.0f, deserialized.Position.Y, 3);
        Assert.Equal(200.0f, deserialized.Position.Z, 3);
    }

    [Fact]
    public void NetworkServer_CanBeCreated()
    {
        // Arrange & Act
        using var server = new NetworkServer(7778);

        // Assert
        Assert.Equal(7778, server.Port);
        Assert.False(server.IsRunning);
        Assert.Equal(0, server.ClientCount);
    }

    [Fact]
    public void NetworkClient_CanBeCreated()
    {
        // Arrange & Act
        using var client = new NetworkClient("127.0.0.1", 7779);

        // Assert
        Assert.Equal("127.0.0.1", client.Host);
        Assert.Equal(7779, client.Port);
        Assert.False(client.IsConnected);
    }

    [Fact]
    public void NetworkingSystem_CanBeCreated()
    {
        // Arrange & Act
        var system = new NetworkingSystem();

        // Assert
        Assert.False(system.IsServer);
        Assert.False(system.IsClient);
        Assert.Null(system.Server);
        Assert.Null(system.Client);
    }

    [Fact]
    public async Task NetworkServer_CanStartAndStop()
    {
        // Arrange
        using var server = new NetworkServer(7780);

        // Act
        server.Start();
        Assert.True(server.IsRunning);
        
        await Task.Delay(100); // Give it time to start
        
        server.Stop();

        // Assert
        Assert.False(server.IsRunning);
    }

    [Fact]
    public void NetworkingSystem_CanStartAsServer()
    {
        // Arrange
        var system = new NetworkingSystem();

        try
        {
            // Act
            system.StartServer(7781);

            // Assert
            Assert.True(system.IsServer);
            Assert.NotNull(system.Server);
            Assert.True(system.Server.IsRunning);
        }
        finally
        {
            // Cleanup
            system.Shutdown();
        }
    }

    [Fact]
    public void NetworkingSystem_CanShutdown()
    {
        // Arrange
        var system = new NetworkingSystem();
        system.StartServer(7782);

        // Act
        system.Shutdown();

        // Assert
        Assert.False(system.IsServer);
        Assert.Null(system.Server);
    }
}
