using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace COEN390_Server.Services;

public class SettingsUpdateService {

    private readonly ConcurrentDictionary<Guid, WebSocket> _activeConnections = new();

    public void AddWebsocket(WebSocket webSocket) {
        _activeConnections.TryAdd(Guid.NewGuid(), webSocket);
    }

    public async Task sendUpdate(object update) {

        var message = Newtonsoft.Json.JsonConvert.SerializeObject(update);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(messageBytes);

        foreach (var connection in _activeConnections) {
        if (connection.Value.State == WebSocketState.Open) {
            await connection.Value.SendAsync(
                segment,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
        else {
            _activeConnections.TryRemove(connection.Key, out _);
        }
        }
    }

    public async Task Listen(WebSocket webSocket) {
        var buffer = new byte[1024 * 4];
        

        try {
            while (webSocket.State == WebSocketState.Open) {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close) {
                    break;
                }
            }

            _activeConnections.TryRemove(id, out _);
        }
        finally {
            _activeConnections.TryRemove(id, out _);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }
}
