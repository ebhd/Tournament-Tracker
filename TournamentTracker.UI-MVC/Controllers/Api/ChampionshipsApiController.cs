using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;
using TournamentTracker.UI_MVC.Models.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace TournamentTracker.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/championships")]
public class ChampionshipsApiController : ControllerBase
{
    private readonly IManager _manager;
    public ChampionshipsApiController(IManager manager) => _manager = manager;

    [HttpGet]
    public ActionResult<IEnumerable<ChampionshipDto>> GetAllChampionships()
    {
        var championships = _manager.GetAllChampionshipsWithGroups();

        return Ok(championships.Select(championship => new ChampionshipDto
        {
            Id = championship.Id,
            ChampionshipEdition = championship.ChampionshipEdition,
            HostCountry = championship.HostCountry,
            Type = championship.Type.ToString(),
            GroupCount = championship.Groups.Count
        }));
    }

    [HttpGet("{championshipId}")]
    public ActionResult<ChampionshipDto> GetOneChampionship(int championshipId)
    {
        var championship = _manager.GetChampionshipWithGroups(championshipId);
        if (championship == null)
        {
            return NotFound();
        }

        return Ok(new ChampionshipDto
        {
            Id = championship.Id,
            ChampionshipEdition = championship.ChampionshipEdition,
            HostCountry = championship.HostCountry,
            Type = championship.Type.ToString(),
            GroupCount = championship.Groups.Count
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public ActionResult<ChampionshipDto> AddChampionship(NewChampionshipDto championshipDto)
    {
        var createdChampionship = _manager.AddChampionship(
            championshipDto.ChampionshipEdition,
            championshipDto.HostCountry,
            championshipDto.Type
        );

        var dto = new ChampionshipDto
        {
            Id = createdChampionship.Id,
            ChampionshipEdition = createdChampionship.ChampionshipEdition,
            HostCountry = createdChampionship.HostCountry,
            Type = createdChampionship.Type.ToString(),
            GroupCount = 0
        };

        return CreatedAtAction(
            nameof(GetOneChampionship),
            new { championshipId = createdChampionship.Id },
            dto
        );
    }
}
