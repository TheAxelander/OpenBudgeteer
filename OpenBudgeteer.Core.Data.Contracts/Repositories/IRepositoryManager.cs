namespace OpenBudgeteer.Core.Data.Contracts.Repositories;

public interface IRepositoryManager : IDisposable
{
    IAccountRepository AccountRepository { get; }
    IBankTransactionRepository BankTransactionRepository { get; }
    IBucketGroupRepository BucketGroupRepository { get; }
    IBucketMovementRepository BucketMovementRepository { get; }
    IBucketRepository BucketRepository { get; }
    IBucketRuleSetRepository BucketRuleSetRepository { get; }
    IBucketVersionRepository BucketVersionRepository { get; }
    IBudgetedTransactionRepository BudgetedTransactionRepository { get; }
    IImportProfileRepository ImportProfileRepository { get; }
    IMappingRuleRepository MappingRuleRepository { get; }
    IRecurringBankTransactionRepository RecurringBankTransactionRepository { get; }
}