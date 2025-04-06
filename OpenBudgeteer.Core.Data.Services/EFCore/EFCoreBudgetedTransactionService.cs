using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBudgetedTransactionService : EFCoreBaseService<BudgetedTransaction>, IBudgetedTransactionService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    public EFCoreBudgetedTransactionService(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected override GenericBudgetedTransactionService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBudgetedTransactionService(new BudgetedTransactionRepository(dbContext));
    }

    public IEnumerable<BudgetedTransaction> GetAll(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAll(periodStart, periodEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public IEnumerable<BudgetedTransaction> GetAllForReporting(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllForReporting(periodStart, periodEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllFromTransaction(transactionId, periodStart, periodEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllFromBucket(bucketId, periodStart, periodEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllNonTransfer(periodStart, periodEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllTransfer(periodStart, periodEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllIncome(periodStart, periodEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
}