using System.ComponentModel.DataAnnotations;
using TournamentTracker.Domain;

namespace TournamentTracker.UI_MVC.Models.Dtos;

public class NewChampionshipDto
{
    [DataType(DataType.Date)]
    public DateTime ChampionshipEdition { get; set; }

    [Required]
    public string HostCountry { get; set; }

    [EnumDataType(typeof(ChampionshipType))]
    public ChampionshipType Type { get; set; }
}
