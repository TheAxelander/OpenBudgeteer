using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Core.Common.Database
{
    public class MySqlDatabaseContextFactory : IDesignTimeDbContextFactory<MySqlDatabaseContext>
    {
        public MySqlDatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
                .UseMySql("Server=192.168.178.30;" +
                           "Port=3306;" +
                           "Database=openbudgeteer-dev" +
                           "User=openbudgeteer-dev" +
                           "Password=openbudgeteer-dev");

            return new MySqlDatabaseContext(optionsBuilder.Options);
        }

        public MySqlDatabaseContext CreateDbContext(IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection("Connection");
            var connectionString = $"Server={configurationSection?["Server"]};" +
                                   $"Port={configurationSection?["Port"]};" +
                                   $"Database={configurationSection?["Database"]};" +
                                   $"User={configurationSection?["User"]};" +
                                   $"Password={configurationSection?["Password"]}";
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
                .UseMySql(
                    connectionString, 
                    b => b.MigrationsAssembly("OpenBudgeteer.Core"));
            return new MySqlDatabaseContext(optionsBuilder.Options);
        }
    }
}
