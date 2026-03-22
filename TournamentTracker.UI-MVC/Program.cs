using AspNetCoreLiveMonitoring.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TournamentTracker.BL;
using TournamentTracker.DAL;
using TournamentTracker.DAL.EF;

const string DefaultPassword = "Password1!";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<LeagueDbContext>(options =>
    options.UseSqlite(LeagueDatabasePath.GetConnectionString()));

builder.Services
    .AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<LeagueDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IManager, Manager>();

var liveMonitoringEnabled = !builder.Environment.IsEnvironment("Testing") &&
                            !builder.Configuration.GetValue<bool>("DisableLiveMonitoring");
builder.Configuration["LiveMonitoring:Enabled"] = liveMonitoringEnabled.ToString();
if (liveMonitoringEnabled)
{
    builder.Services.AddLiveMonitoring();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LeagueDbContext>();
    var created = context.CreateDataBase(shouldDelete: true);
    if (created)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedIdentity(userManager, roleManager);
        Seeder.Seed(context);
        await Seeder.EnsureSeededTeamMaintainersAsync(context);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRouting();

if (liveMonitoringEnabled)
{
    app.UseAndMapLiveMonitoring();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Team}/{action=Index}/{id?}");

app.Run();

static async Task SeedIdentity(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    var lars = new IdentityUser
    {
        Email = "lars@kdg.be",
        UserName = "lars@kdg.be",
        EmailConfirmed = true
    };
    await userManager.CreateAsync(lars, DefaultPassword);

    var jack = new IdentityUser
    {
        Email = "ebrahim@kdg.be",
        UserName = "ebrahim@kdg.be",
        EmailConfirmed = true
    };
    await userManager.CreateAsync(jack, DefaultPassword);

    var ines = new IdentityUser
    {
        Email = "ayoub@kdg.be",
        UserName = "ayoub@kdg.be",
        EmailConfirmed = true
    };
    await userManager.CreateAsync(ines, DefaultPassword);

    var admin = new IdentityUser
    {
        Email = "ali@kdg.be",
        UserName = "ali@kdg.be",
        EmailConfirmed = true
    };
    await userManager.CreateAsync(admin, DefaultPassword);

    var sarah = new IdentityUser
    {
        Email = "chris@kdg.be",
        UserName = "chris@kdg.be",
        EmailConfirmed = true
    };
    await userManager.CreateAsync(sarah, DefaultPassword);

    var userRole = new IdentityRole { Name = "user" };
    var adminRole = new IdentityRole { Name = "admin" };

    await roleManager.CreateAsync(userRole);
    await roleManager.CreateAsync(adminRole);

    await userManager.AddToRoleAsync(lars, "user");
    await userManager.AddToRoleAsync(jack, "user");
    await userManager.AddToRoleAsync(ines, "user");
    await userManager.AddToRoleAsync(admin, "admin");
    await userManager.AddToRoleAsync(sarah, "admin");
}

public partial class Program { }
