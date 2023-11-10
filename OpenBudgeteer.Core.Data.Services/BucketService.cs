using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class BucketService : BaseService<Bucket>, IBucketService
{
    internal BucketService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions)
    {
    }

    public override IEnumerable<Bucket> GetAll()
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRepository(dbContext);
            
            return repository
                .Where(i => 
                    i.Id != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                    i.Id != Guid.Parse("00000000-0000-0000-0000-000000000002"))
                .OrderBy(i => i.Name)
                .ToList();
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
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRepository(dbContext);
            
            return repository
                .Where(i => 
                    i.Id == Guid.Parse("00000000-0000-0000-0000-000000000001") ||
                    i.Id == Guid.Parse("00000000-0000-0000-0000-000000000002"))
                .ToList();
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
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRepository(dbContext);
            
            return repository
                .Where(i => 
                    // Only valid Buckets of current month
                    i.ValidFrom <= validFrom &&
                    // Only active Buckets
                    (i.IsInactive == false ||
                     // Alternative: Bucket is inactive as of today, but was valid in current selected month
                     (i.IsInactive && i.IsInactiveFrom > validFrom)))
                .OrderBy(i => i.Name)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public BucketVersion GetLatestVersion(Guid bucketId)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketVersionRepository(dbContext);
            
            var result = repository
                .Where(i => i.BucketId == bucketId)
                .OrderByDescending(i => i.Version)
                .ToList();
                
            if (!result.Any()) throw new Exception("No Bucket Version found for the selected month");
            return result.First();
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
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketVersionRepository(dbContext);
            
            var result = repository
                .Where(i => i.BucketId == bucketId)
                .OrderByDescending(i => i.ValidFrom)
                .ToList()
                .FirstOrDefault(i => i.ValidFrom <= yearMonth, null);
            if (result == null) throw new Exception("No Bucket Version found for the selected month");
            return result;
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
            using var dbContext = new DatabaseContext(DbContextOptions);
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bucketMovementRepository = new BucketMovementRepository(dbContext);
            
            var result = budgetedTransactionRepository
                .Where(i => 
                    i.BucketId == bucketId &&
                    i.Transaction.TransactionDate < yearMonth.AddMonths(1))
                .ToList()
                .Sum(i => i.Amount);

            result += bucketMovementRepository
                .Where(i => 
                    i.BucketId == bucketId &&
                    i.MovementDate < yearMonth.AddMonths(1))
                .ToList()
                .Sum(i => i.Amount);

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public Tuple<decimal, decimal> GetInAndOut(Guid bucketId, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bucketMovementRepository = new BucketMovementRepository(dbContext);
            
            decimal input = 0, output = 0;
        
            var bucketTransactionsCurrentMonth = budgetedTransactionRepository
                .Where(i => 
                    i.BucketId == bucketId &&
                    i.Transaction.TransactionDate.Year == yearMonth.Year &&
                    i.Transaction.TransactionDate.Month == yearMonth.Month)
                .ToList();
            
            foreach (var bucketTransaction in bucketTransactionsCurrentMonth)
            {
                if (bucketTransaction.Amount < 0)
                    output += bucketTransaction.Amount;
                else
                    input += bucketTransaction.Amount;
            }

            var bucketMovementsCurrentMonth = bucketMovementRepository
                .Where(i => 
                    i.BucketId == bucketId &&
                    i.MovementDate.Year == yearMonth.Year &&
                    i.MovementDate.Month == yearMonth.Month)
                .ToList();

            foreach (var bucketMovement in bucketMovementsCurrentMonth)
            {
                if (bucketMovement.Amount < 0)
                    output += bucketMovement.Amount;
                else
                    input += bucketMovement.Amount;
            }

            return new Tuple<decimal, decimal>(input, output);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public override Bucket Create(Bucket entity)
    {
        throw new NotSupportedException(
            "Please use alternative Create procedure as creation of a Bucket Version is mandatory");
    }

    public Bucket Create(Bucket bucket, BucketVersion bucketVersion, DateTime yearMonth)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRepository = new BucketRepository(dbContext);
            var bucketVersionRepository = new BucketVersionRepository(dbContext);
            
            bucketRepository.Create(bucket);
            
            bucketVersion.BucketId = bucket.Id;
            bucketVersion.Version = 1;
            bucketVersion.ValidFrom = yearMonth;
            bucketVersionRepository.Create(bucketVersion);
            
            transaction.Commit();
            return bucket;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override Bucket Update(Bucket bucket)
    {
        throw new NotSupportedException(
            "Please use alternative Update procedure as update of a Bucket Version can be required");
    }

    public Bucket Update(Bucket bucket, BucketVersion bucketVersion, DateTime yearMonth)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRepository = new BucketRepository(dbContext);
            var bucketVersionRepository = new BucketVersionRepository(dbContext);
            
            // Check on Bucket changes and update database
            var dbBucket = Get(bucket.Id);
            if (dbBucket.Name != bucket.Name ||
                dbBucket.ColorCode != bucket.ColorCode ||
                dbBucket.BucketGroupId != bucket.BucketGroupId)
            {
                bucketRepository.Update(bucket);
            }

            // Check on BucketVersion changes and create new BucketVersion
            var dbBucketVersion = bucketVersionRepository.ById(bucketVersion.Id);
            if (dbBucketVersion == null) throw new Exception("Current Bucket Version not found for Bucket");
            if (dbBucketVersion.BucketType != bucketVersion.BucketType ||
                dbBucketVersion.BucketTypeXParam != bucketVersion.BucketTypeXParam ||
                dbBucketVersion.BucketTypeYParam != bucketVersion.BucketTypeYParam ||
                dbBucketVersion.BucketTypeZParam != bucketVersion.BucketTypeZParam ||
                dbBucketVersion.Notes != bucketVersion.Notes)
            {
                if (bucketVersionRepository.All().Any(i =>
                        i.BucketId == bucketVersion.BucketId && 
                        i.Version > bucketVersion.Version))
                    throw new Exception("Cannot create new Version as already a newer Version exists");

                if (bucketVersion.ValidFrom == yearMonth)
                {
                    // Bucket Version modified in the same month,
                    // so just update the version instead of creating a new version
                    bucketVersionRepository.Update(bucketVersion);
                }
                else
                {
                    bucketVersion.Version++;
                    bucketVersion.Id = Guid.Empty;
                    bucketVersion.ValidFrom = yearMonth;
                    bucketVersionRepository.Create(bucketVersion);
                }
            }
            
            transaction.Commit();
            return bucket;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public Bucket Close(Bucket entity, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var bucketRepository = new BucketRepository(dbContext);
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bucketMovementRepository = new BucketMovementRepository(dbContext);
            
            if (entity.IsInactive) throw new Exception("Bucket has been already set to inactive");
            if (GetBalance(entity.Id, yearMonth) != 0) throw new Exception("Balance must be 0 to close a Bucket");
            
            if (budgetedTransactionRepository.All().Any(i => i.BucketId == entity.Id) ||
                bucketMovementRepository.All().Any(i => i.BucketId == entity.Id))
            {
                // Bucket will be set to inactive for the next month
                entity.IsInactive = true;
                entity.IsInactiveFrom = yearMonth.AddMonths(1);
                bucketRepository.Update(entity);
            }
            else
            {
                // Bucket has no transactions & movements, so it can be directly deleted from the database
                bucketRepository.Delete(entity);
            }
            return entity;
                
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override Bucket Delete(Bucket entity)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRepository = new BucketRepository(dbContext);
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bucketMovementRepository = new BucketMovementRepository(dbContext);
            var bucketVersionRepository = new BucketVersionRepository(dbContext);
            var bucketRuleSetRepository = new BucketRuleSetRepository(dbContext);
            
            if (budgetedTransactionRepository.All().Any(i => i.BucketId == entity.Id))
                throw new Exception("Cannot delete Bucket as it is assigned to at least one Bank Transaction");
            if (bucketMovementRepository.All().Any(i => i.BucketId == entity.Id))
                throw new Exception("Cannot delete Bucket as it contains some Bucket Movements");
                
            // Delete Bucket
            bucketRepository.Delete(entity);
                            
            // Delete all BucketVersion which refer to this Bucket
            bucketVersionRepository.DeleteRange(bucketVersionRepository
                .Where(i => i.Id == entity.Id)
                .ToList());
                            
            // Delete all BucketRuleSet which refer to this Bucket
            bucketRuleSetRepository.DeleteRange(bucketRuleSetRepository
                .Where(i => i.TargetBucketId == entity.Id)
                .ToList());
            
            transaction.Commit();
            return entity;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public BucketMovement CreateMovement(Guid bucketId, decimal amount, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketMovementRepository(dbContext);
            
            var newBucketMovement = new BucketMovement()
            {
                Id = Guid.Empty,
                BucketId = bucketId,
                Amount = amount,
                MovementDate = yearMonth
            };
            var result = repository.Create(newBucketMovement);
            if (result == 0) throw new Exception($"Unable to create {typeof(BucketMovement)} in database");
            return newBucketMovement;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}