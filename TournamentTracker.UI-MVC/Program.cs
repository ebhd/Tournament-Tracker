using AspNetCoreLiveMonitoring.Extensions;
using TournamentTracker.DAL;
using TournamentTracker.DAL.EF;
using TournamentTracker.BL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddXmlSerializerFormatters();

builder.Services.AddDbContext<LeagueDbContext>();
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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
