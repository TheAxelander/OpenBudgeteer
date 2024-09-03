using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockRecurringBankTransactionRepository : IRecurringBankTransactionRepository
{
    private readonly MockDatabase _mockDatabase;

    public MockRecurringBankTransactionRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }

    public IEnumerable<RecurringBankTransaction> All()
    {
        return _mockDatabase.RecurringBankTransactions.Values;
    }

    public IEnumerable<RecurringBankTransaction> AllWithIncludedEntities()
    {
        var mockAccountRepository = new MockAccountRepository(_mockDatabase);
        var recurringBankTransactions = All().ToList();
        foreach (var recurringBankTransaction in recurringBankTransactions)
        {
            recurringBankTransaction.Account = mockAccountRepository.ById(recurringBankTransaction.AccountId) 
                                    ?? throw new Exception("Account doesn't exist");
        }

        return recurringBankTransactions;
    }

    public RecurringBankTransaction? ById(Guid id)
    {
        _mockDatabase.RecurringBankTransactions.TryGetValue(id, out var result);
        return result;
    }

    public RecurringBankTransaction? ByIdWithIncludedEntities(Guid id)
    {
        var mockAccountRepository = new MockAccountRepository(_mockDatabase);
        var recurringBankTransaction = ById(id);
        if (recurringBankTransaction == null) return recurringBankTransaction;
        recurringBankTransaction.Account = mockAccountRepository.ById(recurringBankTransaction.AccountId) 
                                ?? throw new Exception("Account doesn't exist");

        return recurringBankTransaction;
    }

    public int Create(RecurringBankTransaction entity)
    {
        entity.Id = Guid.NewGuid();
        _mockDatabase.RecurringBankTransactions[entity.Id] = entity;
        return 1;
    }

    public int CreateRange(IEnumerable<RecurringBankTransaction> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(RecurringBankTransaction entity)
    {
        try
        {
            _mockDatabase.RecurringBankTransactions[entity.Id] = entity;
            return 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int UpdateRange(IEnumerable<RecurringBankTransaction> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        try
        {
            return _mockDatabase.RecurringBankTransactions.Remove(id) ? 1 : 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        return ids.Sum(Delete);
    }
}