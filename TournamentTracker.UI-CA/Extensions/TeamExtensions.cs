using TournamentTracker.Domain;

namespace TournamentTracker.UI.CA.Extensions;

internal static class TeamExtensions
{
    internal static string StringRepresentation(this Team team)
    {
        return $"Team Id: {team.Id}, Name: {team.Name}, Country: {team.Country}, Coach: {team.Coach}, Team Value: {(team.TeamValue.HasValue ? team.TeamValue.Value.ToString("C") : "N/A")}";
    }
}
