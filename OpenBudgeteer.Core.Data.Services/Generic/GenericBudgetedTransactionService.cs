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

    public IEnumerable<BudgetedTransaction> GetAll(DateOnly periodStart, DateOnly periodEnd)
    {
        return _budgetedTransactionRepository
            .AllWithTransactions()
            .Where(i =>
                i.Transaction.TransactionDate >= periodStart &&
                i.Transaction.TransactionDate <= periodEnd)
            .ToList();
    }
    
    public IEnumerable<BudgetedTransaction> GetAllForReporting(DateOnly periodStart, DateOnly periodEnd)
    {
        return _budgetedTransactionRepository
            .AllWithTransactions()
            .Where(i =>
                i.Transaction.TransactionDate >= periodStart &&
                i.Transaction.TransactionDate <= periodEnd &&
                !i.Bucket!.IsHiddenFromSummaries)
            .ToList();
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId)
    {
        return GetAllFromTransaction(transactionId, DateOnly.MinValue, DateOnly.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId, DateOnly periodStart, DateOnly periodEnd)
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
        return GetAllFromBucket(bucketId, DateOnly.MinValue, DateOnly.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId, DateOnly periodStart, DateOnly periodEnd)
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
        return GetAllNonTransfer(DateOnly.MinValue, DateOnly.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllNonTransfer(DateOnly periodStart, DateOnly periodEnd)
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
        return GetAllTransfer(DateOnly.MinValue, DateOnly.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllTransfer(DateOnly periodStart, DateOnly periodEnd)
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
        return GetAllIncome(DateOnly.MinValue, DateOnly.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllIncome(DateOnly periodStart, DateOnly periodEnd)
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