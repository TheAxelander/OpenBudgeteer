using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Core.Data.Initialization;

// Used for database systems where no initialization is necessary
public class NoOpDatabaseInitializer : IDatabaseInitializer
{
    public void InitializeDatabase(IConfiguration configuration)
    {
        // Do nothing
    }
}