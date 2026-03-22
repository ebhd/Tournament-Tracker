using Microsoft.AspNetCore.Mvc;

namespace TournamentTracker.UI_MVC.Controllers;

public class ChampionshipController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}