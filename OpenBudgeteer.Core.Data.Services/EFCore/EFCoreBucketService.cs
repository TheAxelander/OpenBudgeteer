using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketService : EFCoreBaseService<Bucket>, IBucketService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    public EFCoreBucketService(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected override GenericBucketService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBucketService(
            new BucketRepository(dbContext),
            new BucketVersionRepository(dbContext),
            new BudgetedTransactionRepository(dbContext),
            new BucketMovementRepository(dbContext),
            new BucketRuleSetRepository(dbContext));
    }

    public Bucket GetWithLatestVersion(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetWithLatestVersion(id);
        }
        catch (EntityNotFoundException e)
        {
            Console.WriteLine(e);
            throw new Exception($"{typeof(Bucket)} not found in database");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<Bucket> GetAllWithoutSystemBuckets()
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllWithoutSystemBuckets();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public override IEnumerable<Bucket> GetAll()
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAll();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<Bucket> GetSystemBuckets()
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetSystemBuckets();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<Bucket> GetActiveBuckets(DateTime validFrom)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetActiveBuckets(validFrom);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public BucketVersion GetLatestVersion(Guid bucketId, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetLatestVersion(bucketId, yearMonth);
        }
        catch (EntityNotFoundException e)
        {
            Console.WriteLine(e);
            throw new Exception("No Bucket Version found for the selected month");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public BucketFigures GetFigures(Guid bucketId, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetFigures(bucketId, yearMonth);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public decimal GetBalance(Guid bucketId, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetBalance(bucketId, yearMonth);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public BucketFigures GetInAndOut(Guid bucketId, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetInAndOut(bucketId, yearMonth);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public override Bucket Create(Bucket entity)
    {
        using var dbContext = new DatabaseContext(_dbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        var baseService = CreateBaseService(dbContext);
        try
        {
            var result = baseService.Create(entity);
            transaction.Commit();
            return result;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception(e.Message);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override Bucket Update(Bucket entity)
    {
        using var dbContext = new DatabaseContext(_dbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        var baseService = CreateBaseService(dbContext);
        try
        {
            var result = baseService.Update(entity);
            transaction.Commit();
            return result;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public void Close(Guid id, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            baseService.Close(id, yearMonth);
        }
        catch (EntityUpdateException e)
        {
            Console.WriteLine(e);
            throw new Exception(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override void Delete(Guid id)
    {
        using var dbContext = new DatabaseContext(_dbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        var baseService = CreateBaseService(dbContext);
        try
        {
            baseService.Delete(id);
            transaction.Commit();
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception(e.Message);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public BucketMovement CreateMovement(Guid bucketId, decimal amount, DateTime movementDate)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.CreateMovement(bucketId, amount, movementDate);
        }
        catch (EntityUpdateException e)
        {
            Console.WriteLine(e);
            throw new Exception(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}