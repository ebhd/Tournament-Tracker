using AspNetCoreLiveMonitoring.Extensions;
using TournamentTracker.DAL;
using TournamentTracker.DAL.EF;
using TournamentTracker.BL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TournamentTracker.UI_MVC.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddXmlSerializerFormatters();

builder.Services.AddDbContext<LeagueDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LeagueDatabase")
        ?? throw new InvalidOperationException("Connection string 'LeagueDatabase' is missing.")));
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<LeagueDbContext>();
builder.Services.AddRazorPages();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else
            context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        else
            context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});
builder.Services.AddScoped<IManager, Manager>();
builder.Services.AddScoped<IRepository, Repository>();

builder.Services.AddLiveMonitoring();

var app = builder.Build();

app.UseAndMapLiveMonitoring();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<LeagueDbContext>();
    if (context.CreateDatabase(dropDatabase: true))
    {
        Seeder.Seed(context);
        await IdentitySeeder.SeedAsync(scope.ServiceProvider, context);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program { }
