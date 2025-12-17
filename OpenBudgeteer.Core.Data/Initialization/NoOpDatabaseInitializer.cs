using System.Data.Common;
using OpenBudgeteer.Core.Data.Connection;

namespace OpenBudgeteer.Core.Data.Initialization;

// Used for database systems where no initialization is necessary
public class NoOpDatabaseInitializer : IDatabaseInitializer
{
    public void InitializeDatabase(IDatabaseConnector<DbConnectionStringBuilder> databaseConnector)
    {
        // Do nothing
    }
}