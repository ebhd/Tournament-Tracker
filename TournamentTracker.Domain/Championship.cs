using System.ComponentModel.DataAnnotations;

namespace TournamentTracker.Domain;

public class Championship
{
    public int Id { get; init; }
    [DataType(DataType.Date)]
    public DateTime ChampionshipEdition { get; set; }
    public string HostCountry { get; set; }


    public ICollection<Group> Groups { get; set; } = new List<Group>();


    [EnumDataType(typeof(ChampionshipType))]
    public ChampionshipType Type { get; set; }

}
