namespace TournamentTracker.UI_MVC.Models.Dtos;

public class ChampionshipDto
{
    public int Id { get; set; }
    public DateTime ChampionshipEdition { get; set; }
    public string HostCountry { get; set; }
    public string Type { get; set; }
    public int GroupCount { get; set; }
}
