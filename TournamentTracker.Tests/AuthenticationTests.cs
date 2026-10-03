using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TournamentTracker.BL;
using TournamentTracker.DAL.EF;
using Xunit;

namespace TournamentTracker.Tests;

public sealed class AuthenticationTests : IClassFixture<AuthApplicationFactory>
{
    private readonly AuthApplicationFactory _factory;
    public AuthenticationTests(AuthApplicationFactory factory) => _factory = factory;

    private static async Task<string> Token(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, $"No antiforgery token at {path}");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static async Task Login(HttpClient client, string email)
    {
        var token = await Token(client, "/Identity/Account/Login");
        var response = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email, ["Input.Password"] = "Password1!", ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> Write(HttpClient client, HttpMethod method, string path, object body, bool token = true)
    {
        var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
        if (token) request.Headers.Add("RequestVerificationToken", await Token(client, "/Group/Details/1"));
        return await client.SendAsync(request);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Team")]
    [InlineData("/Team/Details/1")]
    [InlineData("/Group/Details/1")]
    [InlineData("/api/teams")]
    [InlineData("/api/championships/1")]
    [InlineData("/api/groups/1/teams/1")]
    public async Task P5ReadRoutesRemainPublic(string path)
    {
        using var client = _factory.NewClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path)).StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/championships")]
    [InlineData("POST", "/api/groups/1/teams")]
    [InlineData("PUT", "/api/teams/1/teamvalue")]
    public async Task AnonymousApiWritesReturn401WithoutRedirect(string method, string path)
    {
        using var client = _factory.NewClient();
        var response = await Write(client, new HttpMethod(method), path, new { }, false);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task AnonymousTeamCreationRedirectsToLogin()
    {
        using var client = _factory.NewClient();
        var response = await client.GetAsync("/Team/Add");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location!.ToString());
        response = await client.PostAsync("/Team/Add", new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task P5XmlFormatterAndAnonymousReadOnlyViewArePreserved()
    {
        using var client = _factory.NewClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/championships/1");
        request.Headers.Accept.ParseAdd("application/xml");
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType!.MediaType);
        Assert.DoesNotContain("maintainerId", await client.GetStringAsync("/api/teams"));
        var html = await client.GetStringAsync("/Team/Details/1");
        Assert.DoesNotContain("id=\"teamValueForm\"", html);
        Assert.Contains("lars@kdg.be", html);
    }

    [Theory]
    [InlineData("/api/championships")]
    [InlineData("/api/groups/1/teams")]
    public async Task AuthenticatedPostRequiresAntiforgeryToken(string path)
    {
        using var client = _factory.NewClient();
        await Login(client, "lars@kdg.be");
        Assert.Equal(HttpStatusCode.BadRequest, (await Write(client, HttpMethod.Post, path, new { }, false)).StatusCode);
    }

    [Theory]
    [InlineData("lars@kdg.be", HttpStatusCode.NoContent, true)]
    [InlineData("ebrahim@kdg.be", HttpStatusCode.Forbidden, false)]
    [InlineData("ali@kdg.be", HttpStatusCode.NoContent, true)]
    public async Task OnlyOwnerOrAdminCanEditTeamValue(string email, HttpStatusCode expected, bool editorVisible)
    {
        using var client = _factory.NewClient();
        await Login(client, email);
        var html = await client.GetStringAsync("/Team/Details/1");
        Assert.Equal(editorVisible, html.Contains("id=\"teamValueForm\""));
        decimal? original;
        using (var scope = _factory.Services.CreateScope())
            original = scope.ServiceProvider.GetRequiredService<LeagueDbContext>().Teams.Find(1)!.TeamValue;
        var response = await Write(client, HttpMethod.Put, "/api/teams/1/teamvalue", new { teamValue = 456.78m });
        Assert.Equal(expected, response.StatusCode);
        using var verify = _factory.Services.CreateScope();
        Assert.Equal(editorVisible ? 456.78m : original,
            verify.ServiceProvider.GetRequiredService<LeagueDbContext>().Teams.Find(1)!.TeamValue);
    }

    [Fact]
    public async Task TeamValueRejectsInvalidInputAndMissingTokenAndSupportsClearing()
    {
        using var client = _factory.NewClient();
        await Login(client, "lars@kdg.be");
        Assert.Equal(HttpStatusCode.BadRequest, (await Write(client, HttpMethod.Put, "/api/teams/1/teamvalue", new { teamValue = -1 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Write(client, HttpMethod.Put, "/api/teams/1/teamvalue", new { teamValue = 10 }, false)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Write(client, HttpMethod.Put, "/api/teams/99999/teamvalue", new { teamValue = 10 })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Write(client, HttpMethod.Put, "/api/teams/1/teamvalue", new { teamValue = (decimal?)null })).StatusCode);
        using var scope = _factory.Services.CreateScope();
        Assert.Null(scope.ServiceProvider.GetRequiredService<LeagueDbContext>().Teams.Find(1)!.TeamValue);
    }

    [Fact]
    public async Task RegistrationCreatesUserRoleAndTeamOwnerCannotBeSpoofed()
    {
        using var client = _factory.NewClient();
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsync("/Identity/Account/Register?returnUrl=https://example.com", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email, ["Input.Password"] = "Password1!", ["Input.ConfirmPassword"] = "Password1!",
            ["__RequestVerificationToken"] = await Token(client, "/Identity/Account/Register")
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location!.ToString());

        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal(new[] { "user" }, await users.GetRolesAsync(user));
        response = await client.PostAsync("/Team/Add", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Registered team", ["Country"] = "Belgium", ["Coach"] = "Coach", ["LogoImage"] = "https://example.com/logo.png",
            ["MaintainerId"] = "forged-owner", ["__RequestVerificationToken"] = await Token(client, "/Team/Add")
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var id = int.Parse(response.Headers.Location!.ToString().Split('/').Last());
        var team = scope.ServiceProvider.GetRequiredService<LeagueDbContext>().Teams.Find(id)!;
        Assert.Equal(user.Id, team.MaintainerId);
        Assert.Equal(HttpStatusCode.NoContent, (await Write(client, HttpMethod.Put, $"/api/teams/{id}/teamvalue", new { teamValue = 12 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Write(client, HttpMethod.Put, "/api/teams/1/teamvalue", new { teamValue = 12 })).StatusCode);
    }

    [Fact]
    public async Task LoginRejectsWrongPasswordAndLogoutRequiresPostAndToken()
    {
        using var client = _factory.NewClient();
        var invalid = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = "lars@kdg.be", ["Input.Password"] = "wrong", ["__RequestVerificationToken"] = await Token(client, "/Identity/Account/Login")
        }));
        Assert.Contains("Invalid login attempt", await invalid.Content.ReadAsStringAsync());
        await Login(client, "lars@kdg.be");
        await client.GetAsync("/Identity/Account/Logout");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Team/Add")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Identity/Account/Logout", new FormUrlEncodedContent(new Dictionary<string, string>()))).StatusCode);
        var response = await client.PostAsync("/Identity/Account/Logout?returnUrl=https://example.com", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = await Token(client, "/Identity/Account/Logout")
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain("example.com", response.Headers.Location!.ToString());
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Team/Add")).StatusCode);
    }

    [Fact]
    public async Task P5ApiCreationContractsAndGroupValidationArePreserved()
    {
        using var client = _factory.NewClient();
        await Login(client, "lars@kdg.be");
        var response = await Write(client, HttpMethod.Post, "/api/championships", new { championshipEdition = "2026-01-01", hostCountry = "Test", type = 0 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(response.Headers.Location)).StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, body.GetProperty("groupCount").GetInt32());
        Assert.Equal(HttpStatusCode.BadRequest, (await Write(client, HttpMethod.Post, "/api/championships", new { hostCountry = "Test", type = 999 })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Write(client, HttpMethod.Post, "/api/groups/1/teams", new { teamId = 1, points = 0 })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Write(client, HttpMethod.Post, "/api/groups/99999/teams", new { teamId = 1, points = 0 })).StatusCode);
        response = await Write(client, HttpMethod.Post, "/api/groups/11/teams", new { teamId = 5, points = 7 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        body = await client.GetFromJsonAsync<JsonElement>(response.Headers.Location);
        Assert.Equal(7, body.GetProperty("points").GetInt32());
    }

    [Fact]
    public void P5SeedRelationshipsValidationAndConsoleCreationRemainIntact()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var context = new LeagueDbContext(new DbContextOptionsBuilder<LeagueDbContext>().UseSqlite(connection).Options);
        context.CreateDatabase(false);
        Seeder.Seed(context);
        Assert.Equal(5, context.Championships.Count());
        Assert.Equal(11, context.Groups.Count());
        Assert.Equal(6, context.Teams.Count());
        Assert.Equal(11, context.GroupTeams.Count());
        var manager = new Manager(new Repository(context));
        Assert.Throws<ValidationException>(() => manager.AddTeam("Same", "Belgium", "Same", "https://example.com/logo.png", 1));
        var team = manager.AddTeam("Console team", "Belgium", "Coach", "https://example.com/logo.png", 1);
        Assert.Null(team.MaintainerId);
        Assert.False(manager.UpdateTeamValue(team.Id, 5, "someone-else", false));
        Assert.False(manager.UpdateTeamValue(team.Id, 5, "", true));
        Assert.True(manager.UpdateTeamValue(team.Id, 5, "admin-id", true));
        Assert.Throws<ValidationException>(() => manager.UpdateTeamValue(team.Id, -1, "admin-id", true));
        Assert.Throws<ValidationException>(() => manager.AddTeamToGroup(1, 1));
        manager.AddTeamToGroup(2, 5);
        Assert.Throws<ValidationException>(() => manager.AddTeamToGroup(3, 5));
        var championshipRelationship = context.Model.FindEntityType(typeof(TournamentTracker.Domain.Group))!.GetForeignKeys().Single();
        Assert.Equal(DeleteBehavior.Restrict, championshipRelationship.DeleteBehavior);
    }
}
