using System.ComponentModel.DataAnnotations;
using TournamentTracker.BL;
using TournamentTracker.UI.CA.Extensions;

namespace TournamentTracker.UI.CA;

internal class ConsoleUi
{
    private readonly IManager _manager;

    public ConsoleUi(IManager manager)
    {
        _manager = manager;
    }

    public void Run()
    {
        bool running = true;
        while (running)
        {
            ShowMenu();
            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Invalid choice.");
                continue;
            }

            switch (choice)
            {
                case 0:
                    running = false;
                    break;
                case 1:
                    ShowAllTeams();
                    break;
                case 2:
                    ShowTeamsByCountry();
                    break;
                case 3:
                    ShowAllGroups();
                    break;
                case 4:
                    ShowGroupsByChampionship();
                    break;
                case 5:
                    AddTeam();
                    break;
                case 6:
                    AddGroup();
                    break;
                case 7:
                    SearchGroups();
                    break;
                case 8:
                    AddTeamToGroup();
                    break;
                case 9:
                    RemoveTeamFromGroup();
                    break;
            }
        }
    }

    private void ShowMenu()
    {
        Console.WriteLine("==========================");
        Console.WriteLine("0) Quit");
        Console.WriteLine("1) Show all Teams");
        Console.WriteLine("2) Show Teams of Country");
        Console.WriteLine("3) Show all Groups");
        Console.WriteLine("4) Show Groups by Championship Id");
        Console.WriteLine("5) Add Team");
        Console.WriteLine("6) Add Group");
        Console.WriteLine("7) Search Groups (optional Code / MaxTeams)");
        Console.WriteLine("8) Add Team to Group");
        Console.WriteLine("9) Remove Team from Group");
        Console.Write("Choice: ");
    }

    private void ShowAllTeams()
    {
        foreach (var t in _manager.GetAllTeamsWithGroups())
        {
            Console.WriteLine(t.StringRepresentation());

            foreach (var gt in t.Groups)
            {
                Console.WriteLine($"    Group: {gt.Group.GroupCode} (MaxTeams {gt.Group.MaxTeams})");
            }
        }
    }

    private void ShowTeamsByCountry()
    {
        Console.Write("Country: ");
        var country = Console.ReadLine();
        foreach (var t in _manager.GetTeamsByCountry(country ?? "")) Console.WriteLine(t.StringRepresentation());
    }

    private void ShowAllGroups()
    {
        foreach (var g in _manager.GetAllGroupsWithChampionshipAndTeams())
        {
            Console.WriteLine(g.StringRepresentation());
            Console.WriteLine($"    Championship: {g.Championship.HostCountry} {g.Championship.ChampionshipEdition:yyyy}");

            foreach (var gt in g.Teams)
            {
                Console.WriteLine($"    Team: {gt.Team.Name} (Points {gt.Points})");
            }
        }
    }

    private void ShowGroupsByChampionship()
    {
        Console.Write("Championship Id: ");
        var cidTxt = Console.ReadLine();
        if (int.TryParse(cidTxt, out var cid))
        {
            foreach (var g in _manager.GetGroupsByChampionship(cid)) Console.WriteLine(g.StringRepresentation());
        }
    }

    private void AddTeam()
    {
        try
        {
            Console.Write("Name: ");
            var name = Console.ReadLine();
            Console.Write("Country: ");
            var ctry = Console.ReadLine();
            Console.Write("Coach: ");
            var coach = Console.ReadLine();
            Console.Write("Logo URL: ");
            var logo = Console.ReadLine();
            decimal? val;
            bool success;
            do
            {
                Console.Write("Team Value (optional): ");
                var tv = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(tv))
                {
                    val = null;
                    success = true;
                }
                else
                {
                    success = decimal.TryParse(tv, out var teamValue);
                    val = teamValue;
                }
            } while (!success);

            var t = _manager.AddTeam(name, ctry, coach, logo, val);
            Console.WriteLine(t.StringRepresentation());
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void AddGroup()
    {
        try
        {
            Console.Write("MaxTeams: ");
            var mt = Console.ReadLine();
            Console.Write("GroupCode (single letter): ");
            var gc = Console.ReadLine();
            Console.Write("Championship Id: ");
            var chId = Console.ReadLine();

            if (!int.TryParse(mt, out var max)) return;
            if (string.IsNullOrWhiteSpace(gc)) return;
            if (!int.TryParse(chId, out var ch)) return;

            var grp = _manager.AddGroup(max, char.ToUpper(gc[0]), ch);
            Console.WriteLine(grp.StringRepresentation());
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void SearchGroups()
    {
        Console.Write("GroupCode (leave blank): ");
        var codeTxt = Console.ReadLine();

        char? code = null;
        if (!string.IsNullOrWhiteSpace(codeTxt))
            code = char.ToUpper(codeTxt[0]);

        Console.Write("MaxTeams (leave blank): ");
        var maxTxt = Console.ReadLine();

        int? maxTeams = null;
        if (int.TryParse(maxTxt, out var m))
            maxTeams = m;

        foreach (var g in _manager.GetGroupsByCodeAndMaxTeams(code, maxTeams))
            Console.WriteLine(g.StringRepresentation());
    }

    private void AddTeamToGroup()
    {
        try
        {
            Console.WriteLine("Groups:");
            foreach (var g in _manager.GetAllGroups()) Console.WriteLine(g.StringRepresentation());

            Console.Write("Group Id: ");
            if (!int.TryParse(Console.ReadLine(), out var gId)) return;

            Console.WriteLine("Teams:");
            foreach (var t in _manager.GetAllTeams()) Console.WriteLine(t.StringRepresentation());

            Console.Write("Team Id: ");
            if (!int.TryParse(Console.ReadLine(), out var tId)) return;

            Console.Write("Points (optional): ");
            var pTxt = Console.ReadLine();
            int points = 0;
            if (!string.IsNullOrWhiteSpace(pTxt)) int.TryParse(pTxt, out points);

            _manager.AddTeamToGroup(tId, gId, points);
            Console.WriteLine("Team linked to group.");
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void RemoveTeamFromGroup()
    {
        Console.WriteLine("Groups:");
        foreach (var g in _manager.GetAllGroups()) Console.WriteLine(g.StringRepresentation());

        Console.Write("Group Id: ");
        if (!int.TryParse(Console.ReadLine(), out var rgId)) return;

        var linkedTeams = _manager.GetTeamsOfGroup(rgId).ToList();
        if (linkedTeams.Count == 0)
        {
            Console.WriteLine("No teams linked to this group.");
            return;
        }

        Console.WriteLine("Linked teams:");
        foreach (var t in linkedTeams) Console.WriteLine(t.StringRepresentation());

        Console.Write("Team Id to remove: ");
        if (!int.TryParse(Console.ReadLine(), out var rtId)) return;

        _manager.RemoveTeamFromGroup(rtId, rgId);
        Console.WriteLine(" Link removed.");
    }
}
