using COEN390_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace COEN390_Server.Controllers;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class NotificationController : ControllerBase {

    private readonly NotificationService _NotificationService;

    public NotificationController(
        NotificationService notificationService
    ) {
        _NotificationService = notificationService;
    }

    [Route("ws")]
    [Authorize]
    public async Task Get() {

        if (HttpContext.WebSockets.IsWebSocketRequest) {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var wsGuid = _NotificationService.AddWebsocket(webSocket);
            await _NotificationService.Listen(wsGuid, webSocket);
        }
        else {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
