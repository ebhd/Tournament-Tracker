using TournamentTracker.Domain;

namespace TournamentTracker.UI.CA.Extensions;

internal static class GroupExtensions
{
    internal static string StringRepresentation(this Group group)
    {
        return $"Group Id: {group.Id} Group Code: {group.GroupCode}, Max Teams: {group.MaxTeams}, Total Teams: {group.Teams.Count}";
    }
}
