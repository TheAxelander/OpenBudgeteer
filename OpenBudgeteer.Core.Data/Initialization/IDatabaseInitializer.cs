using System.Data.Common;
using Microsoft.Extensions.Configuration;
using OpenBudgeteer.Core.Data.Connection;

namespace OpenBudgeteer.Core.Data.Initialization;

// Contract for database initializers
public interface IDatabaseInitializer
{
    public void InitializeDatabase(IDatabaseConnector<DbConnectionStringBuilder> databaseConnector);
}