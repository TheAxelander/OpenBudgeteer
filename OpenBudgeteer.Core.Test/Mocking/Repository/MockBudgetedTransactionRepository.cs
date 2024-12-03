using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockBudgetedTransactionRepository : IBudgetedTransactionRepository
{
    private readonly MockDatabase _mockDatabase;

    public MockBudgetedTransactionRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }

    public IQueryable<BudgetedTransaction> All()
    {
        return _mockDatabase.BudgetedTransactions.Values.AsQueryable();
    }

    public IQueryable<BudgetedTransaction> AllWithIncludedEntities()
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var mockBankTransactionRepository = new MockBankTransactionRepository(_mockDatabase);
        var budgetedTransactions = All().ToList();
        foreach (var budgetedTransaction in budgetedTransactions)
        {
            budgetedTransaction.Bucket = mockBucketRepository.ById(budgetedTransaction.BucketId) 
                                         ?? throw new Exception("Bucket doesn't exist");
            budgetedTransaction.Transaction = mockBankTransactionRepository.ById(budgetedTransaction.TransactionId) 
                                  ?? throw new Exception("BankTransaction doesn't exist");
        }

        return budgetedTransactions.AsQueryable();
    }
    
    public IQueryable<BudgetedTransaction> AllWithTransactions()
    {
        var mockAccountRepository = new MockAccountRepository(_mockDatabase);
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var mockBankTransactionRepository = new MockBankTransactionRepository(_mockDatabase);
        var budgetedTransactions = All().ToList();
        foreach (var budgetedTransaction in budgetedTransactions)
        {
            budgetedTransaction.Bucket = mockBucketRepository.ById(budgetedTransaction.BucketId) 
                                         ?? throw new Exception("Bucket doesn't exist");
            budgetedTransaction.Transaction = mockBankTransactionRepository.ById(budgetedTransaction.TransactionId) 
                                              ?? throw new Exception("BankTransaction doesn't exist");
            budgetedTransaction.Transaction.Account = mockAccountRepository.ById(budgetedTransaction.Transaction.AccountId) 
                                                      ?? throw new Exception("Account doesn't exist");
        }

        return budgetedTransactions.AsQueryable();
    }

    public BudgetedTransaction? ById(Guid id)
    {
        _mockDatabase.BudgetedTransactions.TryGetValue(id, out var result);
        return result;
    }

    public BudgetedTransaction? ByIdWithIncludedEntities(Guid id)
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var mockBankTransactionRepository = new MockBankTransactionRepository(_mockDatabase);
        var budgetedTransaction = ById(id);
        if (budgetedTransaction == null) return budgetedTransaction;
        budgetedTransaction.Bucket = mockBucketRepository.ById(budgetedTransaction.BucketId) 
                                     ?? throw new Exception("Bucket doesn't exist");
        budgetedTransaction.Transaction = mockBankTransactionRepository.ById(budgetedTransaction.TransactionId) 
                                          ?? throw new Exception("BankTransaction doesn't exist");

        return budgetedTransaction;
    }
    
    public BudgetedTransaction? ByIdWithTransaction(Guid id)
    {
        var mockAccountRepository = new MockAccountRepository(_mockDatabase);
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var mockBankTransactionRepository = new MockBankTransactionRepository(_mockDatabase);
        var budgetedTransaction = ById(id);
        if (budgetedTransaction == null) return budgetedTransaction;
        budgetedTransaction.Bucket = mockBucketRepository.ById(budgetedTransaction.BucketId) 
                                     ?? throw new Exception("Bucket doesn't exist");
        budgetedTransaction.Transaction = mockBankTransactionRepository.ById(budgetedTransaction.TransactionId) 
                                          ?? throw new Exception("BankTransaction doesn't exist");
        budgetedTransaction.Transaction.Account = mockAccountRepository.ById(budgetedTransaction.Transaction.AccountId) 
                                                  ?? throw new Exception("Account doesn't exist");

        return budgetedTransaction;
    }

    public int Create(BudgetedTransaction entity)
    {
        entity.Id = Guid.NewGuid();
        _mockDatabase.BudgetedTransactions[entity.Id] = entity;
        return 1;
    }

    public int CreateRange(IEnumerable<BudgetedTransaction> entities)
    {
        return entities.Sum(Create);
    }

    [Obsolete("{BudgetedTransaction should not be updated, instead delete and re-create", false)]
    public int Update(BudgetedTransaction entity)
    {
        // try
        // {
        //     _mockDatabase.BudgetedTransactions[entity.Id] = entity;
        //     return 1;
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e);
        //     return 0;
        // }
        throw new NotSupportedException(
            $"{typeof(BudgetedTransaction)} should not be updated, instead delete and re-create");
    }

    [Obsolete("{BudgetedTransaction should not be updated, instead delete and re-create", false)]
    public int UpdateRange(IEnumerable<BudgetedTransaction> entities)
    {
        // return entities.Sum(Update);
        throw new NotSupportedException(
            $"{typeof(BudgetedTransaction)} should not be updated, instead delete and re-create");
    }

    public int Delete(Guid id)
    {
        try
        {
            return _mockDatabase.BudgetedTransactions.Remove(id) ? 1 : 0;
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