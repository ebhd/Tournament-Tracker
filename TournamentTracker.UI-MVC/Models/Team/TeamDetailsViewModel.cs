using DomainTeam = TournamentTracker.Domain.Team;

namespace TournamentTracker.UI_MVC.Models.Team;

public sealed class TeamDetailsViewModel
{
    public required DomainTeam Team { get; init; }
    public required string MaintainerName { get; init; }
    public required bool CanEditTeamValue { get; init; }
    public required bool IsAdmin { get; init; }
    public required bool IsAuthenticated { get; init; }
}
