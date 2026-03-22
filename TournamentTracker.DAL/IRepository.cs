using Microsoft.AspNetCore.Identity;
using TournamentTracker.Domain;

namespace TournamentTracker.DAL;

public interface IRepository
{
    Team? ReadTeam(int id);
    Team? ReadTeamWithMaintainer(int id);
    Team? ReadTeamWithGroups(int id);
    IEnumerable<Team> ReadAllTeams();
    IEnumerable<Team> ReadAllTeamsWithMaintainer();
    IEnumerable<Team> ReadTeamsByCountry(string country);
    IEnumerable<Team> ReadTeamsOfGroup(int groupId);
    IEnumerable<Team> ReadAllTeamsWithGroups();
    void CreateTeam(Team team);
    void UpdateTeamValue(int teamId, decimal? teamValue);

    IdentityUser? ReadUserByUsername(string username);

    Group? ReadGroup(int id);
    IEnumerable<Group> ReadAllGroups();
    IEnumerable<Group> ReadGroupsByChampionship(Championship champ);
    IEnumerable<Group> ReadGroupsByCodeAndMaxTeams(char? code, int? maxTeams);
    void CreateGroup(Group group);
    IEnumerable<Group> ReadAllGroupsWithChampionshipAndTeams();

    void CreateGroupTeam(GroupTeam link);
    void DeleteGroupTeam(int groupId, int teamId);

    Championship? ReadChampionship(int id);
    IEnumerable<Championship> ReadAllChampionships();
    void CreateChampionship(Championship championship);

    IEnumerable<GroupTeam> ReadGroupTeamsOfGroup(int groupId);
}
