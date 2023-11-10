namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IServiceManager
{
    IAccountService AccountService { get; }
    IBankTransactionService BankTransactionService { get; }
    IBucketGroupService BucketGroupService { get; }
    IBucketMovementService BucketMovementService { get; }
    IBucketService BucketService { get; }
    IBucketRuleSetService BucketRuleSetService { get; }
    IBudgetedTransactionService BudgetedTransactionService { get; }
    IImportProfileService ImportProfileService { get; }
    IRecurringBankTransactionService RecurringBankTransactionService { get; }
}