using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Infrastructure;

namespace COEN390_Server.Services;

public class SettingsUpdateService {

    private readonly IServiceProvider _serviceProvider;

    public SettingsUpdateService(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }


    private readonly ConcurrentDictionary<Guid, WebSocket> _activeConnections = new();

    public Guid AddWebsocket(WebSocket webSocket) {
        var guid = Guid.NewGuid();
        _activeConnections.TryAdd(guid, webSocket);
        return guid;
    }

    public async Task UpdateEsp(Guid espId) {

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        //throw new NotImplementedException();

        //var updateModel = ...

        //sendUpdate(updateModel);
    }

    private async Task sendUpdate(object update) {

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
}
