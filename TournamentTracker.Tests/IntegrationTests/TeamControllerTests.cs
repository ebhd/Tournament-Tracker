using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TournamentTracker.DAL.EF;
using TournamentTracker.Tests.IntegrationTests.Config;
using Xunit;

namespace TournamentTracker.Tests.IntegrationTests;

public class TeamControllerTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly ExtendedWebApplicationFactory<Program> _factory;

    public TeamControllerTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Add_Should_Require_Authentication_Without_Redirect()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.PostAsync("/Team/Add", null);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Identity/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Add_Should_Accept_And_Create_Valid_Team_And_Should_Redirect()
    {
        var client = _factory.CreateAuthenticatedClient("lars@kdg.be", "Password1!");
        var csrfToken = client.GetCsrfToken("/Team/Add");
        using var scope = _factory.CreateDbContextScope<LeagueDbContext>();
        var recordCount = scope.DbContext.Teams.Count();

        var response = await client.PostAsync("/Team/Add", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Name", "My team" },
            { "Country", "Belgium" },
            { "Coach", "My coach" },
            { "LogoImage", "https://example.com/my-team.png" },
            { "TeamValue", "123.45" },
            { "__RequestVerificationToken", csrfToken }
        }));

        Assert.Equal(recordCount + 1, scope.DbContext.Teams.Count());

        var createdTeam = scope.DbContext.Teams
            .Include(team => team.Maintainer)
            .Single(team => team.Name == "My team");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"/Team/Details/{createdTeam.Id}", response.RequestMessage?.RequestUri?.AbsolutePath);
        Assert.Equal("Belgium", createdTeam.Country);
        Assert.Equal("My coach", createdTeam.Coach);
        Assert.Equal(123.45m, createdTeam.TeamValue);
        Assert.Equal("lars@kdg.be", createdTeam.Maintainer.UserName);
    }

    [Fact]
    public async Task Add_Should_Not_Accept_Invalid_Team()
    {
        var client = _factory.CreateAuthenticatedClient("lars@kdg.be", "Password1!");
        var csrfToken = client.GetCsrfToken("/Team/Add");
        using var scope = _factory.CreateDbContextScope<LeagueDbContext>();
        var recordCount = scope.DbContext.Teams.Count();

        var response = await client.PostAsync("/Team/Add", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Name", "" },
            { "Country", "Belgium" },
            { "Coach", "My coach" },
            { "LogoImage", "https://example.com/my-team.png" },
            { "TeamValue", "123.45" },
            { "__RequestVerificationToken", csrfToken }
        }));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(recordCount, scope.DbContext.Teams.Count());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("/Team/Add", response.RequestMessage?.RequestUri?.AbsolutePath);
        Assert.Contains(">The Name field is required.<", body);
    }
}
