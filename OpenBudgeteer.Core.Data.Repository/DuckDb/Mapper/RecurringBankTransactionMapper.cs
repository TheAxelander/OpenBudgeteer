using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class RecurringBankTransactionMapperExtensions
{
    public static RecurringBankTransaction MapWithAccount(this RecurringBankTransaction transaction, Account account)
    {
        transaction.Account = account;
        return transaction;
    }
}

public class RecurringBankTransactionMapper
{
    public IEnumerable<RecurringBankTransaction> Results => _cache.Values;

    private readonly Dictionary<Guid, RecurringBankTransaction> _cache = new();

    public RecurringBankTransaction MapWithEverything(RecurringBankTransaction transaction, Account account)
    {
        var result = GetOrAdd(transaction);
        return result.MapWithAccount(account);
    }

    private RecurringBankTransaction GetOrAdd(RecurringBankTransaction transaction)
    {
        if (_cache.TryGetValue(transaction.Id, out var existing))
            return existing;

        existing = transaction;
        _cache.Add(transaction.Id, existing);

        return existing;
    }
}
