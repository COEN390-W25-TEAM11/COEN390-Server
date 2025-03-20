using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace COEN390_Server.Services;

public class SettingsUpdateService {

    private readonly ConcurrentDictionary<Guid, WebSocket> _activeConnections = new();

    public void AddWebsocket(Guid lightId, WebSocket webSocket) {
        _activeConnections.TryAdd(lightId, webSocket);
    }

    public async Task sendUpdate(Guid lightId, object update) {
        var websocket = _activeConnections.FirstOrDefault(_activeConnections => _activeConnections.Key == lightId).Value;

        if (websocket == null) {
            Console.WriteLine($"WebSocket for lightId: {lightId} not found.");
            return; 
        }

        var message = Newtonsoft.Json.JsonConvert.SerializeObject(update);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(messageBytes);

        if (websocket.State == WebSocketState.Open) {
            await websocket.SendAsync(
                segment,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
        else {
            _activeConnections.TryRemove(lightId, out _);
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
