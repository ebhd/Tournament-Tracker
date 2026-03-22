using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TournamentTracker.Domain;

namespace TournamentTracker.DAL.EF;

public class Repository : IRepository
{
    public LeagueDbContext _context { get; set; }

    public Repository(LeagueDbContext context)
    {
        _context = context;
    }

    public Team? ReadTeam(int id)
    {
        return _context.Teams.SingleOrDefault(team => team.Id == id);
    }

    public Team? ReadTeamWithMaintainer(int id)
    {
        return _context.Teams
            .Include(team => team.Maintainer)
            .SingleOrDefault(team => team.Id == id);
    }

    public IEnumerable<Team> ReadAllTeams()
    {
        return _context.Teams.ToList();
    }

    public IEnumerable<Team> ReadAllTeamsWithMaintainer()
    {
        return _context.Teams
            .Include(team => team.Maintainer)
            .ToList();
    }

    public IEnumerable<Team> ReadTeamsByCountry(string country)
    {
        return _context.Teams
            .Where(team => team.Country == country)
            .ToList();
    }

    public void CreateTeam(Team team)
    {
        if (team != null)
        {
            _context.Teams.Add(team);
            _context.SaveChanges();
        }
    }

    public void UpdateTeamValue(int teamId, decimal? teamValue)
    {
        var team = _context.Teams.SingleOrDefault(existingTeam => existingTeam.Id == teamId);
        if (team is null)
        {
            return;
        }

        team.TeamValue = teamValue;
        _context.SaveChanges();
    }

    public IdentityUser? ReadUserByUsername(string username)
    {
        return _context.Users.SingleOrDefault(user => user.UserName == username);
    }

    public Group? ReadGroup(int id)
    {
        return _context.Groups
            .Include(g => g.Championship)
            .SingleOrDefault(g => g.Id == id);
    }

    public IEnumerable<Group> ReadAllGroups()
    {
        return _context.Groups.ToList();
    }

    public IEnumerable<Group> ReadGroupsByChampionship(Championship champ)
    {
        return _context.Groups.Where(g => g.Championship.Id == champ.Id).ToList();
    }

    public IEnumerable<Group> ReadGroupsByCodeAndMaxTeams(char? code, int? maxTeams)
    {
        IQueryable<Group> q = _context.Groups;

        if (code.HasValue)
            q = q.Where(g => g.GroupCode == code.Value);

        if (maxTeams.HasValue)
            q = q.Where(g => g.MaxTeams == maxTeams.Value);

        return q.ToList();
    }

    public void CreateGroup(Group group)
    {
        if (group != null)
        {
            _context.Add(group);
            _context.SaveChanges();
        }
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

    public Team? ReadTeamWithGroups(int id)
    {
        return _context.Teams
            .Include(t => t.Maintainer)
            .Include(t => t.Groups)
                .ThenInclude(gt => gt.Group)
                    .ThenInclude(g => g.Championship)
            .SingleOrDefault(t => t.Id == id);
    }

    public Championship? ReadChampionship(int id)
    {
        return _context.Champions
            .Include(c => c.Groups)
            .SingleOrDefault(c => c.Id == id);
    }

    public IEnumerable<Championship> ReadAllChampionships()
    {
        return _context.Champions
            .Include(c => c.Groups)
            .ToList();
    }

    public void CreateChampionship(Championship championship)
    {
        _context.Champions.Add(championship);
        _context.SaveChanges();
    }

    public IEnumerable<GroupTeam> ReadGroupTeamsOfGroup(int groupId)
    {
        return _context.GroupTeams
            .Where(gt => gt.GroupId == groupId)
            .Include(gt => gt.Team)
            .ToList();
    }
}
