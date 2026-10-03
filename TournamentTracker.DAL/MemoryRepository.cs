using TournamentTracker.Domain;


namespace TournamentTracker.DAL;

public class MemoryRepository : IRepository
{
    private static readonly ICollection<Championship> _championships = new List<Championship>();
    private static readonly ICollection<Team> _teams = new List<Team>();
    private static readonly ICollection<Group> _groups = new List<Group>();

    public static void Seed()
    {
        // Championships
        var c1 = new Championship { Id = 1, ChampionshipEdition = new DateTime(2022, 11, 21), HostCountry = "Qatar", Type = ChampionshipType.WorldCup };
        var c2 = new Championship { Id = 2, ChampionshipEdition = new DateTime(2018, 06, 14), HostCountry = "Russia", Type = ChampionshipType.WorldCup };
        var c3 = new Championship { Id = 3, ChampionshipEdition = new DateTime(2014, 06, 12), HostCountry = "Brazil", Type = ChampionshipType.WorldCup };
        var c4 = new Championship { Id = 4, ChampionshipEdition = new DateTime(2010, 06, 11), HostCountry = "South Africa", Type = ChampionshipType.WorldCup };
        _championships.Add(c1);
        _championships.Add(c2);
        _championships.Add(c3);
        _championships.Add(c4);

        // Groups 
        var g1 = new Group { Id = 1, GroupCode = 'A', MaxTeams = 4, Championship = c1 };
        var g2 = new Group { Id = 2, GroupCode = 'B', MaxTeams = 4, Championship = c1 };
        var g3 = new Group { Id = 3, GroupCode = 'C', MaxTeams = 4, Championship = c1 };
        var g4 = new Group { Id = 4, GroupCode = 'D', MaxTeams = 4, Championship = c1 };

        var g5 = new Group { Id = 5, GroupCode = 'A', MaxTeams = 2, Championship = c2 };
        var g6 = new Group { Id = 6, GroupCode = 'B', MaxTeams = 4, Championship = c2 };
        var g7 = new Group { Id = 7, GroupCode = 'C', MaxTeams = 2, Championship = c2 };
        var g8 = new Group { Id = 8, GroupCode = 'D', MaxTeams = 3, Championship = c2 };

        c1.Groups.Add(g1); c1.Groups.Add(g2); c1.Groups.Add(g3); c1.Groups.Add(g4);
        c2.Groups.Add(g5); c2.Groups.Add(g6); c2.Groups.Add(g7); c2.Groups.Add(g8);

        _groups.Add(g1);
        _groups.Add(g2);
        _groups.Add(g3);
        _groups.Add(g4);
        _groups.Add(g5);
        _groups.Add(g6);
        _groups.Add(g7);
        _groups.Add(g8);

        // Teams
        // Fixed demonstration snapshot; squad market values are estimates in euros.
        var t1 = new Team { Id = 1, Name = "Belgische National Team", Country = "Belgium", Coach = "Mark van Bommel", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3382.png", TeamValue = 547_500_000m };
        var t2 = new Team { Id = 2, Name = "France National Team", Country = "France", Coach = "Zinedine Zidane", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3377.png", TeamValue = 1_520_000_000m };
        var t3 = new Team { Id = 3, Name = "Spain National Team", Country = "Spain", Coach = "Luis de la Fuente", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3375.png", TeamValue = 1_220_000_000m };
        var t4 = new Team { Id = 4, Name = "Germany National Team", Country = "Germany", Coach = "Jürgen Klopp", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3262.png", TeamValue = 947_000_000m };
        var t5 = new Team { Id = 5, Name = "Italy National Team", Country = "Italy", Coach = "Roberto Mancini", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3376.png", TeamValue = 833_500_000m };
        var t6 = new Team { Id = 6, Name = "Portugal National Team", Country = "Portugal", Coach = "Jorge Jesus", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3300.png", TeamValue = 1_010_000_000m };

        _teams.Add(t1);
        _teams.Add(t2);
        _teams.Add(t3);
        _teams.Add(t4);
        _teams.Add(t5);
        _teams.Add(t6);

        // link 
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

        // Relationships (same as before but via GroupTeam)
        Link(g1, t1); Link(g1, t2);
        Link(g2, t2); Link(g2, t3);
        Link(g3, t4);
        Link(g4, t5); Link(g4, t6);

        Link(g5, t1);
        Link(g6, t6);
        Link(g7, t3);
        Link(g8, t2);
    }
    public Team ReadTeam(int id)
    {
        foreach (var team in _teams)
        {
            if (team.Id == id) return team;
        }
        return null;
    }

    public IEnumerable<Team> ReadAllTeams()
    {
        return _teams;
    }

    public IEnumerable<Team> ReadTeamsByCountry(string country)
    {
        var list = new List<Team>();
        foreach (var team in _teams)
        {
            if (team.Country != null && team.Country.Equals(country, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(team);
            }
        }
        return list;
    }

    public void CreateTeam(Team team)
    {
        team.Id = _teams.Max(existingTeam => existingTeam.Id) + 1;
        _teams.Add(team);
    }

    public void UpdateTeamValue(int teamId, decimal? teamValue)
    {
        var team = ReadTeam(teamId);
        if (team != null) team.TeamValue = teamValue;
    }

    public Group ReadGroupWithChampionship(int id)
    {
        foreach (var group in _groups)
        {
            if (group.Id == id) return group;
        }
        return null;
    }

    public IEnumerable<Group> ReadAllGroups()
    {
        return _groups;
    }

    public IEnumerable<Group> ReadGroupsByChampionship(int championshipId)
    {
        return _groups
            .Where(group => group.Championship?.Id == championshipId)
            .ToList();
    }
    public IEnumerable<Group> ReadGroupsByCodeAndMaxTeams(char? code, int? maxTeams)
    {
        IEnumerable<Group> result = _groups;

        if (code.HasValue)
            result = result.Where(g => g.GroupCode == code.Value);

        if (maxTeams.HasValue)
            result = result.Where(g => g.MaxTeams == maxTeams.Value);

        return result.ToList();
    }
    public void CreateGroup(Group group)
    {
        group.Id = _groups.Max(existingGroup => existingGroup.Id) + 1;
        _groups.Add(group);
    }

    public IEnumerable<Group> ReadAllGroupsWithChampionshipAndTeams()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Team> ReadAllTeamsWithGroups()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Team> ReadTeamsOfGroup(int groupId)
    {
        throw new NotImplementedException();
    }
    public void CreateGroupTeam(GroupTeam link)
    {
        throw new NotImplementedException();
    }
    public void DeleteGroupTeam(int groupId, int teamId)
    {
        throw new NotImplementedException();
    }

    public Team ReadTeamWithGroupsAndChampionships(int id)
    {
        throw new NotImplementedException();
    }
    
    public Championship ReadChampionship(int id)
    {
        foreach (var championship in _championships)
        {
            if (championship.Id == id) return championship;
        }
        return null;
    }

    public Championship ReadChampionshipWithGroups(int id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Championship> ReadAllChampionshipsWithGroups()
    {
        throw new NotImplementedException();
    }

    public void CreateChampionship(Championship championship)
    {
        throw new NotImplementedException();
    }

    public GroupTeam ReadGroupTeamWithTeam(int groupId, int teamId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<GroupTeam> ReadGroupTeamsOfGroupWithTeam(int groupId)
    {
        throw new NotImplementedException();
    }
}
