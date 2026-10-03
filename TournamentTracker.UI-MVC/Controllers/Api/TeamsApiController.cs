using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;
using TournamentTracker.UI_MVC.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TournamentTracker.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/teams")]
public class TeamsApiController : ControllerBase
{
    private readonly IManager _manager;
    public TeamsApiController(IManager manager) => _manager = manager;

    [HttpGet]
    public ActionResult<IEnumerable<TeamDto>> GetAllTeams()
    {
        var teams = _manager.GetAllTeams()
            .Select(team => new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Country = team.Country
            });

        return Ok(teams);
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPut("{teamId:int}/teamvalue")]
    public IActionResult UpdateTeamValue(int teamId, UpdateTeamValueDto dto)
    {
        var team = _manager.GetTeam(teamId);
        if (team == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("admin");
        if (!_manager.UpdateTeamValue(teamId, dto.TeamValue, userId, isAdmin))
            return Forbid();

        return NoContent();
    }
}
