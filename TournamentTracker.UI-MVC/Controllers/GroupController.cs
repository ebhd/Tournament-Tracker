using Microsoft.AspNetCore.Mvc;
using TournamentTracker.BL;

namespace TournamentTracker.UI_MVC.Controllers;

public class GroupController : Controller
{
    private readonly IManager _manager;

    public GroupController(IManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var group = _manager.GetGroupWithChampionship(id);
        if (group == null)
            return NotFound();

        return View(group);
    }
}
