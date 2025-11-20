using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EvokerEngine.Core;

namespace EvokerEngine.Networking;

/// <summary>
/// Represents a network connection between client and server
/// </summary>
public class NetworkConnection : IDisposable
{
    private readonly TcpClient _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _receiveTask;

    /// <summary>
    /// Unique connection ID
    /// </summary>
    public Guid ConnectionId { get; }

    /// <summary>
    /// Remote endpoint
    /// </summary>
    public IPEndPoint? RemoteEndPoint { get; private set; }

    /// <summary>
    /// Whether the connection is active
    /// </summary>
    public bool IsConnected => _client?.Connected ?? false;

    /// <summary>
    /// Event fired when a message is received
    /// </summary>
    public event Action<NetworkMessage>? OnMessageReceived;

    /// <summary>
    /// Event fired when the connection is closed
    /// </summary>
    public event Action<NetworkConnection>? OnDisconnected;

    public NetworkConnection(TcpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        ConnectionId = Guid.NewGuid();
        RemoteEndPoint = _client.Client.RemoteEndPoint as IPEndPoint;
    }

    /// <summary>
    /// Start receiving messages
    /// </summary>
    public void StartReceiving()
    {
        if (_stream != null)
            return;

        _stream = _client.GetStream();
        _cancellationTokenSource = new CancellationTokenSource();
        _receiveTask = Task.Run(() => ReceiveLoop(_cancellationTokenSource.Token));
    }

    /// <summary>
    /// Send a message
    /// </summary>
    public async Task SendAsync(NetworkMessage message)
    {
        if (_stream == null || !IsConnected)
            throw new InvalidOperationException("Connection is not active");

        try
        {
            var data = message.Serialize();
            var header = BitConverter.GetBytes(data.Length);
            var typeHeader = new byte[] { (byte)message.Type };

            await _stream.WriteAsync(header, 0, header.Length);
            await _stream.WriteAsync(typeHeader, 0, typeHeader.Length);
            await _stream.WriteAsync(data, 0, data.Length);
            await _stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to send message: {ex.Message}");
            Close();
        }
    }

    /// <summary>
    /// Receive messages in a loop
    /// </summary>
    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        if (_stream == null)
            return;

        var headerBuffer = new byte[4];
        var typeBuffer = new byte[1];

        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                // Read message length
                var bytesRead = await _stream.ReadAsync(headerBuffer, 0, 4, cancellationToken);
                if (bytesRead != 4)
                    break;

                var messageLength = BitConverter.ToInt32(headerBuffer, 0);
                if (messageLength <= 0 || messageLength > 10485760) // 10 MB max
                {
                    Logger.Error($"Invalid message length: {messageLength}");
                    break;
                }

                // Read message type
                bytesRead = await _stream.ReadAsync(typeBuffer, 0, 1, cancellationToken);
                if (bytesRead != 1)
                    break;

                var messageType = (MessageType)typeBuffer[0];

                // Read message data
                var dataBuffer = new byte[messageLength];
                var totalRead = 0;
                while (totalRead < messageLength)
                {
                    bytesRead = await _stream.ReadAsync(dataBuffer, totalRead, 
                        messageLength - totalRead, cancellationToken);
                    if (bytesRead == 0)
                        break;
                    totalRead += bytesRead;
                }

                if (totalRead != messageLength)
                    break;

                // Deserialize and invoke event
                var message = CreateMessage(messageType);
                if (message != null)
                {
                    message.Deserialize(dataBuffer);
                    OnMessageReceived?.Invoke(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in receive loop: {ex.Message}");
        }
        finally
        {
            Close();
        }
    }

    /// <summary>
    /// Create a message instance based on type
    /// </summary>
    private NetworkMessage? CreateMessage(MessageType type)
    {
        return type switch
        {
            MessageType.Connect => new ConnectMessage(),
            MessageType.Disconnect => new DisconnectMessage(),
            MessageType.Ping => new PingMessage(),
            MessageType.Pong => new PongMessage(),
            MessageType.ChatMessage => new ChatMessage(),
            MessageType.EntityUpdate => new EntityUpdateMessage(),
            MessageType.InventoryUpdate => new InventoryUpdateMessage(),
            MessageType.DimensionChange => new DimensionChangeMessage(),
            _ => null
        };
    }

    /// <summary>
    /// Close the connection
    /// </summary>
    public void Close()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        _stream?.Close();
        _stream = null;

        _client?.Close();

        OnDisconnected?.Invoke(this);
    }

    public void Dispose()
    {
        Close();
    }
}
