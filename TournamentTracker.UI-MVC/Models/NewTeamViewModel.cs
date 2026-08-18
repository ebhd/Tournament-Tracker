using System.ComponentModel.DataAnnotations;

namespace TournamentTracker.UI_MVC.Models;

public class NewTeamViewModel : IValidatableObject
{
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    public string Country { get; set; }

    public string Coach { get; set; }

    [Required]
    [Url]
    public string LogoImage { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? TeamValue { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Coach == Name)
        {
            yield return new ValidationResult(
                "Coach name cannot be the same as team name.",
                [nameof(Coach), nameof(Name)]);
        }
    }
}
