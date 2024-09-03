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

    public IEnumerable<BudgetedTransaction> GetAll(DateTime periodStart, DateTime periodEnd)
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
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId)
    {
        return GetAllFromTransaction(transactionId, DateTime.MinValue, DateTime.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId, DateTime periodStart, DateTime periodEnd)
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
        return GetAllFromBucket(bucketId, DateTime.MinValue, DateTime.MaxValue);
    }
    
    public IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId, DateTime periodStart, DateTime periodEnd)
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
        return GetAllNonTransfer(DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllNonTransfer(DateTime periodStart, DateTime periodEnd)
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
        return GetAllTransfer(DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllTransfer(DateTime periodStart, DateTime periodEnd)
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
        return GetAllIncome(DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BudgetedTransaction> GetAllIncome(DateTime periodStart, DateTime periodEnd)
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