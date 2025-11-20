using System;
using System.IO;
using System.Text;

namespace EvokerEngine.Networking;

/// <summary>
/// Base class for all network messages
/// </summary>
public abstract class NetworkMessage
{
    /// <summary>
    /// Unique message type identifier
    /// </summary>
    public abstract MessageType Type { get; }

    /// <summary>
    /// Serialize the message to bytes
    /// </summary>
    public abstract byte[] Serialize();

    /// <summary>
    /// Deserialize bytes into a message
    /// </summary>
    public abstract void Deserialize(byte[] data);

    /// <summary>
    /// Helper to write string to stream
    /// </summary>
    protected static void WriteString(BinaryWriter writer, string value)
    {
        writer.Write(value ?? string.Empty);
    }

    /// <summary>
    /// Helper to read string from stream
    /// </summary>
    protected static string ReadString(BinaryReader reader)
    {
        return reader.ReadString();
    }
}

/// <summary>
/// Message types
/// </summary>
public enum MessageType : byte
{
    Connect = 0,
    Disconnect = 1,
    Ping = 2,
    Pong = 3,
    PlayerJoin = 10,
    PlayerLeave = 11,
    EntityUpdate = 20,
    EntityCreate = 21,
    EntityDestroy = 22,
    InventoryUpdate = 30,
    ItemAdd = 31,
    ItemRemove = 32,
    DimensionChange = 40,
    DimensionSync = 41,
    ChatMessage = 50,
    Custom = 255
}
