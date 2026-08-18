using Microsoft.AspNetCore.Mvc;

namespace TournamentTracker.UI_MVC.Controllers;

public class ChampionshipController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}