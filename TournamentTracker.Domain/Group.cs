using System.ComponentModel.DataAnnotations;

namespace TournamentTracker.Domain;
public class Group
{
    public int Id { get; set; }

    [Range(2, 10)]
    public int MaxTeams { get; set; }
    public char GroupCode { get; set; }

    public ICollection<GroupTeam> Teams { get; set; } = new List<GroupTeam>();

    [Required]
    public Championship Championship { get; set; }

}
