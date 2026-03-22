using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;

[ApiController]
[Route("api/groups")]
public class GroupsApiController : ControllerBase
{
    private readonly IManager _manager;
    public GroupsApiController(IManager manager) => _manager = manager;

    [HttpGet("{id}/teams")]
    public IActionResult GetTeams(int id)
    {
        var items = _manager.GetGroupTeamsOfGroup(id)
            .Select(x => new {
                teamId = x.Team.Id,
                name = x.Team.Name,
                country = x.Team.Country,
                points = x.Points
            });

        return Ok(items);
    }

    [Authorize]
    [HttpPost("{id}/teams")]
    public IActionResult AddTeam(int id, [FromBody] AddTeamToGroupDto dto)
    {
        try
        {
            _manager.AddTeamToGroup(dto.TeamId, id, dto.Points);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record AddTeamToGroupDto(int TeamId, int Points);
