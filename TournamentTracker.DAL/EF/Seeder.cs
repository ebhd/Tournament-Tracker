using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TournamentTracker.Domain;

namespace TournamentTracker.DAL.EF;

public static class Seeder
{
    private static readonly string[] DefaultMaintainerEmails =
    [
        "lars@kdg.be",
        "ebrahim@kdg.be",
        "ayoub@kdg.be",
        "ali@kdg.be",
        "chris@kdg.be"
    ];

    public static void Seed(LeagueDbContext context)
    {
        if (context.Champions.Any())
        {
            return;
        }

        var championships = context.Champions;
        var groups = context.Groups;

        var c1 = new Championship { ChampionshipEdition = new DateTime(2022, 11, 21), HostCountry = "Qatar", Type = ChampionshipType.WorldCup };
        var c2 = new Championship { ChampionshipEdition = new DateTime(2018, 06, 14), HostCountry = "Russia", Type = ChampionshipType.WorldCup };
        var c3 = new Championship { ChampionshipEdition = new DateTime(2014, 06, 12), HostCountry = "Brazil", Type = ChampionshipType.WorldCup };
        var c4 = new Championship { ChampionshipEdition = new DateTime(2010, 06, 11), HostCountry = "South Africa", Type = ChampionshipType.WorldCup };
        championships.AddRange(c1, c2, c3, c4);

        var g1 = new Group { GroupCode = 'A', MaxTeams = 4, Championship = c1 };
        var g2 = new Group { GroupCode = 'B', MaxTeams = 4, Championship = c1 };
        var g3 = new Group { GroupCode = 'C', MaxTeams = 1, Championship = c1 };
        var g4 = new Group { GroupCode = 'D', MaxTeams = 4, Championship = c1 };
        var g5 = new Group { GroupCode = 'A', MaxTeams = 2, Championship = c2 };
        var g6 = new Group { GroupCode = 'B', MaxTeams = 4, Championship = c2 };
        var g7 = new Group { GroupCode = 'C', MaxTeams = 2, Championship = c2 };
        var g8 = new Group { GroupCode = 'D', MaxTeams = 3, Championship = c2 };

        groups.AddRange(g1, g2, g3, g4, g5, g6, g7, g8);
        context.SaveChanges();
        context.ChangeTracker.Clear();
    }

    public static async Task<int> EnsureSeededTeamMaintainersAsync(LeagueDbContext context, CancellationToken cancellationToken = default)
    {
        var seededUsers = await context.Users
            .Where(user => DefaultMaintainerEmails.Contains(user.Email!))
            .ToDictionaryAsync(user => user.Email!, cancellationToken);

        if (seededUsers.Count == 0)
        {
            return 0;
        }

        var teams = await context.Teams.OrderBy(team => team.Id).ToListAsync(cancellationToken);
        if (teams.Count == 0)
        {
            teams = CreateSeedTeams(seededUsers);
            await context.Teams.AddRangeAsync(teams, cancellationToken);

            var groups = await context.Groups.OrderBy(group => group.Id).ToListAsync(cancellationToken);
            var groupsByCodeAndChampionship = groups.ToDictionary(
                group => $"{group.ChampionshipId}:{group.GroupCode}",
                group => group);

            var groupTeamLinks = CreateSeedGroupTeams(teams, groupsByCodeAndChampionship);
            await context.GroupTeams.AddRangeAsync(groupTeamLinks, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            context.ChangeTracker.Clear();
            return teams.Count;
        }

        var changedCount = 0;
        foreach (var team in teams)
        {
            if (!string.IsNullOrWhiteSpace(team.MaintainerId) && await context.Users.AnyAsync(user => user.Id == team.MaintainerId, cancellationToken))
            {
                continue;
            }

            var assignedEmail = DefaultMaintainerEmails[(Math.Max(team.Id, 1) - 1) % DefaultMaintainerEmails.Length];
            if (!seededUsers.TryGetValue(assignedEmail, out var maintainer))
            {
                continue;
            }

            team.MaintainerId = maintainer.Id;
            changedCount++;
        }

        if (changedCount > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            context.ChangeTracker.Clear();
        }

        return changedCount;
    }

    private static List<Team> CreateSeedTeams(IReadOnlyDictionary<string, IdentityUser> seededUsers)
    {
        return new List<Team>
        {
            new() { Name = "Belgische National Team", Country = "Belgium", Coach = "A. Coach", LogoImage = "https://example.com/lions.png", TeamValue = 120.5m, MaintainerId = seededUsers["lars@kdg.be"].Id },
            new() { Name = "France National Team", Country = "France", Coach = "B. Coach", LogoImage = "https://example.com/tigers.png", TeamValue = null, MaintainerId = seededUsers["ebrahim@kdg.be"].Id },
            new() { Name = "Spain National Team", Country = "Spain", Coach = "C. Coach", LogoImage = "https://example.com/eagles.png", TeamValue = 95.0m, MaintainerId = seededUsers["ayoub@kdg.be"].Id },
            new() { Name = "Germany National Team", Country = "Germany", Coach = "D. Coach", LogoImage = "https://example.com/sharks.png", TeamValue = 150.0m, MaintainerId = seededUsers["ali@kdg.be"].Id },
            new() { Name = "Italy National Team", Country = "Italy", Coach = "E. Coach", LogoImage = "https://example.com/wolves.png", TeamValue = 80.0m, MaintainerId = seededUsers["chris@kdg.be"].Id },
            new() { Name = "Portugal National Team", Country = "Portugal", Coach = "F. Coach", LogoImage = "https://example.com/dragons.png", TeamValue = 110.0m, MaintainerId = seededUsers["lars@kdg.be"].Id }
        };
    }

    private static IEnumerable<GroupTeam> CreateSeedGroupTeams(IReadOnlyList<Team> teams, IReadOnlyDictionary<string, Group> groupsByCodeAndChampionship)
    {
        Team TeamAt(int index) => teams[index - 1];

        Group GroupAt(int championshipId, char code) => groupsByCodeAndChampionship[$"{championshipId}:{code}"];

        return new[]
        {
            new GroupTeam { Group = GroupAt(1, 'A'), Team = TeamAt(1), Points = 0 },
            new GroupTeam { Group = GroupAt(1, 'A'), Team = TeamAt(2), Points = 0 },
            new GroupTeam { Group = GroupAt(1, 'B'), Team = TeamAt(2), Points = 0 },
            new GroupTeam { Group = GroupAt(1, 'B'), Team = TeamAt(3), Points = 0 },
            new GroupTeam { Group = GroupAt(1, 'C'), Team = TeamAt(4), Points = 0 },
            new GroupTeam { Group = GroupAt(1, 'D'), Team = TeamAt(5), Points = 0 },
            new GroupTeam { Group = GroupAt(1, 'D'), Team = TeamAt(6), Points = 0 },
            new GroupTeam { Group = GroupAt(2, 'A'), Team = TeamAt(1), Points = 0 },
            new GroupTeam { Group = GroupAt(2, 'B'), Team = TeamAt(6), Points = 0 },
            new GroupTeam { Group = GroupAt(2, 'C'), Team = TeamAt(3), Points = 0 },
            new GroupTeam { Group = GroupAt(2, 'D'), Team = TeamAt(2), Points = 0 }
        };
    }
}
