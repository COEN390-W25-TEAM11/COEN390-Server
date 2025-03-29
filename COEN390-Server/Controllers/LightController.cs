using COEN390_Server.Services;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace COEN390_Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class LightController : ControllerBase {

    private readonly MyDbContext _DbContext;
    private readonly SettingsUpdateService _settingsUpdateService;

    public LightController(
        MyDbContext myDbContext,
        SettingsUpdateService settingsUpdateService
    ) {
        _DbContext = myDbContext;
        _settingsUpdateService = settingsUpdateService;
    }



    [HttpPatch("{lightId}")]
    [ActionName("PatchLight")]
    public async Task<IActionResult> PatchLight(Guid lightId, LightUpdateModel lightUpdateDto) {

        var light = await _DbContext.Lights.FindAsync(lightId);

        if (light is null) {
            return NotFound();
        }

        light.Name = lightUpdateDto.Name;
        light.Overide = lightUpdateDto.Overide;
        light.State = lightUpdateDto.State;

        await _DbContext.SaveChangesAsync();

        await _settingsUpdateService.sendUpdate(light.EspId);

        return Ok();
    }

    [HttpPatch("{sensorId}")]
    [ActionName("PatchSensor")]
    public async Task<IActionResult> PatchSensor(Guid sensorId, SensorUpdateModel sensorUpdateDto) {

        var sensor = await _DbContext.Sensors.FindAsync(sensorId);

        if (sensor is null) {
            return NotFound();
        }

        sensor.Name = sensorUpdateDto.Name;
        sensor.Sensitivity = sensorUpdateDto.Sensitivity;
        sensor.Timeout = sensorUpdateDto.Timeout;

        await _DbContext.SaveChangesAsync();

        await _settingsUpdateService.sendUpdate(sensor.EspId);

        return Ok();
    }

    public record LightDto(string Id, string Name, bool Overide, int State);
    public record LightUpdateModel(string Name, bool Overide, int State);
    public record SensorUpdateModel(string Name, int Sensitivity, int Timeout);
}
