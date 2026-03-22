using TournamentTracker.Domain;

namespace TournamentTracker.BL;

public interface IManager
{
    Team? GetTeam(int id);
    bool UpdateTeamValue(int teamId, decimal? teamValue, string? username, bool isAdmin);
    Team? GetTeamWithMaintainer(int id);
    IEnumerable<Team> GetAllTeams();
    IEnumerable<Team> GetAllTeamsWithMaintainer();
    IEnumerable<Team> GetTeamsByCountry(string country);
    Team AddTeam(string name, string country, string coach, string logoImage, decimal? teamValue, string maintainerUsername);

    Group? GetGroup(int id);
    IEnumerable<Group> GetAllGroups();
    IEnumerable<Group> GetGroupsByChampionship(Championship champ);
    IEnumerable<Group> GetGroupsByCodeAndMaxTeams(char? code, int? maxTeams);

    Group AddGroup(int maxTeams, char groupCode, Championship champ);

    IEnumerable<Group> GetAllGroupsWithChampionshipAndTeams();
    IEnumerable<Team> GetAllTeamsWithGroups();

    IEnumerable<Team> GetTeamsOfGroup(int groupId);
    void AddTeamToGroup(int teamId, int groupId, int points = 0);
    void RemoveTeamFromGroup(int teamId, int groupId);

    Team? GetTeamWithGroups(int id);

    Championship? GetChampionship(int id);
    IEnumerable<Championship> GetAllChampionships();
    Championship AddChampionship(DateTime edition, string hostCountry, ChampionshipType type);

    IEnumerable<GroupTeam> GetGroupTeamsOfGroup(int groupId);
}
