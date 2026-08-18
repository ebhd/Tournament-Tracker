using Microsoft.EntityFrameworkCore;
using TournamentTracker.BL;
using TournamentTracker.DAL;
using TournamentTracker.DAL.EF;
using TournamentTracker.UI.CA;

var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
optionsBuilder.UseSqlite("Data Source=../../../../league.db");

var dbContext = new LeagueDbContext(optionsBuilder.Options);
bool isCreated = dbContext.CreateDatabase(dropDatabase: true);

if (isCreated)
{
    Seeder.Seed(dbContext);
}

MemoryRepository.Seed();

IRepository repository = new Repository(dbContext);
IManager manager = new Manager(repository);
ConsoleUi ui = new ConsoleUi(manager);

ui.Run();
