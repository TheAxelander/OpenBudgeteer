using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenBudgeteer.Data;

public class SqliteDatabaseContextFactory : IDesignTimeDbContextFactory<SqliteDatabaseContext>
{
    public SqliteDatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseSqlite("Data Source=database/openbudgeteer.db");

        return new SqliteDatabaseContext(optionsBuilder.Options);
    }

    public SqliteDatabaseContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(
                connectionString,
                b => b.MigrationsAssembly("OpenBudgeteer.Data.Sqlite.Migrations"));
        return new SqliteDatabaseContext(optionsBuilder.Options);
    }
}
