using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class BankTransactionMapperExtensions
{
    public static BankTransaction MapWithAccount(this BankTransaction transaction, Account account)
    {
        transaction.Account = account;
        return transaction;
    }

    public static BankTransaction MapWithBudgetedTransaction(
        this BankTransaction transaction,
        BudgetedTransaction? budgetedTransaction)
    {
        BaseMapperExtensions.AddWithBackReference(
            transaction.BudgetedTransactions ??= new List<BudgetedTransaction>(),
            budgetedTransaction,
            bt => bt.Id,
            bt => bt.Transaction = transaction);
        return transaction;
    }
}

public class BankTransactionMapper
{
    public IEnumerable<BankTransaction> Results => _cache.Values;

    private readonly Dictionary<Guid, BankTransaction> _cache = new();

    public BankTransaction MapWithAccount(BankTransaction transaction, Account account)
    {
        var result = GetOrAdd(transaction);
        return result.MapWithAccount(account);
    }

    public BankTransaction MapWithEverything(
        BankTransaction transaction,
        Account account,
        BudgetedTransaction? budgetedTransaction)
    {
        var result = GetOrAdd(transaction);
        return result
            .MapWithAccount(account)
            .MapWithBudgetedTransaction(budgetedTransaction);
    }

    private BankTransaction GetOrAdd(BankTransaction transaction)
    {
        if (_cache.TryGetValue(transaction.Id, out BankTransaction? existing))
            return existing;

        existing = transaction;
        _cache.Add(transaction.Id, existing);

        return existing;
    }
}
