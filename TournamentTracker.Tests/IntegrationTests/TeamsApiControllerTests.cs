using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TournamentTracker.DAL.EF;
using TournamentTracker.Tests.IntegrationTests.Config;
using Xunit;

namespace TournamentTracker.Tests.IntegrationTests;

public class TeamsApiControllerTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly ExtendedWebApplicationFactory<Program> _factory;

    public TeamsApiControllerTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateTeamValue_Should_Return_Unauthorized_When_Not_Signed_In()
    {
        var client = _factory.CreateClient();
        var team = ReadTeamMaintainedBy("lars@kdg.be");

        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}/teamvalue", new { teamValue = 100m });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(team.TeamValue, ReadTeamValue(team.Id));
    }

    [Fact]
    public async Task UpdateTeamValue_Should_Return_NoContent_When_Signed_In_As_Maintainer()
    {
        var client = _factory.CreateAuthenticatedClient("lars@kdg.be", "Password1!");
        var team = ReadTeamMaintainedBy("lars@kdg.be");

        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}/teamvalue", new { teamValue = 222.75m });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(222.75m, ReadTeamValue(team.Id));
    }

    [Fact]
    public async Task UpdateTeamValue_Should_Return_NoContent_When_Signed_In_As_Admin()
    {
        var client = _factory.CreateAuthenticatedClient("ali@kdg.be", "Password1!");
        var team = ReadTeamMaintainedBy("lars@kdg.be");

        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}/teamvalue", new { teamValue = 444.50m });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(444.50m, ReadTeamValue(team.Id));
    }

    [Fact]
    public async Task UpdateTeamValue_Should_Return_Forbidden_When_Signed_In_As_Not_Admin_And_Not_Maintainer()
    {
        var client = _factory.CreateAuthenticatedClient("ebrahim@kdg.be", "Password1!");
        var team = ReadTeamMaintainedBy("lars@kdg.be");

        var response = await client.PutAsJsonAsync($"/api/teams/{team.Id}/teamvalue", new { teamValue = 321.25m });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(team.TeamValue, ReadTeamValue(team.Id));
    }

    private (int Id, decimal? TeamValue) ReadTeamMaintainedBy(string maintainerEmail)
    {
        using var scope = _factory.CreateDbContextScope<LeagueDbContext>();
        return scope.DbContext.Teams
            .Join(scope.DbContext.Users, t => t.MaintainerId, u => u.Id, (t, u) => new { Team = t, u.Email })
            .Where(x => x.Email == maintainerEmail)
            .OrderBy(x => x.Team.Id)
            .Select(x => new ValueTuple<int, decimal?>(x.Team.Id, x.Team.TeamValue))
            .First();
    }

    private decimal? ReadTeamValue(int teamId)
    {
        using var scope = _factory.CreateDbContextScope<LeagueDbContext>();
        return scope.DbContext.Teams.Where(t => t.Id == teamId).Select(t => t.TeamValue).Single();
    }
}
