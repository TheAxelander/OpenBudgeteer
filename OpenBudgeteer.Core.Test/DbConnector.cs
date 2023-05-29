using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Core.Test;

public class DbConnector
{
    public static DbContextOptions<DatabaseContext> GetDbContextOptions(string dbName)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [ConfigurationKeyConstants.CONNECTION_PROVIDER] = ConfigurationKeyConstants.PROVIDER_SQLITE,
                [ConfigurationKeyConstants.CONNECTION_DATABASE] = $"{dbName}.db"
            })
            .Build();

        var contextOptions = DbContextOptionsFactory.GetContextOptions(configuration);
        var dbContext = new DatabaseContext(contextOptions);
        dbContext.Database.Migrate();

        return contextOptions;
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
