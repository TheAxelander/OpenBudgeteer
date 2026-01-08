using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class BudgetedTransactionMapperExtensions
{
    public static BudgetedTransaction MapWithBucket(this BudgetedTransaction budgetedTransaction, Bucket bucket)
    {
        budgetedTransaction.Bucket = bucket;
        return budgetedTransaction;
    }

    public static BudgetedTransaction MapWithTransaction(
        this BudgetedTransaction budgetedTransaction,
        BankTransaction transaction)
    {
        budgetedTransaction.Transaction = transaction;
        return budgetedTransaction;
    }

    public static BudgetedTransaction MapWithTransactionIncludingAccount(
        this BudgetedTransaction budgetedTransaction,
        BankTransaction transaction,
        Account account)
    {
        budgetedTransaction.Transaction = transaction;
        budgetedTransaction.Transaction.Account = account;
        return budgetedTransaction;
    }
}

public class BudgetedTransactionMapper
{
    public IEnumerable<BudgetedTransaction> Results => _cache.Values;

    private readonly Dictionary<Guid, BudgetedTransaction> _cache = new();

    public BudgetedTransaction MapWithEverything(
        BudgetedTransaction budgetedTransaction,
        Bucket bucket,
        BankTransaction transaction)
    {
        var result = GetOrAdd(budgetedTransaction);
        return result
            .MapWithBucket(bucket)
            .MapWithTransaction(transaction);
    }

    public BudgetedTransaction MapWithEverythingIncludingAccount(
        BudgetedTransaction budgetedTransaction,
        Bucket bucket,
        BankTransaction transaction,
        Account account)
    {
        var result = GetOrAdd(budgetedTransaction);
        return result
            .MapWithBucket(bucket)
            .MapWithTransactionIncludingAccount(transaction, account);
    }

    private BudgetedTransaction GetOrAdd(BudgetedTransaction budgetedTransaction)
    {
        if (_cache.TryGetValue(budgetedTransaction.Id, out BudgetedTransaction? existing))
            return existing;

        existing = budgetedTransaction;
        _cache.Add(budgetedTransaction.Id, existing);

        return existing;
    }
}
