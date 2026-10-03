using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TournamentTracker.DAL.EF;

namespace TournamentTracker.UI_MVC.Security;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, LeagueDbContext context)
    {
        var users = services.GetRequiredService<UserManager<IdentityUser>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "user", "admin" })
        {
            EnsureSucceeded(await roles.CreateAsync(new IdentityRole(role)));
        }

        var emails = new[] { "lars@kdg.be", "ebrahim@kdg.be", "ayoub@kdg.be", "ali@kdg.be", "chris@kdg.be" };
        var maintainers = new List<IdentityUser>();
        foreach (var email in emails)
        {
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            EnsureSucceeded(await users.CreateAsync(user, "Password1!"));
            EnsureSucceeded(await users.AddToRoleAsync(user, maintainers.Count >= 3 ? "admin" : "user"));
            maintainers.Add(user);
        }

        // Assign owners without changing any of the P5 seed data or relationships.
        var teams = await context.Teams.OrderBy(team => team.Id).ToListAsync();
        for (var index = 0; index < teams.Count; index++)
        {
            teams[index].MaintainerId = maintainers[index % maintainers.Count].Id;
        }
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
    }
}
