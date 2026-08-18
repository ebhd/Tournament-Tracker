using TournamentTracker.Domain;

namespace TournamentTracker.DAL;

public interface IRepository
{
    Team ReadTeam(int id);
    Team ReadTeamWithGroupsAndChampionships(int id);
    IEnumerable<Team> ReadAllTeams();
    IEnumerable<Team> ReadAllTeamsWithGroups();
    IEnumerable<Team> ReadTeamsByCountry(string country);
    void CreateTeam(Team team);

    Group ReadGroupWithChampionship(int id);
    IEnumerable<Group> ReadAllGroups();
    IEnumerable<Group> ReadAllGroupsWithChampionshipAndTeams();
    IEnumerable<Group> ReadGroupsByChampionship(int championshipId);
    IEnumerable<Group> ReadGroupsByCodeAndMaxTeams(char? code, int? maxTeams);
    IEnumerable<Team> ReadTeamsOfGroup(int groupId);
    void CreateGroup(Group group);

    Championship ReadChampionship(int id);
    Championship ReadChampionshipWithGroups(int id);
    IEnumerable<Championship> ReadAllChampionshipsWithGroups();
    void CreateChampionship(Championship championship);

    GroupTeam ReadGroupTeamWithTeam(int groupId, int teamId);
    IEnumerable<GroupTeam> ReadGroupTeamsOfGroupWithTeam(int groupId);
    void CreateGroupTeam(GroupTeam link);
    void DeleteGroupTeam(int groupId, int teamId);
}
