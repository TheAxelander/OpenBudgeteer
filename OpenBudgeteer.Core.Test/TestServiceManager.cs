using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services;
using OpenBudgeteer.Core.Test.Extension;

namespace OpenBudgeteer.Core.Test;

public class TestServiceManager : IServiceManager
{
    public IAccountService AccountService => _lazyAccountService.Value;
    public IBankTransactionService BankTransactionService => _lazyBankTransactionService.Value;
    public IBucketGroupService BucketGroupService => _lazyBucketGroupService.Value;
    public IBucketMovementService BucketMovementService => _lazyBucketMovementService.Value;
    public IBucketService BucketService => _lazyBucketService.Value;
    public IBucketRuleSetService BucketRuleSetService => _lazyBucketRuleSetService.Value;
    public IBudgetedTransactionService BudgetedTransactionService => _lazyBudgetedTransactionService.Value;
    public IImportProfileService ImportProfileService => _lazyImportProfileService.Value;
    public IRecurringBankTransactionService RecurringBankTransactionService => _lazyRecurringBankTransactionService.Value;
    
    private readonly Lazy<IAccountService> _lazyAccountService;
    private readonly Lazy<IBankTransactionService> _lazyBankTransactionService;
    private readonly Lazy<IBucketGroupService> _lazyBucketGroupService;
    private readonly Lazy<IBucketMovementService> _lazyBucketMovementService;
    private readonly Lazy<IBucketService> _lazyBucketService;
    private readonly Lazy<IBucketRuleSetService> _lazyBucketRuleSetService;
    private readonly Lazy<IBudgetedTransactionService> _lazyBudgetedTransactionService;
    private readonly Lazy<IImportProfileService> _lazyImportProfileService;
    private readonly Lazy<IRecurringBankTransactionService> _lazyRecurringBankTransactionService;

    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    private TestServiceManager(DbContextOptions<DatabaseContext> dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
        var coreServiceManager = new ServiceManager(dbContextOptions);
        _lazyAccountService = new Lazy<IAccountService>(() => coreServiceManager.AccountService);
        _lazyBankTransactionService = new Lazy<IBankTransactionService>(() => coreServiceManager.BankTransactionService);
        _lazyBucketGroupService = new Lazy<IBucketGroupService>(() => coreServiceManager.BucketGroupService);
        _lazyBucketMovementService = new Lazy<IBucketMovementService>(() => coreServiceManager.BucketMovementService);
        _lazyBucketService = new Lazy<IBucketService>(() => coreServiceManager.BucketService);
        _lazyBucketRuleSetService = new Lazy<IBucketRuleSetService>(() => coreServiceManager.BucketRuleSetService);
        _lazyBudgetedTransactionService = new Lazy<IBudgetedTransactionService>(() => coreServiceManager.BudgetedTransactionService);
        _lazyImportProfileService = new Lazy<IImportProfileService>(() => coreServiceManager.ImportProfileService);
        _lazyRecurringBankTransactionService = new Lazy<IRecurringBankTransactionService>(() => coreServiceManager.RecurringBankTransactionService);
    }

    public static TestServiceManager CreateUsingSqlite(string dbName)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [ConfigurationKeyConstants.CONNECTION_PROVIDER] = ConfigurationKeyConstants.PROVIDER_SQLITE,
                [ConfigurationKeyConstants.CONNECTION_DATABASE] = $"{dbName}.db"
            }!)
            .Build();

        return GenerateTestServiceManager(configuration);
    }

    public static TestServiceManager CreateUsingMySql()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [ConfigurationKeyConstants.CONNECTION_PROVIDER] = ConfigurationKeyConstants.PROVIDER_MYSQL,
                [ConfigurationKeyConstants.CONNECTION_SERVER] = "mariadb-dev.home.lab",
                [ConfigurationKeyConstants.CONNECTION_PORT] = "3306",
                [ConfigurationKeyConstants.CONNECTION_USER] = "openbudgeteer_unit_test",
                [ConfigurationKeyConstants.CONNECTION_PASSWORD] = "openbudgeteer_unit_test",
                [ConfigurationKeyConstants.CONNECTION_DATABASE] = "openbudgeteer_unit_test"
            }!)
            .Build();

        return GenerateTestServiceManager(configuration);
    }

    private static TestServiceManager GenerateTestServiceManager(IConfiguration configuration)
    {
        var contextOptions = DbContextOptionsFactory.GetContextOptions(configuration);
        var dbContext = new DatabaseContext(contextOptions);
        dbContext.Database.Migrate();

        return new TestServiceManager(contextOptions);
    }
    
    public void CleanupDatabase()
    {
        using var dbContext = new DatabaseContext(_dbContextOptions);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(new AccountRepository(dbContext));
        dbContext.SaveChanges();
        DeleteAllExtension<IBankTransactionRepository, BankTransaction>.DeleteAll(new BankTransactionRepository(dbContext));
        dbContext.SaveChanges();
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(new BucketRepository(dbContext));
        dbContext.SaveChanges();
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(new BucketGroupRepository(dbContext));
        dbContext.SaveChanges();
        DeleteAllExtension<IBucketMovementRepository, BucketMovement>.DeleteAll(new BucketMovementRepository(dbContext));
        dbContext.SaveChanges();
        DeleteAllExtension<IBucketRuleSetRepository, BucketRuleSet>.DeleteAll(new BucketRuleSetRepository(dbContext));
        dbContext.SaveChanges();
        DeleteAllExtension<IBudgetedTransactionRepository, BudgetedTransaction>.DeleteAll(new BudgetedTransactionRepository(dbContext));
        dbContext.SaveChanges();
        DeleteAllExtension<IImportProfileRepository, ImportProfile>.DeleteAll(new ImportProfileRepository(dbContext));
        dbContext.SaveChanges();
        DeleteAllExtension<IRecurringBankTransactionRepository, RecurringBankTransaction>.DeleteAll(new RecurringBankTransactionRepository(dbContext));
        dbContext.SaveChanges();
    }
}