using System.ComponentModel.DataAnnotations;
using TournamentTracker.DAL;
using TournamentTracker.Domain;

namespace TournamentTracker.BL;

public class Manager : IManager
{
    private readonly IRepository _repository;

    public Manager(IRepository repository)
    {
        _repository = repository;
    }

    public Team? GetTeam(int id)
    {
        return _repository.ReadTeam(id);
    }

    public bool UpdateTeamValue(int teamId, decimal? teamValue, string? username, bool isAdmin)
    {
        var team = _repository.ReadTeamWithMaintainer(teamId);
        if (team is null)
        {
            return false;
        }

        var isMaintainer = !string.IsNullOrWhiteSpace(username) &&
                           string.Equals(team.Maintainer.UserName, username, StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && !isMaintainer)
        {
            return false;
        }

        _repository.UpdateTeamValue(teamId, teamValue);
        return true;
    }

    public Team? GetTeamWithMaintainer(int id)
    {
        return _repository.ReadTeamWithMaintainer(id);
    }

    public IEnumerable<Team> GetAllTeams()
    {
        return _repository.ReadAllTeams();
    }

    public IEnumerable<Team> GetAllTeamsWithMaintainer()
    {
        return _repository.ReadAllTeamsWithMaintainer();
    }

    public IEnumerable<Team> GetTeamsByCountry(string country)
    {
        return _repository.ReadTeamsByCountry(country);
    }

    public Team AddTeam(string name, string country, string coach, string logoImage, decimal? teamValue, string maintainerUsername)
    {
        if (string.IsNullOrWhiteSpace(maintainerUsername))
        {
            throw new ValidationException("A maintainer username is required to create a team.");
        }

        var maintainer = _repository.ReadUserByUsername(maintainerUsername);
        if (maintainer is null)
        {
            throw new ValidationException($"No identity user found for username '{maintainerUsername}'.");
        }

        var team = new Team
        {
            Name = name,
            Country = country,
            Coach = coach,
            LogoImage = logoImage,
            TeamValue = teamValue,
            MaintainerId = maintainer.Id,
            Maintainer = maintainer
        };

        var ctx = new ValidationContext(team);
        Validator.ValidateObject(team, ctx, true);
        _repository.CreateTeam(team);
        return team;
    }

    public Group? GetGroup(int id)
    {
        return _repository.ReadGroup(id);
    }

    public IEnumerable<Group> GetAllGroups()
    {
        return _repository.ReadAllGroups();
    }

    public IEnumerable<Group> GetGroupsByChampionship(Championship champ)
    {
        return _repository.ReadGroupsByChampionship(champ);
    }

    public IEnumerable<Group> GetGroupsByCodeAndMaxTeams(char? code, int? maxTeams)
    {
        return _repository.ReadGroupsByCodeAndMaxTeams(code, maxTeams);
    }

    public Group AddGroup(int maxTeams, char groupCode, Championship champ)
    {
        var g = new Group { MaxTeams = maxTeams, GroupCode = groupCode, Championship = champ };
        var ctx = new ValidationContext(g);
        Validator.ValidateObject(g, ctx, true);
        _repository.CreateGroup(g);
        return g;
    }

    public IEnumerable<Group> GetAllGroupsWithChampionshipAndTeams()
    {
        return _repository.ReadAllGroupsWithChampionshipAndTeams();
    }

    public IEnumerable<Team> GetAllTeamsWithGroups()
    {
        return _repository.ReadAllTeamsWithGroups();
    }

    public IEnumerable<Team> GetTeamsOfGroup(int groupId)
    {
        return _repository.ReadTeamsOfGroup(groupId);
    }

    public void AddTeamToGroup(int teamId, int groupId, int points = 0)
    {
        var team = _repository.ReadTeam(teamId);
        if (team == null)
            throw new ValidationException($"Team with id {teamId} does not exist.");

        var group = _repository.ReadGroup(groupId);
        if (group == null)
            throw new ValidationException($"Group with id {groupId} does not exist.");

        var link = new GroupTeam
        {
            TeamId = teamId,
            GroupId = groupId,
            Points = points
        };

        _repository.CreateGroupTeam(link);
    }

    public void RemoveTeamFromGroup(int teamId, int groupId)
    {
        _repository.DeleteGroupTeam(groupId, teamId);
    }

    public Team? GetTeamWithGroups(int id)
    {
        return _repository.ReadTeamWithGroups(id);
    }

    public Championship? GetChampionship(int id)
    {
        return _repository.ReadChampionship(id);
    }

    public IEnumerable<Championship> GetAllChampionships()
    {
        return _repository.ReadAllChampionships();
    }

    public Championship AddChampionship(DateTime edition, string hostCountry, ChampionshipType type)
    {
        var c = new Championship
        {
            ChampionshipEdition = edition,
            HostCountry = hostCountry,
            Type = type
        };

        var ctx = new ValidationContext(c);
        Validator.ValidateObject(c, ctx, true);

        _repository.CreateChampionship(c);
        return c;
    }

    public IEnumerable<GroupTeam> GetGroupTeamsOfGroup(int groupId)
    {
        return _repository.ReadGroupTeamsOfGroup(groupId);
    }
}
