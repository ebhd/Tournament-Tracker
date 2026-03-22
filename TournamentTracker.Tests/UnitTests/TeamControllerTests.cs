using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TournamentTracker.BL;
using TournamentTracker.Domain;
using TournamentTracker.UI_MVC.Controllers;
using Xunit;

namespace TournamentTracker.Tests.UnitTests;

public class TeamControllerTests
{
    private readonly TeamController _controller;
    private readonly Mock<IManager> _managerMock;

    public TeamControllerTests()
    {
        _managerMock = new Mock<IManager>();
        _controller = new TeamController(_managerMock.Object);
    }

    [Fact]
    public void Add_Should_Redirect_To_Details_If_ModelState_Valid()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "lars@kdg.be")
        ], "mockAuthType"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        _managerMock.Setup(manager => manager.AddTeam(
                "My team",
                "Belgium",
                "My coach",
                "https://example.com/my-team.png",
                123.45m,
                "lars@kdg.be"))
            .Returns(new Team { Id = 1234 });

        var result = _controller.Add(new Team
        {
            Name = "My team",
            Country = "Belgium",
            Coach = "My coach",
            LogoImage = "https://example.com/my-team.png",
            TeamValue = 123.45m
        });

        _managerMock.Verify(manager => manager.AddTeam(
            "My team",
            "Belgium",
            "My coach",
            "https://example.com/my-team.png",
            123.45m,
            "lars@kdg.be"));
        Assert.IsType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues?.Count);
        Assert.Equal(1234, redirectResult.RouteValues!["id"]);
    }

    [Fact]
    public void Add_Should_Stay_On_Add_Form_If_ModelState_Invalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");

        var result = _controller.Add(new Team());

        _managerMock.Verify(manager => manager.AddTeam(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<string>()), Times.Never);
        Assert.IsType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsType<Team>(viewResult.Model);
    }
}
