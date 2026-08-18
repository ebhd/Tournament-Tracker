using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;
using TournamentTracker.UI_MVC.Models.Dtos;

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
}
