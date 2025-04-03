using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using static COEN390_Server.Controllers.ApiController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace COEN390_Server.Services;

public class SettingsUpdateService {

    private readonly IServiceProvider _serviceProvider;

    public SettingsUpdateService(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    private readonly ConcurrentDictionary<Guid, List<Guid>> _connectionDictionary = new();
    private readonly ConcurrentDictionary<Guid, WebSocket> _activeConnections = new();

    public async Task AddWebsocket(Guid espId, WebSocket webSocket) {
        var connecitonId = Guid.NewGuid();

        if (!_connectionDictionary.TryGetValue(espId, out var list)) {
            list = new List<Guid> { connecitonId };
            _connectionDictionary.TryAdd(espId, list);
        }
        else {
            list.Add(connecitonId);
        }

        _activeConnections.TryAdd(connecitonId, webSocket);

        await Listen(espId, connecitonId, webSocket);
    }

    public async Task UpdateEsp(Guid espId) {

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var lights = dbContext.Lights.Where(l => l.EspId == espId).ToListAsync();
        var sensors = dbContext.Sensors.Where(s => s.EspId == espId).Include(s => s.MotionHistory).ToListAsync();
        var assigned = dbContext.Assigneds.Where(a => a.Light.EspId == espId && a.Sensor.EspId == espId).ToListAsync();

        var updateModel = new SettingUpdateModel {
            Lights = (await lights).Select(l => new SettingUpdateModel.SettingUpdateLightModel {
                Pin = l.Pin,
                Overide = l.Overide,
                State = l.State,
                Brightness = l.Brightness,
            }),
            Sensors = (await sensors).Select(s => new SettingUpdateModel.SettingUpdateSensorModel {
                Pin = s.Pin,
                Sensitivity = s.Sensitivity,
                Timeout = s.Timeout,
            }),
            Assigned = (await assigned).Select(a => new SettingUpdateModel.SettingUpdateAssignedModel {
                LightPin = a.Light.Pin,
                SensorPin = a.Sensor.Pin,
            })
        };

        await sendUpdate(espId, updateModel);
    }

    private async Task sendUpdate(Guid espId, object update) {

        var message = Newtonsoft.Json.JsonConvert.SerializeObject(update);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(messageBytes);


        if (!_connectionDictionary.TryGetValue(espId, out var connectionIdList)) {
            Console.WriteLine("Warning: No websocket connection found to relay the message");
            return;
        }

        foreach (Guid connectionId in connectionIdList!) {
            if (!_activeConnections.TryGetValue(connectionId, out WebSocket? webSocket)) {
                Console.WriteLine("Warning: Websocket not in active connection removing from dictionary");
                RemoveConnection(espId, connectionId);
            }

            if (webSocket!.State == WebSocketState.Open) {
                await webSocket.SendAsync(
                    segment,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            else {
                RemoveConnection(espId, connectionId);
            }

        }
    }

    private async Task Listen(Guid espId, Guid connectionId, WebSocket webSocket) {
        var buffer = new byte[1024 * 4];

        try {
            while (webSocket.State == WebSocketState.Open) {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close) {
                    break;
                }
            }

            RemoveConnection(espId, connectionId);
        }
        finally {
            RemoveConnection(espId, connectionId);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }

    private void RemoveConnection(Guid espId, Guid connectionId) {
        _activeConnections.TryRemove(connectionId, out _);

        if (_connectionDictionary.TryGetValue(espId, out var list)) {
            list.Remove(connectionId);
        }
    }

    private class SettingUpdateModel {
        public required IEnumerable<SettingUpdateLightModel> Lights { get; set; }
        public required IEnumerable<SettingUpdateSensorModel> Sensors { get; set; }
        public required IEnumerable<SettingUpdateAssignedModel> Assigned { get; set; }

        public class SettingUpdateLightModel {
            public required int Pin { get; set; }
            public required bool Overide { get; set; }
            public required int State { get; set; }
            public required int Brightness { get; set; }
        }

        public class SettingUpdateSensorModel {
            public required int Pin { get; set; }
            public required int Sensitivity { get; set; }
            public required int Timeout { get; set; }
        }

        public class SettingUpdateAssignedModel {
            public required int SensorPin { get; set; }
            public required int LightPin { get; set; }
        }
    }
}
