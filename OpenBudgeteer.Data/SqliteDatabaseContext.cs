using Microsoft.EntityFrameworkCore;

namespace OpenBudgeteer.Data;

public class SqliteDatabaseContext : DatabaseContext
{
    public SqliteDatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        
    }
}

