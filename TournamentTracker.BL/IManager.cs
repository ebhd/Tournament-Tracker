using TournamentTracker.Domain;

namespace TournamentTracker.BL;

public interface IManager
{
    Team GetTeam(int id);
    Team GetTeamWithGroupsAndChampionships(int id);
    IEnumerable<Team> GetAllTeams();
    IEnumerable<Team> GetAllTeamsWithGroups();
    IEnumerable<Team> GetTeamsByCountry(string country);
    Team AddTeam(string name, string country, string coach, string logoImage, decimal? teamValue);

    Group GetGroupWithChampionship(int id);
    IEnumerable<Group> GetAllGroups();
    IEnumerable<Group> GetAllGroupsWithChampionshipAndTeams();
    IEnumerable<Group> GetGroupsByChampionship(int championshipId);
    IEnumerable<Group> GetGroupsByCodeAndMaxTeams(char? code, int? maxTeams);
    IEnumerable<Team> GetTeamsOfGroup(int groupId);
    Group AddGroup(int maxTeams, char groupCode, int championshipId);

    Championship GetChampionshipWithGroups(int id);
    IEnumerable<Championship> GetAllChampionshipsWithGroups();
    Championship AddChampionship(DateTime edition, string hostCountry, ChampionshipType type);

    GroupTeam GetGroupTeamWithTeam(int groupId, int teamId);
    IEnumerable<GroupTeam> GetGroupTeamsOfGroupWithTeam(int groupId);
    GroupTeam AddTeamToGroup(int teamId, int groupId, int points = 0);
    void RemoveTeamFromGroup(int teamId, int groupId);
}
