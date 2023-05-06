using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenBudgeteer.Data;

public class MySqlDatabaseContextFactory : IDesignTimeDbContextFactory<MySqlDatabaseContext>
{
    private const string MysqlConnectionString = "Server=192.168.178.201;" +
                                                 "Port=3306;" +
                                                 "Database=openbudgeteer-dev;" +
                                                 "User=openbudgeteer-dev;" +
                                                 "Password=openbudgeteer-dev";
    /*
     * Pass arguments to args by appending them to the tool call.
     * i.e.: dotnet ef migrations add Test -c OpenBudgeteer.Core.Common.Database.MySqlDatabaseContext -- "Server=kitana.vdaa;Port=3307;Database=openbudgeteer-dev;User=openbudgeteer-dev;Password=openbudgeteer-dev"
     */
    public MySqlDatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

        if ((args?.Length ?? 0) == 0)
        {
                optionsBuilder.UseMySql(MysqlConnectionString, ServerVersion.AutoDetect(MysqlConnectionString));
        }
        else
        {
            var connectionString = string.Join(";", args);
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        return new MySqlDatabaseContext(optionsBuilder.Options);
    }

    public MySqlDatabaseContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
            .UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                b => b.MigrationsAssembly("OpenBudgeteer.Core"));
        
        return new MySqlDatabaseContext(optionsBuilder.Options);
    }
}

