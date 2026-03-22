using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;

[ApiController]
[Route("api/championships")]
public class ChampionshipsApiController : ControllerBase
{
    private readonly IManager _manager;
    public ChampionshipsApiController(IManager manager) => _manager = manager;

    [HttpGet]
    public IActionResult GetAll()
    {
        var items = _manager.GetAllChampionships()
            .Select(c => new ChampionshipListDto(
                c.Id,
                c.ChampionshipEdition,
                c.HostCountry,
                c.Type.ToString(),
                c.Groups?.Count ?? 0
            ));

        return Ok(items);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Create([FromBody] ChampionshipCreateDto dto)
    {
        var created = _manager.AddChampionship(dto.ChampionshipEdition, dto.HostCountry, dto.Type);

        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, new {
            created.Id, created.ChampionshipEdition, created.HostCountry, Type = created.Type.ToString()
        });
    }
}

public record ChampionshipListDto(int Id, DateTime ChampionshipEdition, string HostCountry, string Type, int GroupCount);
public record ChampionshipCreateDto(DateTime ChampionshipEdition, string HostCountry, TournamentTracker.Domain.ChampionshipType Type);
