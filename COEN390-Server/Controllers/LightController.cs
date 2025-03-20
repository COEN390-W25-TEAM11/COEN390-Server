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


    [HttpGet]
    [ActionName("GetLights")]
    public async Task<ActionResult<IEnumerable<Light>>> GetLights() {
        // Get all the lights in this database with the last 50 motion history

        var lights = await _DbContext.Lights
            .Include(l => l.MotionHistory)
            .ToListAsync();

        var result = lights.Select(l => new {
            l.Id,
            l.Name,
            l.Overide,
            l.State,
            MotionHistory = l.MotionHistory.OrderByDescending(m => m.DateTime).Select(m => new {
                m.Id,
                m.DateTime,
                m.motion
            }).Take(50)
        });

        return Ok(result);
    }

    [HttpPost]
    [ActionName("PostLight")]
    public async Task<IActionResult> PostLight(LightDto lightDto) {
        // Create a light to be stored in the database

        try {
        var light = new Light()
        {
            Id = Guid.Parse(lightDto.Id),
            Name = lightDto.Name,
            Overide = lightDto.Overide,
            State = lightDto.State,
        };

        _DbContext.Lights.Add(light);
        await _DbContext.SaveChangesAsync();
        return Created();
        
        } catch (Exception e) {
            return BadRequest(e.Message);
        }
    }

    [HttpPatch("{lightId}")]
    [ActionName("PostLight")]
    public async Task<IActionResult> PostLight(Guid lightId, LightUpdateDto lightUpdateDto) {

        var light = await _DbContext.Lights.FindAsync(lightId);

        if (light is null) {
            return NotFound();
        }

        light.Name = lightUpdateDto.Name;
        light.Overide = lightUpdateDto.Overide;
        light.State = lightUpdateDto.State;

        await _DbContext.SaveChangesAsync();

        var updateObject = new {
            overide = lightUpdateDto.Overide,
            state = lightUpdateDto.State
        };

        await _settingsUpdateService.sendUpdate(lightId, updateObject);

        return Ok();
    }

    public record LightDto(string Id, string Name, bool Overide, int State);
    public record LightUpdateDto(string Name, bool Overide, int State);
}
