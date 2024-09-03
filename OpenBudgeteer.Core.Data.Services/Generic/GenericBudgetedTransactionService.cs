using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericBudgetedTransactionService : GenericBaseService<BudgetedTransaction>, IBudgetedTransactionService
{
    private readonly IBudgetedTransactionRepository _budgetedTransactionRepository;
    
    public GenericBudgetedTransactionService(
        IBudgetedTransactionRepository budgetedTransactionRepository) : base(budgetedTransactionRepository)
    {
        _budgetedTransactionRepository = budgetedTransactionRepository;
    }

    public IEnumerable<BudgetedTransaction> GetAll(DateTime periodStart, DateTime periodEnd)
    {
        return _budgetedTransactionRepository
            .AllWithTransactions()
            .Where(i =>
                i.Transaction.TransactionDate >= periodStart &&
                i.Transaction.TransactionDate <= periodEnd)
            .ToList();
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId)
    {
        return GetAllFromTransaction(transactionId, DateTime.MinValue, DateTime.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId, DateTime periodStart, DateTime periodEnd)
    {
        return _budgetedTransactionRepository
            .AllWithTransactions()
            .Where(i =>
                i.Transaction.TransactionDate >= periodStart &&
                i.Transaction.TransactionDate <= periodEnd &&
                i.TransactionId == transactionId)
            .ToList();
    }

    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId)
    {
        return GetAllFromBucket(bucketId, DateTime.MinValue, DateTime.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId, DateTime periodStart, DateTime periodEnd)
    {
        return _budgetedTransactionRepository
            .AllWithTransactions()
            .Where(i =>
                i.Transaction.TransactionDate >= periodStart &&
                i.Transaction.TransactionDate <= periodEnd && 
                i.BucketId == bucketId)
            .OrderByDescending(i => i.Transaction.TransactionDate)
            .ToList();
    }
    
    public IEnumerable<BudgetedTransaction> GetAllNonTransfer()
    {
        return GetAllNonTransfer(DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllNonTransfer(DateTime periodStart, DateTime periodEnd)
    {
        return _budgetedTransactionRepository
            .AllWithTransactions()
            .Where(i =>
                i.Transaction.TransactionDate >= periodStart &&
                i.Transaction.TransactionDate <= periodEnd &&
                i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000002"))
            .ToList();
    }

    public IEnumerable<BudgetedTransaction> GetAllTransfer()
    {
        return GetAllTransfer(DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllTransfer(DateTime periodStart, DateTime periodEnd)
    {
        return _budgetedTransactionRepository
            .AllWithTransactions()
            .Where(i =>
                i.Transaction.TransactionDate >= periodStart &&
                i.Transaction.TransactionDate <= periodEnd &&
                i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000002"))
            .ToList();
    }

    public IEnumerable<BudgetedTransaction> GetAllIncome()
    {
        return GetAllIncome(DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllIncome(DateTime periodStart, DateTime periodEnd)
    {
        return _budgetedTransactionRepository
            .AllWithTransactions()
            .Where(i =>
                i.Transaction.TransactionDate >= periodStart &&
                i.Transaction.TransactionDate <= periodEnd &&
                i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
            .ToList();
    }
}