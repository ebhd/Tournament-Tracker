using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;
using TournamentTracker.UI_MVC.Models.Dtos;

namespace TournamentTracker.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/groups")]
public class GroupsApiController : ControllerBase
{
    private readonly IManager _manager;
    public GroupsApiController(IManager manager) => _manager = manager;

    [HttpGet("{groupId}/teams")]
    public ActionResult<IEnumerable<GroupTeamDto>> GetTeams(int groupId)
    {
        var group = _manager.GetGroupWithChampionship(groupId);
        if (group == null)
        {
            return NotFound();
        }

        var groupTeams = _manager.GetGroupTeamsOfGroupWithTeam(groupId);

        return Ok(groupTeams.Select(groupTeam => new GroupTeamDto
        {
            TeamId = groupTeam.Team.Id,
            Name = groupTeam.Team.Name,
            Country = groupTeam.Team.Country,
            Points = groupTeam.Points
        }));
    }

    [HttpGet("{groupId}/teams/{teamId}")]
    public ActionResult<GroupTeamDto> GetOneGroupTeam(int groupId, int teamId)
    {
        var groupTeam = _manager.GetGroupTeamWithTeam(groupId, teamId);
        if (groupTeam == null)
        {
            return NotFound();
        }

        return Ok(new GroupTeamDto
        {
            TeamId = groupTeam.Team.Id,
            Name = groupTeam.Team.Name,
            Country = groupTeam.Team.Country,
            Points = groupTeam.Points
        });
    }

    [HttpPost("{groupId}/teams")]
    public ActionResult<GroupTeamDto> AddTeam(int groupId, NewGroupTeamDto groupTeamDto)
    {
        var group = _manager.GetGroupWithChampionship(groupId);
        if (group == null)
        {
            return NotFound();
        }

        var teamId = groupTeamDto.TeamId.Value;
        var existingGroupTeam = _manager.GetGroupTeamWithTeam(groupId, teamId);
        if (existingGroupTeam != null)
        {
            return Conflict(new { error = $"Team with id {teamId} is already linked to group with id {groupId}" });
        }

        try
        {
            var createdGroupTeam = _manager.AddTeamToGroup(
                teamId,
                groupId,
                groupTeamDto.Points
            );

            var dto = new GroupTeamDto
            {
                TeamId = createdGroupTeam.Team.Id,
                Name = createdGroupTeam.Team.Name,
                Country = createdGroupTeam.Team.Country,
                Points = createdGroupTeam.Points
            };

            return CreatedAtAction(
                nameof(GetOneGroupTeam),
                new { groupId, teamId },
                dto
            );
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
