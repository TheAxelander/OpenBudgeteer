using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Core.Data.Initialization;

// Contract for database initializers
public interface IDatabaseInitializer
{
    public void InitializeDatabase(IConfiguration configuration);
}