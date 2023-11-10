using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;

namespace OpenBudgeteer.Core.Data.Services;

public class ServiceManager : IServiceManager
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

    public ServiceManager(DbContextOptions<DatabaseContext> dbContextOptions)
    {
        _lazyAccountService = new Lazy<IAccountService>(() => new AccountService(dbContextOptions));
        _lazyBankTransactionService = new Lazy<IBankTransactionService>(() => new BankTransactionService(dbContextOptions));
        _lazyBucketGroupService = new Lazy<IBucketGroupService>(() => new BucketGroupService(dbContextOptions));
        _lazyBucketMovementService = new Lazy<IBucketMovementService>(() => new BucketMovementService(dbContextOptions));
        _lazyBucketService = new Lazy<IBucketService>(() => new BucketService(dbContextOptions));
        _lazyBucketRuleSetService = new Lazy<IBucketRuleSetService>(() => new BucketRuleSetService(dbContextOptions));
        _lazyBudgetedTransactionService = new Lazy<IBudgetedTransactionService>(() => new BudgetedTransactionService(dbContextOptions));
        _lazyImportProfileService = new Lazy<IImportProfileService>(() => new ImportProfileService(dbContextOptions));
        _lazyRecurringBankTransactionService = new Lazy<IRecurringBankTransactionService>(() => new RecurringBankTransactionService(dbContextOptions));
    }
}