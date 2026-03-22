using Microsoft.AspNetCore.Identity;
using TournamentTracker.Domain;

namespace TournamentTracker.DAL;

public class MemoryRepository : IRepository
{
    private readonly List<Championship> championshipList = new();
    private readonly List<Team> teamsList = new();
    private readonly List<Group> groupsList = new();
    private readonly List<IdentityUser> usersList = new();

    public MemoryRepository()
    {
        usersList.AddRange(new[]
        {
            new IdentityUser { Id = "user-1", UserName = "lars@kdg.be", Email = "lars@kdg.be" },
            new IdentityUser { Id = "user-2", UserName = "ebrahim@kdg.be", Email = "ebrahim@kdg.be" },
            new IdentityUser { Id = "user-3", UserName = "ayoub@kdg.be", Email = "ayoub@kdg.be" },
            new IdentityUser { Id = "user-4", UserName = "ali@kdg.be", Email = "ali@kdg.be" },
            new IdentityUser { Id = "user-5", UserName = "chris@kdg.be", Email = "chris@kdg.be" }
        });

        var c1 = new Championship { Id = 1, ChampionshipEdition = new DateTime(2022, 11, 21), HostCountry = "Qatar", Type = ChampionshipType.WorldCup };
        var c2 = new Championship { Id = 2, ChampionshipEdition = new DateTime(2018, 06, 14), HostCountry = "Russia", Type = ChampionshipType.WorldCup };
        var c3 = new Championship { Id = 3, ChampionshipEdition = new DateTime(2014, 06, 12), HostCountry = "Brazil", Type = ChampionshipType.WorldCup };
        var c4 = new Championship { Id = 4, ChampionshipEdition = new DateTime(2010, 06, 11), HostCountry = "South Africa", Type = ChampionshipType.WorldCup };
        championshipList.AddRange(new[] { c1, c2, c3, c4 });

        var g1 = new Group { Id = 1, GroupCode = 'A', MaxTeams = 4, Championship = c1, ChampionshipId = c1.Id };
        var g2 = new Group { Id = 2, GroupCode = 'B', MaxTeams = 4, Championship = c1, ChampionshipId = c1.Id };
        var g3 = new Group { Id = 3, GroupCode = 'C', MaxTeams = 1, Championship = c1, ChampionshipId = c1.Id };
        var g4 = new Group { Id = 4, GroupCode = 'D', MaxTeams = 4, Championship = c1, ChampionshipId = c1.Id };
        var g5 = new Group { Id = 5, GroupCode = 'A', MaxTeams = 2, Championship = c2, ChampionshipId = c2.Id };
        var g6 = new Group { Id = 6, GroupCode = 'B', MaxTeams = 4, Championship = c2, ChampionshipId = c2.Id };
        var g7 = new Group { Id = 7, GroupCode = 'C', MaxTeams = 2, Championship = c2, ChampionshipId = c2.Id };
        var g8 = new Group { Id = 8, GroupCode = 'D', MaxTeams = 3, Championship = c2, ChampionshipId = c2.Id };

        c1.Groups.Add(g1); c1.Groups.Add(g2); c1.Groups.Add(g3); c1.Groups.Add(g4);
        c2.Groups.Add(g5); c2.Groups.Add(g6); c2.Groups.Add(g7); c2.Groups.Add(g8);

        groupsList.AddRange(new[] { g1, g2, g3, g4, g5, g6, g7, g8 });

        var t1 = CreateSeedTeam(1, "Belgische National Team", "Belgium", "A. Coach", "https://example.com/lions.png", 120.5m, "lars@kdg.be");
        var t2 = CreateSeedTeam(2, "France National Team", "France", "B. Coach", "https://example.com/tigers.png", null, "ebrahim@kdg.be");
        var t3 = CreateSeedTeam(3, "Spain National Team", "Spain", "C. Coach", "https://example.com/eagles.png", 95.0m, "ayoub@kdg.be");
        var t4 = CreateSeedTeam(4, "Germany National Team", "Germany", "D. Coach", "https://example.com/sharks.png", 150.0m, "ali@kdg.be");
        var t5 = CreateSeedTeam(5, "Italy National Team", "Italy", "E. Coach", "https://example.com/wolves.png", 80.0m, "chris@kdg.be");
        var t6 = CreateSeedTeam(6, "Portugal National Team", "Portugal", "F. Coach", "https://example.com/dragons.png", 110.0m, "lars@kdg.be");

        teamsList.AddRange(new[] { t1, t2, t3, t4, t5, t6 });

        void Link(Group g, Team t, int points = 0)
        {
            var gt = new GroupTeam
            {
                Group = g,
                Team = t,
                GroupId = g.Id,
                TeamId = t.Id,
                Points = points
            };
            g.Teams.Add(gt);
            t.Groups.Add(gt);
        }

        Link(g1, t1); Link(g1, t2);
        Link(g2, t2); Link(g2, t3);
        Link(g3, t4);
        Link(g4, t5); Link(g4, t6);
        Link(g5, t1);
        Link(g6, t6);
        Link(g7, t3);
        Link(g8, t2);
    }

    public Team? ReadTeam(int id) => teamsList.SingleOrDefault(team => team.Id == id);

    public Team? ReadTeamWithMaintainer(int id)
    {
        return ReadTeam(id);
    }

    public Team? ReadTeamWithGroups(int id) => ReadTeam(id);

    public IEnumerable<Team> ReadAllTeams() => teamsList;

    public IEnumerable<Team> ReadAllTeamsWithMaintainer()
    {
        return teamsList;
    }

    public IEnumerable<Team> ReadTeamsByCountry(string country)
    {
        return teamsList
            .Where(team => team.Country != null && team.Country.Equals(country, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public IEnumerable<Team> ReadTeamsOfGroup(int groupId)
    {
        return groupsList
            .SingleOrDefault(group => group.Id == groupId)?
            .Teams
            .Select(link => link.Team)
            .ToList() ?? new List<Team>();
    }

    public IEnumerable<Team> ReadAllTeamsWithGroups() => teamsList;

    public void CreateTeam(Team team)
    {
        team.Id = teamsList.Count + 1;
        teamsList.Add(team);
    }

    public void UpdateTeamValue(int teamId, decimal? teamValue)
    {
        var team = ReadTeam(teamId);
        if (team is null)
        {
            return;
        }

        team.TeamValue = teamValue;
    }

    public IdentityUser? ReadUserByUsername(string username)
    {
        return usersList.SingleOrDefault(user => string.Equals(user.UserName, username, StringComparison.OrdinalIgnoreCase));
    }

    public Group? ReadGroup(int id) => groupsList.SingleOrDefault(group => group.Id == id);

    public IEnumerable<Group> ReadAllGroups() => groupsList;

    public IEnumerable<Group> ReadGroupsByChampionship(Championship champ)
    {
        return groupsList.Where(group => group.Championship != null && champ != null && group.Championship.Id == champ.Id).ToList();
    }

    public IEnumerable<Group> ReadGroupsByCodeAndMaxTeams(char? code, int? maxTeams)
    {
        IEnumerable<Group> query = groupsList;

        if (code.HasValue)
            query = query.Where(group => group.GroupCode == code.Value);

        if (maxTeams.HasValue)
            query = query.Where(group => group.MaxTeams == maxTeams.Value);

        return query.ToList();
    }

    public void CreateGroup(Group group)
    {
        group.Id = groupsList.Count + 1;
        groupsList.Add(group);
    }

    public IEnumerable<Group> ReadAllGroupsWithChampionshipAndTeams() => groupsList;

    public void CreateGroupTeam(GroupTeam link)
    {
        var group = ReadGroup(link.GroupId);
        var team = ReadTeam(link.TeamId);
        if (group is null || team is null)
        {
            return;
        }

        if (group.Teams.Any(existing => existing.TeamId == team.Id))
        {
            return;
        }

        link.Group = group;
        link.Team = team;
        group.Teams.Add(link);
        team.Groups.Add(link);
    }

    public void DeleteGroupTeam(int groupId, int teamId)
    {
        var group = ReadGroup(groupId);
        var team = ReadTeam(teamId);
        if (group is null || team is null)
        {
            return;
        }

        var link = group.Teams.SingleOrDefault(existing => existing.TeamId == teamId);
        if (link is null)
        {
            return;
        }

        group.Teams.Remove(link);
        team.Groups.Remove(link);
    }

    public Championship? ReadChampionship(int id) => championshipList.SingleOrDefault(championship => championship.Id == id);

    public IEnumerable<Championship> ReadAllChampionships() => championshipList;

    public void CreateChampionship(Championship championship)
    {
        var created = new Championship
        {
            Id = championshipList.Count + 1,
            ChampionshipEdition = championship.ChampionshipEdition,
            HostCountry = championship.HostCountry,
            Type = championship.Type,
            Groups = championship.Groups
        };
        championshipList.Add(created);
    }

    public IEnumerable<GroupTeam> ReadGroupTeamsOfGroup(int groupId)
    {
        return ReadGroup(groupId)?.Teams.ToList() ?? new List<GroupTeam>();
    }

    private Team CreateSeedTeam(int id, string name, string country, string coach, string logoImage, decimal? teamValue, string maintainerUsername)
    {
        var maintainer = usersList.Single(user => user.UserName == maintainerUsername);
        return new Team
        {
            Id = id,
            Name = name,
            Country = country,
            Coach = coach,
            LogoImage = logoImage,
            TeamValue = teamValue,
            MaintainerId = maintainer.Id,
            Maintainer = maintainer
        };
    }
}
