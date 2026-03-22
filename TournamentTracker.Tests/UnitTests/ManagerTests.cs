using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Moq;
using TournamentTracker.BL;
using TournamentTracker.DAL;
using TournamentTracker.Domain;
using Xunit;

namespace TournamentTracker.Tests.UnitTests;

public class ManagerTests
{
    private readonly IManager _manager;
    private readonly Mock<IRepository> _repositoryMock;

    public ManagerTests()
    {
        _repositoryMock = new Mock<IRepository>();
        _manager = new Manager(_repositoryMock.Object);
    }

    [Fact]
    public void AddTeam_Should_Accept_Valid_Team()
    {
        _repositoryMock.Setup(repo => repo.ReadUserByUsername("lars@kdg.be"))
            .Returns(new IdentityUser
            {
                Id = "user-1",
                UserName = "lars@kdg.be"
            });

        var createdTeam = _manager.AddTeam(
            "My team",
            "Belgium",
            "My coach",
            "https://example.com/my-team.png",
            123.45m,
            "lars@kdg.be");

        Assert.Equal("My team", createdTeam.Name);
        Assert.Equal("Belgium", createdTeam.Country);
        Assert.Equal("My coach", createdTeam.Coach);
        Assert.Equal(123.45m, createdTeam.TeamValue);
        Assert.Equal("user-1", createdTeam.MaintainerId);
        Assert.Equal("lars@kdg.be", createdTeam.Maintainer.UserName);

        _repositoryMock.Verify(repo => repo.CreateTeam(
                It.Is<Team>(team =>
                    team.Name == createdTeam.Name &&
                    team.Country == createdTeam.Country &&
                    team.Coach == createdTeam.Coach &&
                    team.TeamValue == createdTeam.TeamValue &&
                    team.MaintainerId == createdTeam.MaintainerId &&
                    team.Maintainer.UserName == createdTeam.Maintainer.UserName)),
            Times.Once());
    }

    [Fact]
    public void AddTeam_Should_Reject_Invalid_Maintainer()
    {
        _repositoryMock.Setup(repo => repo.ReadUserByUsername("missing@kdg.be"))
            .Returns((IdentityUser?)null);

        var action = () => _manager.AddTeam(
            "My team",
            "Belgium",
            "My coach",
            "https://example.com/my-team.png",
            123.45m,
            "missing@kdg.be");

        Assert.Throws<ValidationException>(action);
        _repositoryMock.Verify(repo => repo.CreateTeam(It.IsAny<Team>()), Times.Never());
    }

    [Fact]
    public void UpdateTeamValue_Should_Return_True_For_Maintainer()
    {
        _repositoryMock.Setup(repo => repo.ReadTeamWithMaintainer(5))
            .Returns(new Team
            {
                Id = 5,
                Name = "Belgium",
                Country = "Belgium",
                LogoImage = "https://example.com/logo.png",
                MaintainerId = "user-1",
                Maintainer = new IdentityUser { UserName = "lars@kdg.be" }
            });
        _repositoryMock.Setup(repo => repo.UpdateTeamValue(5, 250.50m));

        var updated = _manager.UpdateTeamValue(5, 250.50m, "lars@kdg.be", isAdmin: false);

        Assert.True(updated);
        _repositoryMock.Verify(repo => repo.UpdateTeamValue(5, 250.50m), Times.Once());
    }

    [Fact]
    public void UpdateTeamValue_Should_Return_False_For_Not_Admin_And_Not_Maintainer()
    {
        _repositoryMock.Setup(repo => repo.ReadTeamWithMaintainer(5))
            .Returns(new Team
            {
                Id = 5,
                Name = "Belgium",
                Country = "Belgium",
                LogoImage = "https://example.com/logo.png",
                MaintainerId = "user-1",
                Maintainer = new IdentityUser { UserName = "lars@kdg.be" }
            });

        var updated = _manager.UpdateTeamValue(5, 250.50m, "ebrahim@kdg.be", isAdmin: false);

        Assert.False(updated);
        _repositoryMock.Verify(repo => repo.UpdateTeamValue(It.IsAny<int>(), It.IsAny<decimal?>()), Times.Never());
    }
}
