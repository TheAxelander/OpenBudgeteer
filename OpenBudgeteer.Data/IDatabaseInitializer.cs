using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Data;

public interface IDatabaseInitializer
{
    public void InitializeDatabase(IConfiguration configuration);
}