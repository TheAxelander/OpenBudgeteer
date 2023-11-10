using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class BudgetedTransactionService : BaseService<BudgetedTransaction>, IBudgetedTransactionService
{
    internal BudgetedTransactionService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions)
    {
    }

    public IEnumerable<BudgetedTransaction> GetAll(DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BudgetedTransactionRepository(dbContext);
            
            return repository
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId)
    {
        return GetAllFromTransaction(transactionId, DateTime.MinValue, DateTime.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId, DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BudgetedTransactionRepository(dbContext);
            
            return repository
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd &&
                    i.TransactionId == transactionId)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId)
    {
        return GetAllFromBucket(bucketId, DateTime.MinValue, DateTime.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId, DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BudgetedTransactionRepository(dbContext);
            
            return repository
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd && 
                    i.BucketId == bucketId)
                .OrderByDescending(i => i.Transaction.TransactionDate)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public IEnumerable<BudgetedTransaction> GetAllNonTransfer()
    {
        return GetAllNonTransfer(DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllNonTransfer(DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BudgetedTransactionRepository(dbContext);
            
            return repository
                .Where(i =>
                        i.Transaction.TransactionDate >= periodStart &&
                        i.Transaction.TransactionDate <= periodEnd &&
                        i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000002"))
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<BudgetedTransaction> GetAllTransfer()
    {
        return GetAllTransfer(DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllTransfer(DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BudgetedTransactionRepository(dbContext);
            
            return repository
                .Where(i =>
                        i.Transaction.TransactionDate >= periodStart &&
                        i.Transaction.TransactionDate <= periodEnd &&
                        i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000002"))
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<BudgetedTransaction> GetAllIncome()
    {
        return GetAllIncome(DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllIncome(DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BudgetedTransactionRepository(dbContext);
            
            return repository
                .Where(i =>
                        i.Transaction.TransactionDate >= periodStart &&
                        i.Transaction.TransactionDate <= periodEnd &&
                        i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
}