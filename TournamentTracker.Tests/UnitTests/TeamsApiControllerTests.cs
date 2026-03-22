using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TournamentTracker.BL;
using TournamentTracker.Domain;
using Xunit;

namespace TournamentTracker.Tests.UnitTests;

public class TeamsApiControllerTests
{
    [Fact]
    public void UpdateTeamValue_Should_Return_ValidationProblem_When_ModelState_Invalid()
    {
        var managerMock = new Mock<IManager>();
        var controller = CreateController(managerMock);
        controller.ModelState.AddModelError(nameof(UpdateTeamValueDto.TeamValue), "The field TeamValue must be non-negative.");

        var result = controller.UpdateTeamValue(5, new UpdateTeamValueDto { TeamValue = -1m });

        Assert.IsType<BadRequestObjectResult>(result);
        managerMock.Verify(manager => manager.UpdateTeamValue(It.IsAny<int>(), It.IsAny<decimal?>(), It.IsAny<string?>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void UpdateTeamValue_Should_Return_Forbidden_When_User_Is_Not_Owner_Or_Admin()
    {
        var managerMock = new Mock<IManager>();
        managerMock.Setup(manager => manager.GetTeamWithMaintainer(7))
            .Returns(new Team
            {
                Id = 7,
                Name = "Belgium",
                Country = "Belgium",
                LogoImage = "https://example.com/logo.png",
                MaintainerId = "user-1",
                Maintainer = new IdentityUser { UserName = "owner@kdg.be" }
            });

        var controller = CreateController(managerMock, username: "other@kdg.be", isAdmin: false);

        var result = controller.UpdateTeamValue(7, new UpdateTeamValueDto { TeamValue = 222.25m });

        Assert.IsType<ForbidResult>(result);
        managerMock.Verify(manager => manager.UpdateTeamValue(It.IsAny<int>(), It.IsAny<decimal?>(), It.IsAny<string?>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void UpdateTeamValue_Should_Return_NoContent_When_Owner_Updates_Team_Value()
    {
        var managerMock = new Mock<IManager>();
        managerMock.Setup(manager => manager.GetTeamWithMaintainer(7))
            .Returns(new Team
            {
                Id = 7,
                Name = "Belgium",
                Country = "Belgium",
                LogoImage = "https://example.com/logo.png",
                MaintainerId = "user-1",
                Maintainer = new IdentityUser { UserName = "owner@kdg.be" }
            });
        managerMock.Setup(manager => manager.UpdateTeamValue(7, 222.25m, "owner@kdg.be", false))
            .Returns(true);

        var controller = CreateController(managerMock, username: "owner@kdg.be", isAdmin: false);

        var result = controller.UpdateTeamValue(7, new UpdateTeamValueDto { TeamValue = 222.25m });

        Assert.IsType<NoContentResult>(result);
        managerMock.Verify(manager => manager.UpdateTeamValue(7, 222.25m, "owner@kdg.be", false), Times.Once);
    }

    private static TeamsApiController CreateController(Mock<IManager> managerMock, string? username = null, bool isAdmin = false)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
        var serviceProvider = services.BuildServiceProvider();

        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(username))
        {
            claims.Add(new Claim(ClaimTypes.Name, username));
        }

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "admin"));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth", nameType: ClaimTypes.Name, roleType: ClaimTypes.Role);

        return new TeamsApiController(managerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = serviceProvider,
                    User = new ClaimsPrincipal(identity)
                },
                RouteData = new RouteData()
            }
        };
    }
}
