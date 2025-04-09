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
        var sensors = _DbContext.Sensors.Include(s => s.MotionHistory).ToListAsync();
        var assigned = _DbContext.Assigneds.ToListAsync();

        return new ResponseModel {
            Lights = (await lights).Select(l => new ResponseModel.ResponseLightModel { 
                Id = l.Id,
                Name = l.Name,
                Pin = l.Pin,
                Overide = l.Overide,
                State = l.State,
                Brightness = l.Brightness,
            }),
            Sensors = (await sensors).Select(s => new ResponseModel.ResponseSensorModel {
                Id = s.Id,
                Name = s.Name,
                Pin = s.Pin,
                Sensitivity = s.Sensitivity,
                Timeout = s.Timeout,
                Motion = s.MotionHistory?.Select(m => new ResponseModel.ResponseMotionModel { DateTime = m.DateTime, Motion = m.motion }).ToList() ?? new List<ResponseModel.ResponseMotionModel>(),
            }),
            Combinations = (await assigned).Select(a => new ResponseModel.ResponseAssignedModel {
                Id = a.Id,
                LightId = a.Light.Id,
                SensorId = a.Sensor.Id,
            })
        };
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

	await _settingsUpdateService.UpdateEsp(light.EspId);

        return Ok();
    }

    [HttpDelete("sensor/{sensorId}")]
    [ActionName("DeleteSensor")]
    public async Task<IActionResult> DeleteSensor(Guid sensorId) {
        var sensor = await _DbContext.Sensors.FindAsync(sensorId);

        if (sensor is null) {
            return NotFound();
        }

        _DbContext.Remove(sensor);
        await _DbContext.SaveChangesAsync();

	await _settingsUpdateService.UpdateEsp(light.EspId);

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
        var assigned = await _DbContext.Assigneds
            .Include(x => x.Light)
            .Include(x => x.Sensor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (assigned is null) {
            return NotFound();
        }

        var espId = assigned.Light.EspId;

        _DbContext.Remove(assigned);
        await _DbContext.SaveChangesAsync();

        await _settingsUpdateService.UpdateEsp(espId);

        return Ok();
    }


    public class ResponseModel {

        public required IEnumerable<ResponseLightModel> Lights { get; set; }
        public required IEnumerable<ResponseSensorModel> Sensors { get; set; }
        public required IEnumerable<ResponseAssignedModel> Combinations { get; set; }

        public class ResponseLightModel {
            public required Guid Id { get; set; }
            public required string Name { get; set; }
            public required int Pin { get; set; }
            public required bool Overide { get; set; }
            public required int State { get; set; }
            public required int Brightness { get; set; }
        }

        public class ResponseSensorModel {
            public required Guid Id { get; set; }
            public required string Name { get; set; }
            public required int Pin { get; set; }
            public required int Sensitivity { get; set; }
            public required int Timeout { get; set; }
            public required IEnumerable<ResponseMotionModel> Motion { get; set; }
        }

        public class ResponseAssignedModel {
            public required Guid Id { get; set; }
            public required Guid LightId { get; set; }
            public required Guid SensorId { get; set; }
        }

        public class ResponseMotionModel {
            public required DateTime DateTime { get; set; }
            public required bool Motion { get; set; }
        }
    };

    public record LightUpdateModel(string Name, bool Overide, int State);
    public record SensorUpdateModel(string Name, int Sensitivity, int Timeout);
    public record CreateComboModel(Guid lightId, Guid sensorId);
}
