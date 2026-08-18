
using System.ComponentModel.DataAnnotations;
using TournamentTracker.Domain;
using TournamentTracker.DAL;
namespace TournamentTracker.BL;

    public class Manager : IManager
    {
        private readonly IRepository _repository;

        public Manager(IRepository repository)
        {
            _repository = repository;
        }

        public Team GetTeam(int id)
        {
            return _repository.ReadTeam(id);
        }

        public Team GetTeamWithGroupsAndChampionships(int id)
        {
            return _repository.ReadTeamWithGroupsAndChampionships(id);
        }

        public IEnumerable<Team> GetAllTeams()
        {
            return _repository.ReadAllTeams();
        }

        public IEnumerable<Team> GetAllTeamsWithGroups()
        {
            return _repository.ReadAllTeamsWithGroups();
        }

        public IEnumerable<Team> GetTeamsByCountry(string country)
        {
            return _repository.ReadTeamsByCountry(country);
        }

        public Team AddTeam(string name, string country, string coach, string logoImage, decimal? teamValue)
        {
            var team = new Team { Name = name, Country = country, Coach = coach, LogoImage = logoImage, TeamValue = teamValue };

            var validationResults = new List<ValidationResult>();
            bool success = Validator.TryValidateObject(team,
                new ValidationContext(team), validationResults,
                validateAllProperties: true);
            if (!success)
            {
                var message = "";
                foreach (var validationResult in validationResults)
                {
                    message += validationResult.ErrorMessage + " ";
                }

                throw new ValidationException(message);
            }

            _repository.CreateTeam(team);
            return team;
        }

        public Group GetGroupWithChampionship(int id)
        {
            return _repository.ReadGroupWithChampionship(id);
        }

        public IEnumerable<Group> GetAllGroups()
        {
            return _repository.ReadAllGroups();
        }

        public IEnumerable<Group> GetAllGroupsWithChampionshipAndTeams()
        {
            return _repository.ReadAllGroupsWithChampionshipAndTeams();
        }

        public IEnumerable<Group> GetGroupsByChampionship(int championshipId)
        {
            return _repository.ReadGroupsByChampionship(championshipId);
        }
        public IEnumerable<Group> GetGroupsByCodeAndMaxTeams(char? code, int? maxTeams)
        {
            return _repository.ReadGroupsByCodeAndMaxTeams(code, maxTeams);
        }

        public IEnumerable<Team> GetTeamsOfGroup(int groupId)
        {
            return _repository.ReadTeamsOfGroup(groupId);
        }
       
        public Group AddGroup(int maxTeams, char groupCode, int championshipId)
        {
            var championship = _repository.ReadChampionship(championshipId);
            if (championship == null)
            {
                throw new ValidationException($"Championship with id {championshipId} does not exist");
            }

            var group = new Group { MaxTeams = maxTeams, GroupCode = groupCode, Championship = championship };

            var validationResults = new List<ValidationResult>();
            bool success = Validator.TryValidateObject(group,
                new ValidationContext(group), validationResults,
                validateAllProperties: true);
            if (!success)
            {
                var message = "";
                foreach (var validationResult in validationResults)
                {
                    message += validationResult.ErrorMessage + " ";
                }

                throw new ValidationException(message);
            }

            _repository.CreateGroup(group);
            return group;
        }

    public Championship GetChampionshipWithGroups(int id)
    {
        return _repository.ReadChampionshipWithGroups(id);
    }

    public IEnumerable<Championship> GetAllChampionshipsWithGroups()
    {
        return _repository.ReadAllChampionshipsWithGroups();
    }

    public Championship AddChampionship(DateTime edition, string hostCountry, ChampionshipType type)
    {
        var championship = new Championship
        {
            ChampionshipEdition = edition,
            HostCountry = hostCountry,
            Type = type
        };

        var validationResults = new List<ValidationResult>();
        bool success = Validator.TryValidateObject(championship,
            new ValidationContext(championship), validationResults,
            validateAllProperties: true);
        if (!success)
        {
            var message = "";
            foreach (var validationResult in validationResults)
            {
                message += validationResult.ErrorMessage + " ";
            }

            throw new ValidationException(message);
        }

        _repository.CreateChampionship(championship);
        return championship;
    }

    public GroupTeam GetGroupTeamWithTeam(int groupId, int teamId)
    {
        return _repository.ReadGroupTeamWithTeam(groupId, teamId);
    }

    public IEnumerable<GroupTeam> GetGroupTeamsOfGroupWithTeam(int groupId)
    {
        return _repository.ReadGroupTeamsOfGroupWithTeam(groupId);
    }

    public GroupTeam AddTeamToGroup(int teamId, int groupId, int points = 0)
    {
        var team = _repository.ReadTeam(teamId);
        if (team == null)
            throw new ValidationException($"Team with id {teamId} does not exist.");

        var group = _repository.ReadGroupWithChampionship(groupId);
        if (group == null)
            throw new ValidationException($"Group with id {groupId} does not exist.");

        var existingGroupTeam = _repository.ReadGroupTeamWithTeam(groupId, teamId);
        if (existingGroupTeam != null)
            throw new ValidationException($"Team with id {teamId} is already linked to group with id {groupId}.");

        int teamCount = _repository.ReadGroupTeamsOfGroupWithTeam(groupId).Count();
        if (teamCount >= group.MaxTeams)
            throw new ValidationException($"Group with id {groupId} already contains the maximum number of teams.");

        var groupTeam = new GroupTeam
        {
            TeamId = teamId,
            Team = team,
            GroupId = groupId,
            Group = group,
            Points = points
        };

        _repository.CreateGroupTeam(groupTeam);
        return groupTeam;
    }

     public void RemoveTeamFromGroup(int teamId, int groupId)
    {
        _repository.DeleteGroupTeam(groupId, teamId);
    }

}
