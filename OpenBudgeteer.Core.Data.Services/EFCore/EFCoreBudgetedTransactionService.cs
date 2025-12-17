using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBudgetedTransactionService : EFCoreBaseService<BudgetedTransaction>, IBudgetedTransactionService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBudgetedTransactionService> _logger;

    public EFCoreBudgetedTransactionService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBudgetedTransactionService> logger) : base(dbContextFactory, logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override GenericBudgetedTransactionService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBudgetedTransactionService(new BudgetedTransactionRepository(dbContext));
    }

    public IEnumerable<BudgetedTransaction> GetAll(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAll(periodStart, periodEnd);
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
    
    public IEnumerable<BudgetedTransaction> GetAllForReporting(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllForReporting(periodStart, periodEnd);
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
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId)
    {
        return GetAllFromTransaction(transactionId, DateOnly.MinValue, DateOnly.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId, DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllFromTransaction(transactionId, periodStart, periodEnd);
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

    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId)
    {
        return GetAllFromBucket(bucketId, DateOnly.MinValue, DateOnly.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId, DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllFromBucket(bucketId, periodStart, periodEnd);
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
    
    public IEnumerable<BudgetedTransaction> GetAllNonTransfer()
    {
        return GetAllNonTransfer(DateOnly.MinValue, DateOnly.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllNonTransfer(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllNonTransfer(periodStart, periodEnd);
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

    public IEnumerable<BudgetedTransaction> GetAllTransfer()
    {
        return GetAllTransfer(DateOnly.MinValue, DateOnly.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllTransfer(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllTransfer(periodStart, periodEnd);
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

    public IEnumerable<BudgetedTransaction> GetAllIncome()
    {
        return GetAllIncome(DateOnly.MinValue, DateOnly.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllIncome(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllIncome(periodStart, periodEnd);
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