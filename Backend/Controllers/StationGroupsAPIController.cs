using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/stationgroups")]
[ApiController]
public class StationGroupsAPIController : ControllerBase
{
    private readonly IStationGroupService _stationGroupService;

    public StationGroupsAPIController(IStationGroupService stationGroupService)
    {
        _stationGroupService = stationGroupService;
    }

    [HttpGet]
    public IActionResult GetStationGroups()
    {
        var stationGroups = _stationGroupService.GetStationGroups();
        return Ok(stationGroups);
    }

    [HttpPost]
    public IActionResult CreateStationGroup([FromBody] StationGroup stationGroup)
    {
        if (stationGroup == null)
        {
            return BadRequest();
        }

        var createdStationGroup = _stationGroupService.CreateStationGroup(stationGroup);
        return CreatedAtAction(nameof(GetStationGroups), new { id = createdStationGroup.ID }, createdStationGroup);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateStationGroup(int id, [FromBody] StationGroup updatedStationGroup)
    {
        var stationGroup = _stationGroupService.UpdateStationGroup(id, updatedStationGroup);
        if (stationGroup == null)
        {
            return NotFound();
        }

        return Ok(stationGroup);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteStationGroup(int id)
    {
        var result = _stationGroupService.DeleteStationGroup(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}

