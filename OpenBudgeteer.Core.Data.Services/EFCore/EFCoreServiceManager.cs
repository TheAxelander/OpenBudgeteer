using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreServiceManager : IServiceManager
{
    public IAccountService AccountService => 
        new EFCoreAccountService(_dbContextFactory, new Logger<EFCoreAccountService>(_loggerFactory));
    
    public IBankTransactionService BankTransactionService => 
        new EFCoreBankTransactionService(_dbContextFactory, new Logger<EFCoreBankTransactionService>(_loggerFactory));
    
    public IBucketGroupService BucketGroupService => 
        new EFCoreBucketGroupService(_dbContextFactory, new Logger<EFCoreBucketGroupService>(_loggerFactory));
    
    public IBucketMovementService BucketMovementService => 
        new EFCoreBucketMovementService(_dbContextFactory, new Logger<EFCoreBucketMovementService>(_loggerFactory));
    
    public IBucketService BucketService => 
        new EFCoreBucketService(_dbContextFactory, new Logger<EFCoreBucketService>(_loggerFactory));
    
    public IBucketRuleSetService BucketRuleSetService => 
        new EFCoreBucketRuleSetService(_dbContextFactory, new Logger<EFCoreBucketRuleSetService>(_loggerFactory));
    
    public IBudgetedTransactionService BudgetedTransactionService => 
        new EFCoreBudgetedTransactionService(_dbContextFactory, new Logger<EFCoreBudgetedTransactionService>(_loggerFactory));
    
    public IImportProfileService ImportProfileService => 
        new EFCoreImportProfileService(_dbContextFactory, new Logger<EFCoreImportProfileService>(_loggerFactory));
    
    public IRecurringBankTransactionService RecurringBankTransactionService =>
        new EFCoreRecurringBankTransactionService(_dbContextFactory, new Logger<EFCoreRecurringBankTransactionService>(_loggerFactory));

    /// <summary>Creates a new export service instance backed by the shared DB context factory.</summary>
    public ICsvExportService CsvExportService =>
        new EFCoreCsvExportService(_dbContextFactory, new Logger<EFCoreCsvExportService>(_loggerFactory));

    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;
    
    public EFCoreServiceManager(IDbContextFactory<DatabaseContext> dbContextFactory, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _dbContextFactory = dbContextFactory;
    }

    public ILogger CreateLogger(Type type)
    {
        return _loggerFactory.CreateLogger(type);
    }
}