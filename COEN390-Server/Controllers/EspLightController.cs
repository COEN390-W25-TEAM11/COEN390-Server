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

    [HttpPost("{lightId}")]
    public async Task<IActionResult> MovementUpdate(Guid lightId, bool movement) {

        var light = _DbContext.Lights.Find(lightId);

        if (light == null) {
            return NotFound();
        }

        _DbContext.Add(new Motion {
            Id = Guid.NewGuid(),
            DateTime = DateTime.Now,
            motion = movement,
            Light = light,
        });
        await _DbContext.SaveChangesAsync();

        if (movement) {
            await _NotificationService.SendToAll($"Light {light.Name} has movement");
        }

        return Ok();
    }

    [Route("ws/{lightId}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task UpdateSettings(Guid lightId) {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            _SettingsUpdateService.AddWebsocket(lightId, webSocket);
            await _SettingsUpdateService.Listen(lightId, webSocket);
        }
        else {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
