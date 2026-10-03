using TournamentTracker.Domain;


namespace TournamentTracker.DAL.EF;

public static class Seeder
{
    public static void Seed(LeagueDbContext context)
    {
        var championships = context.Championships;
        var groups = context.Groups;
        var teams = context.Teams;
        var groupTeams = context.GroupTeams;

        var c1 = new Championship { ChampionshipEdition = new DateTime(2022, 11, 21), HostCountry = "Qatar", Type = ChampionshipType.WorldCup };
        var c2 = new Championship { ChampionshipEdition = new DateTime(2018, 06, 14), HostCountry = "Russia", Type = ChampionshipType.WorldCup };
        var c3 = new Championship { ChampionshipEdition = new DateTime(2014, 06, 12), HostCountry = "Brazil", Type = ChampionshipType.WorldCup };
        var c4 = new Championship { ChampionshipEdition = new DateTime(2010, 06, 11), HostCountry = "South Africa", Type = ChampionshipType.WorldCup };
        var c5 = new Championship { ChampionshipEdition = new DateTime(2006, 06, 09), HostCountry = "Germany", Type = ChampionshipType.WorldCup };
        championships.AddRange(c1, c2, c3, c4, c5);

        var g1 = new Group { GroupCode = 'A', MaxTeams = 4, Championship = c1 };
        var g2 = new Group { GroupCode = 'B', MaxTeams = 4, Championship = c1 };
        var g3 = new Group { GroupCode = 'C', MaxTeams = 4, Championship = c1 };
        var g4 = new Group { GroupCode = 'D', MaxTeams = 4, Championship = c1 };

        var g5 = new Group { GroupCode = 'A', MaxTeams = 2, Championship = c2 };
        var g6 = new Group { GroupCode = 'B', MaxTeams = 4, Championship = c2 };
        var g7 = new Group { GroupCode = 'C', MaxTeams = 2, Championship = c2 };
        var g8 = new Group { GroupCode = 'D', MaxTeams = 3, Championship = c2 };
        
        var g9 = new Group { GroupCode = 'A', MaxTeams = 4, Championship = c3};
        var g10 = new Group { GroupCode = 'A', MaxTeams = 4, Championship = c4 };
        var g11 = new Group { GroupCode = 'A', MaxTeams = 4, Championship = c5 };
        
        groups.AddRange(
            g1, g2, g3, g4,
            g5, g6, g7, g8,
            g9, g10, g11
        );
        // Fixed demonstration snapshot; squad market values are estimates in euros.
        var t1 = new Team { Name = "Belgische National Team", Country = "Belgium", Coach = "Mark van Bommel", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3382.png", TeamValue = 547_500_000m };
        var t2 = new Team { Name = "France National Team", Country = "France", Coach = "Zinedine Zidane", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3377.png", TeamValue = 1_520_000_000m };
        var t3 = new Team { Name = "Spain National Team", Country = "Spain", Coach = "Luis de la Fuente", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3375.png", TeamValue = 1_220_000_000m };
        var t4 = new Team { Name = "Germany National Team", Country = "Germany", Coach = "Jürgen Klopp", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3262.png", TeamValue = 947_000_000m };
        var t5 = new Team { Name = "Italy National Team", Country = "Italy", Coach = "Roberto Mancini", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3376.png", TeamValue = 833_500_000m };
        var t6 = new Team { Name = "Portugal National Team", Country = "Portugal", Coach = "Jorge Jesus", LogoImage = "https://tmssl.akamaized.net/images/wappen/big/3300.png", TeamValue = 1_010_000_000m };

        teams.AddRange(t1, t2, t3, t4, t5, t6);

        groupTeams.AddRange(
            new GroupTeam { Group = g1, Team = t1, Points = 0 },
            new GroupTeam { Group = g1, Team = t2, Points = 0 },

            new GroupTeam { Group = g2, Team = t2, Points = 0 },
            new GroupTeam { Group = g2, Team = t3, Points = 0 },

            new GroupTeam { Group = g3, Team = t4, Points = 0 },

            new GroupTeam { Group = g4, Team = t5, Points = 0 },
            new GroupTeam { Group = g4, Team = t6, Points = 0 },

            new GroupTeam { Group = g5, Team = t1, Points = 0 },
            new GroupTeam { Group = g6, Team = t6, Points = 0 },
            new GroupTeam { Group = g7, Team = t3, Points = 0 },
            new GroupTeam { Group = g8, Team = t2, Points = 0 }
        );

        context.SaveChanges();
        context.ChangeTracker.Clear();
    }
}
