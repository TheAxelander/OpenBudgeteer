using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreServiceManager : IServiceManager
{
    public IAccountService AccountService => new EFCoreAccountService(_dbContextOptions);
    public IBankTransactionService BankTransactionService => new EFCoreBankTransactionService(_dbContextOptions);
    public IBucketGroupService BucketGroupService => new EFCoreBucketGroupService(_dbContextOptions);
    public IBucketMovementService BucketMovementService => new EFCoreBucketMovementService(_dbContextOptions);
    public IBucketService BucketService => new EFCoreBucketService(_dbContextOptions);
    public IBucketRuleSetService BucketRuleSetService => new EFCoreBucketRuleSetService(_dbContextOptions);
    public IBudgetedTransactionService BudgetedTransactionService => new EFCoreBudgetedTransactionService(_dbContextOptions);
    public IImportProfileService ImportProfileService => new EFCoreImportProfileService(_dbContextOptions);
    public IRecurringBankTransactionService RecurringBankTransactionService => new EFCoreRecurringBankTransactionService(_dbContextOptions);
    
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;
    
    public EFCoreServiceManager(DbContextOptions<DatabaseContext> dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }
}