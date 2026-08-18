using System.ComponentModel.DataAnnotations;

namespace TournamentTracker.Domain;

public class GroupTeam
{
    public int GroupId { get; set; }
    [Required]
    public Group Group { get; set; }

    public int TeamId { get; set; }
    [Required]
    public Team Team { get; set; }

    public int Points { get; set; }
}
