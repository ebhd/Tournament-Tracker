using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;

[ApiController]
[Route("api/teams")]
public class TeamsApiController : ControllerBase
{
    private readonly IManager _manager;
    public TeamsApiController(IManager manager) => _manager = manager;

    [HttpGet]
    public IActionResult GetAll()
    {
        var teams = _manager.GetAllTeams()
            .Select(t => new { t.Id, t.Name, t.Country });
        return Ok(teams);
    }

    [Authorize]
    [HttpPut("{teamId:int}/teamvalue")]
    public IActionResult UpdateTeamValue(int teamId, [FromBody] UpdateTeamValueDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var team = _manager.GetTeamWithMaintainer(teamId);
        if (team is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("admin") && !string.Equals(team.Maintainer.UserName, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var updated = _manager.UpdateTeamValue(teamId, dto.TeamValue, User.Identity?.Name, User.IsInRole("admin"));
        return updated ? NoContent() : NotFound();
    }
}

public class UpdateTeamValueDto
{
    [Range(0, double.MaxValue)]
    public decimal? TeamValue { get; set; }
}
