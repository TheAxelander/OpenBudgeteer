using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Data;

public class NoOpDatabaseInitializer : IDatabaseInitializer
{
    public void InitializeDatabase(IConfiguration configuration)
    {
        // Do nothing. - SQLite automatically creates the database if not exists
    }
}