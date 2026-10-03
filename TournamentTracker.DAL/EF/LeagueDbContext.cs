using TournamentTracker.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TournamentTracker.DAL.EF;

public class LeagueDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Championship> Championships { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Group> Groups { get; set; }

    public DbSet<GroupTeam> GroupTeams { get; set; }

    public LeagueDbContext(DbContextOptions options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Team>()
            .HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(team => team.MaintainerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Championship 1 --- * Group
        modelBuilder.Entity<Group>()
            .HasOne(g => g.Championship)
            .WithMany(c => c.Groups)
            .OnDelete(DeleteBehavior.Restrict);

        // GroupTeam (scenario 2 )
        modelBuilder.Entity<GroupTeam>()
            .HasKey(gt => new { gt.GroupId, gt.TeamId });

        // Group 1 --- * GroupTeam
        modelBuilder.Entity<GroupTeam>()
            .HasOne(gt => gt.Group)
            .WithMany(g => g.Teams)
            .HasForeignKey(gt => gt.GroupId);

        // Team 1 --- * GroupTeam
        modelBuilder.Entity<GroupTeam>()
            .HasOne(gt => gt.Team)
            .WithMany(t => t.Groups)
            .HasForeignKey(gt => gt.TeamId);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(message => Debug.WriteLine(message),
            LogLevel.Information);

    }

    public bool CreateDatabase(bool dropDatabase)
    {
        if (dropDatabase)
        {
            Database.EnsureDeleted();
        }

        return Database.EnsureCreated();
    }


}
