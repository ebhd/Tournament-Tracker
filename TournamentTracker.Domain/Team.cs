using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TournamentTracker.Domain;

public class Team : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    public string Coach { get; set; } = string.Empty;

    [Required]
    [Url]
    public string LogoImage { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal? TeamValue { get; set; }

    [Required]
    public string MaintainerId { get; set; } = string.Empty;

    [Required]
    public IdentityUser Maintainer { get; set; }

    public ICollection<GroupTeam> Groups { get; set; } = new List<GroupTeam>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Coach == Name)
        {
            yield return new ValidationResult("Coach name cannot be the same as team name.", new[] { nameof(Coach), nameof(Name) });
        }
    }

    public override string ToString()
    {
        return "Team Id: " + Id + ", Name: " + Name + ", Country: " + Country + ", Coach: " + Coach + ", Team Value: " + (TeamValue.HasValue ? TeamValue.Value.ToString("C") : "N/A");
    }
}
