using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TournamentTracker.BL;
using TournamentTracker.DAL.EF;
using TournamentTracker.Tests.IntegrationTests.Config;
using Xunit;

namespace TournamentTracker.Tests.IntegrationTests;

public class ManagerTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly ExtendedWebApplicationFactory<Program> _factory;

    public ManagerTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void AddTeam_Should_Validate_Invalid_Team()
    {
        using var scope = _factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LeagueDbContext>();
        var recordCount = dbContext.Teams.Count();

        Assert.Throws<ValidationException>(() => manager.AddTeam(
            "",
            "Belgium",
            "Coach",
            "https://example.com/logo.png",
            50m,
            "lars@kdg.be"));

        Assert.Equal(recordCount, dbContext.Teams.Count());
    }

    [Fact]
    public void AddTeam_Should_Return_Created_Team_With_Maintainer()
    {
        using var scope = _factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LeagueDbContext>();
        var recordCount = dbContext.Teams.Count();

        var createdTeam = manager.AddTeam(
            "My team",
            "Belgium",
            "My coach",
            "https://example.com/my-team.png",
            123.45m,
            "lars@kdg.be");

        var persistedTeam = dbContext.Teams
            .Include(team => team.Maintainer)
            .Single(team => team.Id == createdTeam.Id);

        Assert.Equal(recordCount + 1, dbContext.Teams.Count());
        Assert.InRange(createdTeam.Id, 1, int.MaxValue);
        Assert.Equal("My team", createdTeam.Name);
        Assert.Equal("Belgium", createdTeam.Country);
        Assert.Equal("My coach", createdTeam.Coach);
        Assert.Equal(123.45m, createdTeam.TeamValue);
        Assert.Equal("lars@kdg.be", createdTeam.Maintainer.UserName);
        Assert.Equal("My team", persistedTeam.Name);
        Assert.Equal("lars@kdg.be", persistedTeam.Maintainer.UserName);
    }

    [Fact]
    public void UpdateTeamValue_Should_Return_False_For_Not_Admin_And_Not_Maintainer()
    {
        using var scope = _factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LeagueDbContext>();

        var team = dbContext.Teams
            .Include(t => t.Maintainer)
            .OrderBy(t => t.Id)
            .First(t => t.Maintainer.UserName == "lars@kdg.be");
        var originalValue = team.TeamValue;

        var updated = manager.UpdateTeamValue(team.Id, 999.99m, "ebrahim@kdg.be", isAdmin: false);

        Assert.False(updated);
        Assert.Equal(originalValue, dbContext.Teams.Single(t => t.Id == team.Id).TeamValue);
    }

    [Fact]
    public void UpdateTeamValue_Should_Return_True_For_Admin_And_Persist_Value()
    {
        using var scope = _factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LeagueDbContext>();

        var team = dbContext.Teams
            .Include(t => t.Maintainer)
            .OrderBy(t => t.Id)
            .First(t => t.Maintainer.UserName == "lars@kdg.be");

        var updated = manager.UpdateTeamValue(team.Id, 432.10m, "ali@kdg.be", isAdmin: true);

        Assert.True(updated);
        Assert.Equal(432.10m, dbContext.Teams.Single(t => t.Id == team.Id).TeamValue);
    }
}
