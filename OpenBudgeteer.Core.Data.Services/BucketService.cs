using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class BucketService : BaseService<Bucket>, IBucketService
{
    internal BucketService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions, new BucketRepository(new DatabaseContext(dbContextOptions)))
    {
    }

    public Bucket GetWithLatestVersion(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRepository(dbContext);
            
            var result = repository.ByIdWithVersions(id);
            if (result == null) throw new Exception($"{typeof(Bucket)} not found in database");
            result.CurrentVersion = GetLatestVersion(id, DateTime.Now);
            result.BucketVersions = result.BucketVersions!.OrderByDescending(i => i.Version).ToList();
            
            return result;
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
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRepository(dbContext);
            
            return repository
                .All()
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
                .All()
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
            
            var result = repository
                .AllWithVersions()
                .Where(i => 
                    // Only valid Buckets of current month
                    i.ValidFrom <= validFrom &&
                    // Only active Buckets
                    (i.IsInactive == false ||
                     // Alternative: Bucket is inactive as of today, but was valid in current selected month
                     (i.IsInactive && i.IsInactiveFrom > validFrom)))
                .OrderBy(i => i.Name)
                .ToList();
            foreach (var bucket in result)
            {
                bucket.CurrentVersion = bucket.BucketVersions!
                    .OrderByDescending(i => i.ValidFrom)
                    .ToList()
                    .First(i => i.ValidFrom <= validFrom);
            }

            return result;
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
                .All()
                .Where(i => i.BucketId == bucketId)
                .OrderByDescending(i => i.ValidFrom)
                .ToList()
                .FirstOrDefault(i => i!.ValidFrom <= yearMonth, null);
            if (result == null) throw new Exception("No Bucket Version found for the selected month");
            return result;
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
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRepository(dbContext);
            var bucketWithTransactions = repository.ByIdWithTransactions(bucketId) ?? throw new Exception("Bucket not found.");
            var bucketWithMovements = repository.ByIdWithMovements(bucketId) ?? throw new Exception("Bucket not found.");
            
            decimal input = 0, output = 0;

            // Calculate Balance
            var balance = bucketWithTransactions.BudgetedTransactions!
                .Where(i => i.Transaction.TransactionDate < yearMonth.AddMonths(1))
                .ToList()
                .Sum(i => i.Amount);

            balance += bucketWithMovements.BucketMovements!
                .Where(i => i.MovementDate < yearMonth.AddMonths(1))
                .ToList()
                .Sum(i => i.Amount);

        
            // Calculate In & Out
            var bucketTransactionsCurrentMonth = bucketWithTransactions.BudgetedTransactions!
                .Where(i => 
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

            var bucketMovementsCurrentMonth = bucketWithMovements.BucketMovements!
                .Where(i => 
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

            return new BucketFigures{ Balance = balance, Input = input, Output = output };
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
            var repository = new BucketRepository(dbContext);
            var bucketWithTransactions = repository.ByIdWithTransactions(bucketId) ?? throw new Exception("Bucket not found.");
            var bucketWithMovements = repository.ByIdWithMovements(bucketId) ?? throw new Exception("Bucket not found.");
            
            var result = bucketWithTransactions.BudgetedTransactions!
                .Where(i => i.Transaction.TransactionDate < yearMonth.AddMonths(1))
                .ToList()
                .Sum(i => i.Amount);

            result += bucketWithMovements.BucketMovements!
                .Where(i => i.MovementDate < yearMonth.AddMonths(1))
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

    public BucketFigures GetInAndOut(Guid bucketId, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRepository(dbContext);
            var bucketWithTransactions = repository.ByIdWithTransactions(bucketId) ?? throw new Exception("Bucket not found.");
            var bucketWithMovements = repository.ByIdWithMovements(bucketId) ?? throw new Exception("Bucket not found.");
            
            decimal input = 0, output = 0;
        
            var bucketTransactionsCurrentMonth = bucketWithTransactions.BudgetedTransactions!
                .Where(i => 
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

            var bucketMovementsCurrentMonth = bucketWithMovements.BucketMovements!
                .Where(i => 
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

            return new BucketFigures{ Balance = null, Input = input, Output = output };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public override Bucket Create(Bucket entity)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var repository = new BucketRepository(dbContext);
            if (entity.CurrentVersion == null) throw new Exception("No Bucket Version defined");

            entity.CurrentVersion.Version = 1;
            entity.BucketVersions = new List<BucketVersion>();
            entity.BucketVersions.Add(entity.CurrentVersion);
            
            repository.Create(entity);
            
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

    public override Bucket Update(Bucket entity)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRepository = new BucketRepository(dbContext);

            if (entity.CurrentVersion != null)
            {
                entity.BucketVersions = new List<BucketVersion>();
                if (entity.Id == Guid.Empty)
                {
                    // New Bucket - Create new Version
                    var newVersion = entity.CurrentVersion;
                    newVersion.Id = Guid.Empty;
                    newVersion.Version = 1;
                    entity.BucketVersions.Add(newVersion);
                }
                else
                {
                    var latestVersion = GetLatestVersion(entity.Id, DateTime.Now);
                    if (entity.CurrentVersion.ValidFrom == latestVersion.ValidFrom)
                    {
                        // Change in same month, overwrite latest Version
                        latestVersion.BucketType = entity.CurrentVersion.BucketType;
                        latestVersion.BucketTypeXParam = entity.CurrentVersion.BucketTypeXParam;
                        latestVersion.BucketTypeYParam = entity.CurrentVersion.BucketTypeYParam;
                        latestVersion.BucketTypeZParam = entity.CurrentVersion.BucketTypeZParam;
                        latestVersion.Notes = entity.CurrentVersion.Notes;

                        entity.BucketVersions.Add(latestVersion);
                    }
                    else
                    {
                        // Create new Version
                        var newVersion = entity.CurrentVersion;
                        newVersion.Id = Guid.Empty;
                        newVersion.Version = latestVersion.Version + 1;
                        entity.BucketVersions.Add(newVersion);
                    }
                }
            }

            bucketRepository.Update(entity);
            
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

    public void Close(Guid id, DateTime yearMonth)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var bucketRepository = new BucketRepository(dbContext);
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bucketMovementRepository = new BucketMovementRepository(dbContext);

            if (GetBalance(id, yearMonth) != 0) throw new Exception("Balance must be 0 to close a Bucket");
            
            if (budgetedTransactionRepository.All().Any(i => i.BucketId == id) ||
                bucketMovementRepository.All().Any(i => i.BucketId == id))
            {
                // Update: Bucket will be set to inactive for the next month
                var entity = bucketRepository.ById(id);
                if (entity == null) throw new Exception("Bucket not found");
                if (entity.IsInactive) throw new Exception("Bucket has been already set to inactive");
                entity.IsInactive = true;
                entity.IsInactiveFrom = yearMonth.AddMonths(1);
                bucketRepository.Update(entity);
            }
            else
            {
                // Delete: Bucket has no transactions & movements, so it can be directly deleted from the database
                bucketRepository.Delete(id);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override void Delete(Guid id)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRepository = new BucketRepository(dbContext);
            var bucketRuleSetRepository = new BucketRuleSetRepository(dbContext);
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bucketMovementRepository = new BucketMovementRepository(dbContext);

            if (budgetedTransactionRepository.All().Any(i => i.BucketId == id) ||
                bucketMovementRepository.All().Any(i => i.BucketId == id))
            {
                throw new Exception("Cannot delete a Bucket with assigned Transactions or Bucket Movements");
            }
            
            // Delete Bucket
            bucketRepository.Delete(id);
                            
            // Delete all BucketRuleSet which refer to this Bucket
            var bucketRuleSetIds = bucketRuleSetRepository
                .All()
                .Where(i => i.TargetBucketId == id)
                .Select(i => i.Id)
                .ToList();
            if (bucketRuleSetIds.Count != 0) bucketRuleSetRepository.DeleteRange(bucketRuleSetIds);
            
            transaction.Commit();
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