using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Data.Initialization;

// Contract for database initializers
public interface IDatabaseInitializer
{
    public void InitializeDatabase(IConfiguration configuration);
}