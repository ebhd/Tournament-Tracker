using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TournamentTracker.BL;
using TournamentTracker.DAL;
using TournamentTracker.DAL.EF;
using TournamentTracker.Domain;

class Program
{
    static void Main(string[] args)
    {
        var optionBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
        optionBuilder.UseSqlite(LeagueDatabasePath.GetConnectionString());

        var dbContext = new LeagueDbContext(optionBuilder.Options);
        bool isCreated = dbContext.CreateDataBase(true);

        if (isCreated)
        {
            SeedConsoleIdentityUsers(dbContext);
            Seeder.Seed(dbContext);
            Seeder.EnsureSeededTeamMaintainersAsync(dbContext).GetAwaiter().GetResult();
        }

        IRepository repo = new Repository(dbContext);
        IManager manager = new Manager(repo);

        bool running = true;
        while (running)
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
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "0":
                    running = false;
                    break;

                case "1":
                    foreach (var team in manager.GetAllTeamsWithMaintainer())
                    {
                        Console.WriteLine(team);
                        Console.WriteLine($"    Maintainer: {team.Maintainer.UserName}");

                        foreach (var gt in team.Groups)
                        {
                            Console.WriteLine($"    Group: {gt.Group.GroupCode} (MaxTeams {gt.Group.MaxTeams})");
                        }
                    }
                    break;

                case "2":
                    Console.Write("Country: ");
                    var country = Console.ReadLine();
                    foreach (var t in manager.GetTeamsByCountry(country ?? string.Empty))
                    {
                        Console.WriteLine(t);
                    }
                    break;

                case "3":
                    foreach (var g in manager.GetAllGroupsWithChampionshipAndTeams())
                    {
                        Console.WriteLine(g);
                        Console.WriteLine($"    Championship: {g.Championship.HostCountry} {g.Championship.ChampionshipEdition:yyyy}");

                        foreach (var gt in g.Teams)
                        {
                            Console.WriteLine($"    Team: {gt.Team.Name} (Points {gt.Points})");
                        }
                    }
                    break;

                case "4":
                    Console.Write("Championship Id: ");
                    var cidTxt = Console.ReadLine();
                    if (int.TryParse(cidTxt, out var cid))
                    {
                        var champ = new Championship { Id = cid };
                        foreach (var g in manager.GetGroupsByChampionship(champ))
                        {
                            Console.WriteLine(g);
                        }
                    }
                    break;

                case "5":
                    try
                    {
                        Console.Write("Maintainer username: ");
                        var maintainerUsername = Console.ReadLine();
                        Console.Write("Name: ");
                        var name = Console.ReadLine();
                        Console.Write("Country: ");
                        var ctry = Console.ReadLine();
                        Console.Write("Coach: ");
                        var coach = Console.ReadLine();
                        Console.Write("Logo URL: ");
                        var logo = Console.ReadLine();
                        Console.Write("Team Value (optional): ");
                        var tv = Console.ReadLine();
                        decimal? val = string.IsNullOrWhiteSpace(tv) ? null : Convert.ToDecimal(tv);
                        var team = manager.AddTeam(name ?? string.Empty, ctry ?? string.Empty, coach ?? string.Empty, logo ?? string.Empty, val, maintainerUsername ?? string.Empty);
                        Console.WriteLine(team);
                        Console.WriteLine($"Maintainer: {team.Maintainer.UserName}");
                    }
                    catch (ValidationException ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    break;

                case "6":
                    try
                    {
                        Console.Write("MaxTeams: ");
                        var mt = Console.ReadLine();
                        Console.Write("GroupCode (single letter): ");
                        var gc = Console.ReadLine();
                        Console.Write("Championship Id: ");
                        var chId = Console.ReadLine();

                        if (!int.TryParse(mt, out var max)) break;
                        if (string.IsNullOrWhiteSpace(gc)) break;
                        if (!int.TryParse(chId, out var ch)) break;

                        var grp = manager.AddGroup(max, char.ToUpper(gc[0]), new Championship { Id = ch });
                        Console.WriteLine(grp);
                    }
                    catch (ValidationException ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    break;

                case "7":
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

                    foreach (var g in manager.GetGroupsByCodeAndMaxTeams(code, maxTeams))
                        Console.WriteLine(g);
                    break;

                case "8":
                    try
                    {
                        Console.WriteLine("Groups:");
                        foreach (var g in manager.GetAllGroups()) Console.WriteLine(g);

                        Console.Write("Group Id: ");
                        if (!int.TryParse(Console.ReadLine(), out var gId)) break;

                        Console.WriteLine("Teams:");
                        foreach (var t in manager.GetAllTeams()) Console.WriteLine(t);

                        Console.Write("Team Id: ");
                        if (!int.TryParse(Console.ReadLine(), out var tId)) break;

                        Console.Write("Points (optional): ");
                        var pTxt = Console.ReadLine();
                        int points = 0;
                        if (!string.IsNullOrWhiteSpace(pTxt)) int.TryParse(pTxt, out points);

                        manager.AddTeamToGroup(tId, gId, points);
                        Console.WriteLine("Team linked to group.");
                    }
                    catch (ValidationException ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    break;

                case "9":
                    Console.WriteLine("Groups:");
                    foreach (var g in manager.GetAllGroups()) Console.WriteLine(g);

                    Console.Write("Group Id: ");
                    if (!int.TryParse(Console.ReadLine(), out var rgId)) break;

                    var linkedTeams = manager.GetTeamsOfGroup(rgId).ToList();
                    if (linkedTeams.Count == 0)
                    {
                        Console.WriteLine("No teams linked to this group.");
                        break;
                    }

                    Console.WriteLine("Linked teams:");
                    foreach (var t in linkedTeams) Console.WriteLine(t);

                    Console.Write("Team Id to remove: ");
                    if (!int.TryParse(Console.ReadLine(), out var rtId)) break;

                    manager.RemoveTeamFromGroup(rtId, rgId);
                    Console.WriteLine(" Link removed.");
                    break;
            }
        }
    }

    private static void SeedConsoleIdentityUsers(LeagueDbContext context)
    {
        if (context.Users.Any())
        {
            return;
        }

        var users = new[]
        {
            new IdentityUser { UserName = "lars@kdg.be", Email = "lars@kdg.be", NormalizedUserName = "LARS@KDG.BE", NormalizedEmail = "LARS@KDG.BE@TOURNAMENTTRACKER.LOCAL" },
            new IdentityUser { UserName = "ebrahim@kdg.be", Email = "ebrahim@kdg.be", NormalizedUserName = "EBRAHIM@KDG.BE", NormalizedEmail = "EBRAHIM@KDG.BE@TOURNAMENTTRACKER.LOCAL" },
            new IdentityUser { UserName = "ayoub@kdg.be", Email = "ayoub@kdg.be", NormalizedUserName = "AYOUB@KDG.BE", NormalizedEmail = "AYOUB@KDG.BE@TOURNAMENTTRACKER.LOCAL" },
            new IdentityUser { UserName = "ali@kdg.be", Email = "ali@kdg.be", NormalizedUserName = "ALI@KDG.BE", NormalizedEmail = "ALI@KDG.BE@TOURNAMENTTRACKER.LOCAL" },
            new IdentityUser { UserName = "chris@kdg.be", Email = "chris@kdg.be", NormalizedUserName = "CHRIS@KDG.BE", NormalizedEmail = "CHRIS@KDG.BE@TOURNAMENTTRACKER.LOCAL" }
        };

        context.Users.AddRange(users);
        context.SaveChanges();
    }
}
