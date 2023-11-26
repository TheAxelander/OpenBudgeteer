using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;

namespace OpenBudgeteer.Core.Data.Services;

public class ServiceManager : IServiceManager
{
    public IAccountService AccountService => new AccountService(_dbContextOptions);
    public IBankTransactionService BankTransactionService => new BankTransactionService(_dbContextOptions);
    public IBucketGroupService BucketGroupService => new BucketGroupService(_dbContextOptions);
    public IBucketMovementService BucketMovementService => new BucketMovementService(_dbContextOptions);
    public IBucketService BucketService => new BucketService(_dbContextOptions);
    public IBucketRuleSetService BucketRuleSetService => new BucketRuleSetService(_dbContextOptions);
    public IBudgetedTransactionService BudgetedTransactionService => new BudgetedTransactionService(_dbContextOptions);
    public IImportProfileService ImportProfileService => new ImportProfileService(_dbContextOptions);
    public IRecurringBankTransactionService RecurringBankTransactionService => new RecurringBankTransactionService(_dbContextOptions);
    
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;
    
    public ServiceManager(DbContextOptions<DatabaseContext> dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }
}