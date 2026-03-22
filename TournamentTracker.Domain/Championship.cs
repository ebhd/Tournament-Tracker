using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentTracker.Domain;

public class Championship
{
    public int Id { get; init; }
    public DateTime ChampionshipEdition { get; set; }
    public string HostCountry { get; set; }


    public ICollection<Group> Groups { get; set; } = new List<Group>();


    [Required]
    [DataType(DataType.Date)]
    public ChampionshipType Type { get; set; }

    public override string ToString()
    {
        return "Championship Id: " + Id + ", Edition: " + ChampionshipEdition + ", Host Country: " + HostCountry + ", Number of Groups: " + Groups.Count;
    }
    
}