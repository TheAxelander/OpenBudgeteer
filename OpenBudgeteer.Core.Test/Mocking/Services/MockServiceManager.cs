using OpenBudgeteer.Core.Data.Contracts.Services;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockServiceManager : IServiceManager
{
    public IAccountService AccountService { get; }
    public IBankTransactionService BankTransactionService { get; }
    public IBucketGroupService BucketGroupService { get; }
    public IBucketMovementService BucketMovementService { get; }
    public IBucketService BucketService { get; }
    public IBucketRuleSetService BucketRuleSetService { get; }
    public IBudgetedTransactionService BudgetedTransactionService { get; }
    public IImportProfileService ImportProfileService { get; }
    public IRecurringBankTransactionService RecurringBankTransactionService { get; }

    public MockServiceManager(MockDatabase mockDatabase)
    {
        AccountService = new MockAccountService(mockDatabase);
        BankTransactionService = new MockBankTransactionService(mockDatabase);
        BucketGroupService = new MockBucketGroupService(mockDatabase);
        BucketMovementService = new MockBucketMovementService(mockDatabase);
        BucketService = new MockBucketService(mockDatabase);
        BucketRuleSetService = new MockBucketRuleSetService(mockDatabase);
        BudgetedTransactionService = new MockBudgetedTransactionService(mockDatabase);
        ImportProfileService = new MockImportProfileService(mockDatabase);
        RecurringBankTransactionService = new MockRecurringBankTransactionService(mockDatabase);
    }
}