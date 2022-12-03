using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;

namespace OpenBudgeteer.Core.Test;

public class DbConnector
{
    public static DbContextOptions<DatabaseContext> GetDbContextOptions(string dbName)
    {
        var connectionString = $"Data Source={dbName}.db";

        //Check on Pending Db Migrations
       var sqliteDbContext = new SqliteDatabaseContextFactory().CreateDbContext(connectionString);
        if (sqliteDbContext.Database.GetPendingMigrations().Any())
            sqliteDbContext.Database.Migrate();

        return new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    public static void CleanupDatabase(string dbName)
    {
        using (var dbContext = new DatabaseContext(GetDbContextOptions(dbName)))
        {
            dbContext.DeleteAccounts(dbContext.Account);
            dbContext.DeleteBankTransactions(dbContext.BankTransaction);
            dbContext.DeleteBuckets(dbContext.Bucket);
            dbContext.DeleteBucketGroups(dbContext.BucketGroup);
            dbContext.DeleteBucketMovements(dbContext.BucketMovement);
            dbContext.DeleteBucketRuleSets(dbContext.BucketRuleSet);
            dbContext.DeleteBucketVersions(dbContext.BucketVersion);
            dbContext.DeleteBudgetedTransactions(dbContext.BudgetedTransaction);
            dbContext.DeleteImportProfiles(dbContext.ImportProfile);
            dbContext.DeleteMappingRules(dbContext.MappingRule);
            dbContext.DeleteRecurringBankTransactions(dbContext.RecurringBankTransaction);
        }
    }
}
