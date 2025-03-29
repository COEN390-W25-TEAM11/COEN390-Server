using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace COEN390_Server.Services;

public class NotificationService {

    private readonly ConcurrentDictionary<Guid, WebSocket> _activeConnections = new();

    public Guid AddWebsocket(WebSocket webSocket) {
        var connectionId = Guid.NewGuid();
        _activeConnections.TryAdd(connectionId, webSocket);
        return connectionId;
    }

    public async Task SendToAll(string message) {
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

    public async Task Listen(Guid wsGuid, WebSocket webSocket) {
        var buffer = new byte[1024 * 4];

        try {
            while (webSocket.State == WebSocketState.Open) {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close) {
                    break;
                }
            }
        }
        finally {
            _activeConnections.TryRemove(wsGuid, out _);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }
}
