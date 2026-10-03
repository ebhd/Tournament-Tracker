using System.ComponentModel.DataAnnotations;

namespace TournamentTracker.Domain;
public class Team : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string Name { get; set;  }
    [Required]
    public string Country { get; set; }
    public string Coach { get; set; }
    [Required]
    [Url]
    public string LogoImage { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? TeamValue { get; set; }

    // Null for teams created through the original, identity-free console workflow.
    public string MaintainerId { get; set; }

    public ICollection<GroupTeam> Groups { get; set; } = new List<GroupTeam>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Coach == Name)
        {
            yield return new ValidationResult("Coach name cannot be the same as team name.", new[] { nameof(Coach), nameof(Name) });
        }

    }
}
