using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentTracker.Domain;
public class Group
{
    public int Id { get; set; }

    [Required]
    [Range(2, 10)]
    public int MaxTeams { get; set; }
    [Required]
    public char GroupCode { get; set; }

    public ICollection<GroupTeam> Teams { get; set; } = new List<GroupTeam>();


    public int ChampionshipId { get; set; }
    public Championship Championship { get; set; }

    public override string ToString()
    {
        return "Group Id: " + Id + " Group Code: "+ GroupCode +", Max Teams: " + MaxTeams + ", Total Teams: " + Teams.Count;
    }

}