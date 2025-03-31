using COEN390_Server.Services;
using Infrastructure;
using Infrastructure.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COEN390_Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase {

    private readonly MyDbContext _DbContext;
    private readonly SettingsUpdateService _settingsUpdateService;

    public ApiController(
        MyDbContext myDbContext,
        SettingsUpdateService settingsUpdateService
    ) {
        _DbContext = myDbContext;
        _settingsUpdateService = settingsUpdateService;
    }

    [HttpGet]
    [ActionName("Get")]
    public async Task<ResponseModel> Get() {
        var lights = _DbContext.Lights.ToListAsync();
        var sensors = _DbContext.Sensors.ToListAsync();
        var assigned = _DbContext.Assigneds.ToListAsync();

        return new ResponseModel(
            (await lights).Select(l => (l.Id, l.Name, l.Overide, l.State, l.Brightness)).ToArray(),
            (await sensors).Select(s => (s.Id, s.Name, s.Sensitivity, s.Timeout, s.MotionHistory.Take(25).Select(mh => (mh.DateTime, mh.motion)).ToArray())).ToArray(),
            (await assigned).Select(a => (a.Id, a.Light.Id, a.Sensor.Id)).ToArray()
            );

    }

    [HttpPatch("light/{lightId}")]
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

        await _settingsUpdateService.UpdateEsp(light.EspId);

        return Ok();
    }

    [HttpPatch("sensor/{sensorId}")]
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

        await _settingsUpdateService.UpdateEsp(sensor.EspId);

        return Ok();
    }

    [HttpDelete("light/{lightId}")]
    [ActionName("DeleteLight")]
    public async Task<IActionResult> DeleteLight(Guid lightId) {
        var light = await _DbContext.Lights.FindAsync(lightId);

        if (light is null) {
            return NotFound();
        }

        _DbContext.Remove(light);
        await _DbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("sensor/{sensorId}")]
    [ActionName("DeleteSensor")]
    public async Task<IActionResult> DeleteSensor(Guid sensorId) {
        var sensor = await _DbContext.Lights.FindAsync(sensorId);

        if (sensor is null) {
            return NotFound();
        }

        _DbContext.Remove(sensor);
        await _DbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("assigned")]
    [ActionName("AssignedLightAndSensor")]
    public async Task<IActionResult> AssignedLightAndSensor(CreateComboModel createComboModel) {

        if (await _DbContext.Assigneds.AnyAsync(combo => combo.Light.Id == createComboModel.lightId && combo.Sensor.Id == createComboModel.sensorId))
            return Conflict();

        var light = await _DbContext.Lights.FindAsync(createComboModel.lightId);
        if (light == null) return NotFound();

        var sensor = await _DbContext.Sensors.FindAsync(createComboModel.sensorId);
        if (sensor == null) return NotFound();

        if (light.EspId != sensor.EspId) {
            return BadRequest();
        }

        _DbContext.Add(new Assigned {
            Id = Guid.NewGuid(),
            Light = light,
            Sensor = sensor
        });

        await _DbContext.SaveChangesAsync();

        await _settingsUpdateService.UpdateEsp(light.EspId);

        return Created();
    }

    [HttpDelete("assigned/{id}")]
    [ActionName("DeleteAssignedLightAndSensor")]
    public async Task<IActionResult> DeleteAssignedLightAndSensor(Guid id) {
        var assigned = await _DbContext.Assigneds.FindAsync(id);

        if (assigned is null) {
            return NotFound();
        }

        var espId = assigned.Light.EspId;

        _DbContext.Remove(assigned);
        await _DbContext.SaveChangesAsync();

        await _settingsUpdateService.UpdateEsp(espId);

        return Ok();
    }


    public record ResponseModel(
        (Guid Id, string Name, bool Override, int State, int Brightness)[] Lights,
        (Guid Id, string Name, int Sensitivity, int Timeout, (DateTime DateTime, bool Motion)[] MotionHistory)[] Sensors,
        (Guid Id, Guid LightId, Guid SensorId)[] Combinations
    );

    public record LightUpdateModel(string Name, bool Overide, int State);
    public record SensorUpdateModel(string Name, int Sensitivity, int Timeout);
    public record CreateComboModel(Guid lightId, Guid sensorId);
}
