using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentTracker.Domain;

public class GroupTeam
{
    public int GroupId { get; set; }
    public Group Group { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; }

    public int Points { get; set; }
}
