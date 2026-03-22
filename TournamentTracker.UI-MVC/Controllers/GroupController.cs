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

    public IActionResult Details(int id)
    {
        var group = _manager.GetGroup(id);
        if (group == null)
            return NotFound();

        return View(group);
    }
}