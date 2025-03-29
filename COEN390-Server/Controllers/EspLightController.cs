using COEN390_Server.Services;
using Infrastructure;
using Infrastructure.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COEN390_Server.Controllers;

// This controller is for the requests from the esp.

[ApiController]
[Route("[controller]")]
public class EspLightController : ControllerBase {

    private readonly MyDbContext _DbContext;
    private readonly NotificationService _NotificationService;
    private readonly SettingsUpdateService _SettingsUpdateService;

    public EspLightController(
        MyDbContext myDbContext,
        NotificationService notificationService,
        SettingsUpdateService settingsUpdateService
    ) {
        _DbContext = myDbContext;
        _NotificationService = notificationService;
        _SettingsUpdateService = settingsUpdateService;
    }

    [HttpPost("register/{espId}")]
    public async Task<IActionResult> Register(Guid espId, RegisterRequest request) {
        
        var lights = await _DbContext.Lights
            .Where(l => request.lightPins.Contains(l.Pin))
            .ToListAsync();

        var sensors = await _DbContext.Sensors
            .Where(s => request.sensorPins.Contains(s.Pin))
            .ToListAsync();
        
        // add sensors and lights that are not already registered
        foreach (var light in lights) {
            if (!lights.Any(l => l.Pin == light.Pin)) {
                _DbContext.Add(new Light {
                    Id = Guid.NewGuid(),
                    EspId = espId,
                    Pin = light.Pin,
                });
            }
        }
        foreach (var sensor in sensors) {
            if (!sensors.Any(s => s.Pin == sensor.Pin)) {
                _DbContext.Add(new Sensor {
                    Id = Guid.NewGuid(),
                    EspId = espId,
                    Pin = sensor.Pin,
                });
            }
        }

        return Ok();
    }

    [HttpPost("movement/{pin}")]
    public async Task<IActionResult> MovementUpdate(int pin, bool movement) {

        var sensor = await _DbContext.Sensors
            .FirstOrDefaultAsync(s => s.Pin == pin);

        if (sensor == null) {
            return NotFound();
        }

        _DbContext.Add(new Motion {
            Id = Guid.NewGuid(),
            DateTime = DateTime.Now,
            motion = movement,
            Sensor = sensor,
        });
        await _DbContext.SaveChangesAsync();

        if (movement) {
            await _NotificationService.SendToAll($"Light {sensor.Name} has movement");
        }

        return Ok();
    }

    [Route("ws")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task UpdateSettings() {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            _SettingsUpdateService.AddWebsocket(webSocket);
            await _SettingsUpdateService.Listen(webSocket);
        }
        else {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    public record RegisterRequest(
        int[] sensorPins,
        int[] lightPins
    );
}
