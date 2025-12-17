using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericBudgetedTransactionService<TDatabase> : GenericBaseService<BudgetedTransaction, TDatabase>, IBudgetedTransactionService
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;
    
    public GenericBudgetedTransactionService(ILogger logger) : base(logger)
    {
        _logger = logger;
    }
    
    protected abstract override IBudgetedTransactionRepository CreateBaseRepository(TDatabase dbConnection);

    public virtual IEnumerable<BudgetedTransaction> GetAll(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .AllWithTransactions()
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd)
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }
    
    public virtual IEnumerable<BudgetedTransaction> GetAllForReporting(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .AllWithTransactions()
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd &&
                    !i.Bucket!.IsHiddenFromSummaries)
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }
    
    public virtual IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId)
    {
        return GetAllFromTransaction(transactionId, DateOnly.MinValue, DateOnly.MaxValue);
    }
    
    public virtual IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId, DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .AllWithTransactions()
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd &&
                    i.TransactionId == transactionId)
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public virtual IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId)
    {
        return GetAllFromBucket(bucketId, DateOnly.MinValue, DateOnly.MaxValue);
    }
    
    public virtual IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId, DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .AllWithTransactions()
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd && 
                    i.BucketId == bucketId)
                .OrderByDescending(i => i.Transaction.TransactionDate)
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }
    
    public virtual IEnumerable<BudgetedTransaction> GetAllNonTransfer()
    {
        return GetAllNonTransfer(DateOnly.MinValue, DateOnly.MaxValue);
    }

    public virtual IEnumerable<BudgetedTransaction> GetAllNonTransfer(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .AllWithTransactions()
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd &&
                    i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000002"))
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public virtual IEnumerable<BudgetedTransaction> GetAllTransfer()
    {
        return GetAllTransfer(DateOnly.MinValue, DateOnly.MaxValue);
    }

    public virtual IEnumerable<BudgetedTransaction> GetAllTransfer(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .AllWithTransactions()
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd &&
                    i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000002"))
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public virtual IEnumerable<BudgetedTransaction> GetAllIncome()
    {
        return GetAllIncome(DateOnly.MinValue, DateOnly.MaxValue);
    }

    public virtual IEnumerable<BudgetedTransaction> GetAllIncome(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .AllWithTransactions()
                .Where(i =>
                    i.Transaction.TransactionDate >= periodStart &&
                    i.Transaction.TransactionDate <= periodEnd &&
                    i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }
}