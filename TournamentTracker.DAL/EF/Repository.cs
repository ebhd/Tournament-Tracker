using Microsoft.EntityFrameworkCore;
using TournamentTracker.Domain;


namespace TournamentTracker.DAL.EF;

 public class Repository : IRepository
 {
    private readonly LeagueDbContext _context;

    public Repository(LeagueDbContext context)
    {
        _context = context;
    }

    public Team ReadTeam(int id)
    {
      return _context.Teams.Find(id);
    }

    public IEnumerable<Team> ReadAllTeams()
    {
        return _context.Teams.ToList();
    }

    public IEnumerable<Team> ReadTeamsByCountry(string country)
    {
        return _context.Teams.Where(t => t.Country == country).ToList();
    }

    public void CreateTeam(Team team)
    {
        _context.Teams.Add(team);
        _context.SaveChanges();
    }

    public void UpdateTeamValue(int teamId, decimal? teamValue)
    {
        var team = ReadTeam(teamId);
        if (team == null) return;
        team.TeamValue = teamValue;
        _context.SaveChanges();
    }

    public Group ReadGroupWithChampionship(int id)
    {
        return _context.Groups
            .Include(g => g.Championship)
            .SingleOrDefault(g => g.Id == id);

    }

    public IEnumerable<Group> ReadAllGroups()
    {
        return _context.Groups.ToList();
    }

    public IEnumerable<Group> ReadGroupsByChampionship(int championshipId)
    {
        return _context.Groups
            .Where(group => group.Championship.Id == championshipId)
            .ToList();
    }

    public IEnumerable<Group> ReadGroupsByCodeAndMaxTeams(char? code, int? maxTeams)
    {
        IQueryable<Group> result = _context.Groups;

        if (code.HasValue)
            result = result.Where(g => g.GroupCode == code.Value);

        if (maxTeams.HasValue)
            result = result.Where(g => g.MaxTeams == maxTeams.Value);

        return result.ToList();
    }

    public void CreateGroup(Group group)
    {
        _context.Groups.Add(group);
        _context.SaveChanges();
    }


    public IEnumerable<Group> ReadAllGroupsWithChampionshipAndTeams()
    {
        return _context.Groups
            .Include(g => g.Championship)
            .Include(g => g.Teams)
                .ThenInclude(gt => gt.Team)
            .ToList();
    }

    public IEnumerable<Team> ReadAllTeamsWithGroups()
    {
        return _context.Teams
            .Include(t => t.Groups)
                .ThenInclude(gt => gt.Group)
            .ToList();
    }



    public IEnumerable<Team> ReadTeamsOfGroup(int groupId)
    {
        return _context.GroupTeams
            .Where(gt => gt.GroupId == groupId)
            .Include(gt => gt.Team)
            .Select(gt => gt.Team)
            .ToList();
    }

    public void CreateGroupTeam(GroupTeam link)
    {
        bool exists = _context.GroupTeams.Any(gt =>
            gt.GroupId == link.GroupId && gt.TeamId == link.TeamId);

        if (exists) return;

        _context.GroupTeams.Add(link);
        _context.SaveChanges();
    }

    public void DeleteGroupTeam(int groupId, int teamId)
    {
        var link = _context.GroupTeams.Find(groupId, teamId);
        if (link == null) return;

        _context.GroupTeams.Remove(link);
        _context.SaveChanges();
    }
    
    
    public Team ReadTeamWithGroupsAndChampionships(int id)
    {
        return _context.Teams
            .Include(t => t.Groups)
            .ThenInclude(gt => gt.Group)
            .ThenInclude(g => g.Championship)
            .SingleOrDefault(t => t.Id == id);
    }
    
    
    public Championship ReadChampionship(int id)
    {
        return _context.Championships.Find(id);
    }

    public Championship ReadChampionshipWithGroups(int id)
    {
        return _context.Championships
            .Include(c => c.Groups)
            .SingleOrDefault(c => c.Id == id);
    }

    public IEnumerable<Championship> ReadAllChampionshipsWithGroups()
    {
        return _context.Championships
            .Include(c => c.Groups) // so Groups.Count works
            .ToList();
    }

    public void CreateChampionship(Championship championship)
    {
        _context.Championships.Add(championship);
        _context.SaveChanges();
    }

    public GroupTeam ReadGroupTeamWithTeam(int groupId, int teamId)
    {
        return _context.GroupTeams
            .Include(gt => gt.Team)
            .SingleOrDefault(gt => gt.GroupId == groupId && gt.TeamId == teamId);
    }

    public IEnumerable<GroupTeam> ReadGroupTeamsOfGroupWithTeam(int groupId)
    {
        return _context.GroupTeams
            .Where(gt => gt.GroupId == groupId)
            .Include(gt => gt.Team)     
            .ToList();
    }

}
