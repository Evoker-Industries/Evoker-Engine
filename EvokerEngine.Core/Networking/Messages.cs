using System;
using System.IO;
using System.Numerics;

namespace EvokerEngine.Networking;

/// <summary>
/// Connection request message
/// </summary>
public class ConnectMessage : NetworkMessage
{
    public override MessageType Type => MessageType.Connect;
    public string PlayerName { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        WriteString(writer, PlayerName);
        WriteString(writer, Version);
        return ms.ToArray();
    }

    public override void Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        PlayerName = ReadString(reader);
        Version = ReadString(reader);
    }
}

/// <summary>
/// Disconnection message
/// </summary>
public class DisconnectMessage : NetworkMessage
{
    public override MessageType Type => MessageType.Disconnect;
    public string Reason { get; set; } = string.Empty;

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        WriteString(writer, Reason);
        return ms.ToArray();
    }

    public override void Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        Reason = ReadString(reader);
    }
}

/// <summary>
/// Ping message for latency testing
/// </summary>
public class PingMessage : NetworkMessage
{
    public override MessageType Type => MessageType.Ping;
    public long Timestamp { get; set; }

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(Timestamp);
        return ms.ToArray();
    }

    public override void Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        Timestamp = reader.ReadInt64();
    }
}

/// <summary>
/// Pong response message
/// </summary>
public class PongMessage : NetworkMessage
{
    public override MessageType Type => MessageType.Pong;
    public long Timestamp { get; set; }

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(Timestamp);
        return ms.ToArray();
    }

    public override void Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        Timestamp = reader.ReadInt64();
    }
}

/// <summary>
/// Chat message
/// </summary>
public class ChatMessage : NetworkMessage
{
    public override MessageType Type => MessageType.ChatMessage;
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long Timestamp { get; set; }

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        WriteString(writer, Sender);
        WriteString(writer, Message);
        writer.Write(Timestamp);
        return ms.ToArray();
    }

    public override void Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        Sender = ReadString(reader);
        Message = ReadString(reader);
        Timestamp = reader.ReadInt64();
    }
}

/// <summary>
/// Entity update message for position and state synchronization
/// </summary>
public class EntityUpdateMessage : NetworkMessage
{
    public override MessageType Type => MessageType.EntityUpdate;
    public uint EntityId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Velocity { get; set; }

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(EntityId);
        writer.Write(Position.X);
        writer.Write(Position.Y);
        writer.Write(Position.Z);
        writer.Write(Rotation.X);
        writer.Write(Rotation.Y);
        writer.Write(Rotation.Z);
        writer.Write(Velocity.X);
        writer.Write(Velocity.Y);
        writer.Write(Velocity.Z);
        return ms.ToArray();
    }

    public override void Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        EntityId = reader.ReadUInt32();
        Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        Rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        Velocity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}

/// <summary>
/// Inventory synchronization message
/// </summary>
public class InventoryUpdateMessage : NetworkMessage
{
    public override MessageType Type => MessageType.InventoryUpdate;
    public uint EntityId { get; set; }
    public int SlotIndex { get; set; }
    public string ItemId { get; set; } = string.Empty; // namespace:key format
    public int Quantity { get; set; }

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(EntityId);
        writer.Write(SlotIndex);
        WriteString(writer, ItemId);
        writer.Write(Quantity);
        return ms.ToArray();
    }

    public override void Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        EntityId = reader.ReadUInt32();
        SlotIndex = reader.ReadInt32();
        ItemId = ReadString(reader);
        Quantity = reader.ReadInt32();
    }
}

/// <summary>
/// Dimension change message
/// </summary>
public class DimensionChangeMessage : NetworkMessage
{
    public override MessageType Type => MessageType.DimensionChange;
    public uint EntityId { get; set; }
    public string DimensionId { get; set; } = string.Empty; // namespace:key format
    public Vector3 Position { get; set; }

    public override byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(EntityId);
        WriteString(writer, DimensionId);
        writer.Write(Position.X);
        writer.Write(Position.Y);
        writer.Write(Position.Z);
        return ms.ToArray();
    }

    public override void Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        EntityId = reader.ReadUInt32();
        DimensionId = ReadString(reader);
        Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}
