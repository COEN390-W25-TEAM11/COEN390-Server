using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace COEN390_Server.Services;

public class NotificationService {

    private readonly ConcurrentDictionary<Guid, WebSocket> _activeConnections = new();

    public Guid AddWebsocket(WebSocket webSocket) {
        var guid = Guid.NewGuid();
        _activeConnections.TryAdd(guid, webSocket);
        return guid;
    }

    public async Task SendMovement(Guid sensorId, DateTime dateTime, bool motion) {
        var model = new MessageModel {
            sensorId = sensorId,
            dateTime = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"),
            motion = motion
        };

        await SendToAll(JsonSerializer.Serialize(model));
    }

    private async Task SendToAll(string message) {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(messageBytes);

        foreach (var (id, socket) in _activeConnections.ToArray()) {
            if (socket.State == WebSocketState.Open) {
                await socket.SendAsync(
                    segment,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            else {
                _activeConnections.TryRemove(id, out _);
            }
        }
    }

    public async Task Listen(Guid guid, WebSocket webSocket) {
        var buffer = new byte[1024 * 4];

        try {
            while (webSocket.State == WebSocketState.Open) {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close) {
                    break;
                }
            }

            _activeConnections.TryRemove(guid, out _);
        }
        finally {
            _activeConnections.TryRemove(guid, out _);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }

    private class MessageModel {
        public required Guid sensorId { get; set; }
        public required string dateTime { get; set; }
        public required bool motion { get; set; }
    }
}
