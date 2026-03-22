namespace TournamentTracker.DAL.EF;

public static class LeagueDatabasePath
{
    public static string GetDatabasePath()
    {
        return Path.GetFullPath("league.db");
    }

    public static string GetConnectionString()
    {
        return $"Data Source={GetDatabasePath()}";
    }
}
