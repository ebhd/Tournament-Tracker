using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;
using TournamentTracker.Domain;
using TournamentTracker.UI_MVC.Models.Team;

namespace TournamentTracker.UI_MVC.Controllers;

public class TeamController : Controller
{
    private readonly IManager _manager;

    public TeamController(IManager manager)
    {
        _manager = manager;
    }

    public IActionResult Index()
    {
        var teams = _manager.GetAllTeams();
        return View(teams);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Add()
    {
        return View(new Team());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Team team)
    {
        ModelState.Remove(nameof(Team.MaintainerId));
        ModelState.Remove(nameof(Team.Maintainer));

        if (!ModelState.IsValid)
        {
            return View(team);
        }

        var created = _manager.AddTeam(
            team.Name,
            team.Country,
            team.Coach ?? string.Empty,
            team.LogoImage,
            team.TeamValue,
            User.Identity?.Name ?? string.Empty
        );

        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    public IActionResult Details(int id)
    {
        var team = _manager.GetTeamWithGroups(id);
        if (team == null)
            return NotFound();

        var currentUserName = User.Identity?.Name;
        var isAdmin = User.IsInRole("admin");
        var canEditTeamValue = !string.IsNullOrWhiteSpace(currentUserName) &&
                               (isAdmin || string.Equals(team.Maintainer.UserName, currentUserName, StringComparison.OrdinalIgnoreCase));

        var viewModel = new TeamDetailsViewModel
        {
            Team = team,
            MaintainerName = string.IsNullOrWhiteSpace(team.Maintainer.UserName) ? "unknown" : team.Maintainer.UserName,
            CanEditTeamValue = canEditTeamValue,
            IsAdmin = isAdmin,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        };

        return View(viewModel);
    }
}
