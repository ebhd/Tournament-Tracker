using System.ComponentModel.DataAnnotations;

namespace TournamentTracker.UI_MVC.Models.Dtos;

public class NewGroupTeamDto
{
    [Required]
    public int? TeamId { get; set; }

    [Range(0, int.MaxValue)]
    public int Points { get; set; }
}
