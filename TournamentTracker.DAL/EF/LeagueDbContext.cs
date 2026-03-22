using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TournamentTracker.Domain;

namespace TournamentTracker.DAL.EF;

public class LeagueDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Championship> Champions { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupTeam> GroupTeams { get; set; }

    public LeagueDbContext(DbContextOptions<LeagueDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(LeagueDatabasePath.GetConnectionString());
        }

        optionsBuilder.LogTo(msg => Debug.WriteLine(msg));
    }

    public bool CreateDataBase(bool shouldDelete)
    {
        if (shouldDelete)
        {
            Database.EnsureDeleted();
        }

        return Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityUser>(entity =>
        {
            entity.ToTable("AspNetUsers");
        });

        modelBuilder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("AspNetRoles");
        });

        modelBuilder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("AspNetUserRoles");
        });

        modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("AspNetUserClaims");
        });

        modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("AspNetUserLogins");
        });

        modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("AspNetRoleClaims");
        });

        modelBuilder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("AspNetUserTokens");
        });

        modelBuilder.Entity<Championship>(entity =>
        {
            entity.HasKey(championship => championship.Id);
            entity.Property(championship => championship.HostCountry).IsRequired();
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.Property(team => team.MaintainerId).IsRequired();

            entity.HasOne(team => team.Maintainer)
                .WithMany()
                .HasForeignKey(team => team.MaintainerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasOne(group => group.Championship)
                .WithMany(championship => championship.Groups)
                .HasForeignKey(group => group.ChampionshipId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GroupTeam>(entity =>
        {
            entity.HasKey(groupTeam => new { groupTeam.GroupId, groupTeam.TeamId });

            entity.HasOne(groupTeam => groupTeam.Group)
                .WithMany(group => group.Teams)
                .HasForeignKey(groupTeam => groupTeam.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(groupTeam => groupTeam.Team)
                .WithMany(team => team.Groups)
                .HasForeignKey(groupTeam => groupTeam.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
