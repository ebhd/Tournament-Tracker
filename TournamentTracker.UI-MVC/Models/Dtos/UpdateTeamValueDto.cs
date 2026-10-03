using System.ComponentModel.DataAnnotations;

namespace TournamentTracker.UI_MVC.Models.Dtos;

public class UpdateTeamValueDto
{
    [Range(0, double.MaxValue)]
    public decimal? TeamValue { get; set; }
}
