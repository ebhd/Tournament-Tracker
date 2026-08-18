using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;
using TournamentTracker.UI_MVC.Models;

namespace TournamentTracker.UI_MVC.Controllers;

public class TeamController : Controller
{
    private readonly IManager _manager; 
    
    public TeamController(IManager manager)
    {
        _manager = manager;
    }
    [HttpGet]
    public IActionResult Index()
    {
        var teams = _manager.GetAllTeams();
        return View(teams);
    }
    
    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(NewTeamViewModel teamViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(teamViewModel);
        }

        var createdTeam = _manager.AddTeam(
            teamViewModel.Name,
            teamViewModel.Country,
            teamViewModel.Coach,
            teamViewModel.LogoImage,
            teamViewModel.TeamValue
        );

        return RedirectToAction(nameof(Details), new { id = createdTeam.Id });
    }
    
    [HttpGet]
    public IActionResult Details(int id)
    {
        var team = _manager.GetTeamWithGroupsAndChampionships(id);
        if (team == null)
            return NotFound();
        
        
        return View(team);
    }
}
