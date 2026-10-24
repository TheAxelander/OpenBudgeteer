using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericBucketService<TDatabase> : GenericBaseService<Bucket, TDatabase>, IBucketService
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;

    public GenericBucketService(ILogger logger) : base(logger)
    {
        _logger = logger;
    }

    protected abstract override IBucketRepository CreateBaseRepository(TDatabase dbConnection);
    protected abstract IBucketVersionRepository CreateBucketVersionRepository(TDatabase dbConnection);
    protected abstract IBudgetedTransactionRepository CreateBudgetedTransactionRepository(TDatabase dbConnection);
    protected abstract IBucketMovementRepository CreateBucketMovementRepository(TDatabase dbConnection);
    protected abstract IBucketRuleSetRepository CreateBucketRuleSetRepository(TDatabase dbConnection);

    public virtual Bucket GetWithLatestVersion(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var result = baseRepository.ByIdWithVersions(id);
            if (result is null) throw new EntityNotFoundException("Unable to find Bucket with the given id.");
            result.CurrentVersion = GetLatestVersion(id, DateOnly.FromDateTime(DateTime.Today));
            result.BucketVersions = result.BucketVersions!.OrderByDescending(i => i.Version).ToList();

            return result;
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

    public override IEnumerable<Bucket> GetAll()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .All()
                .OrderBy(i => i.Name)
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

    public virtual IEnumerable<Bucket> GetAllWithoutSystemBuckets()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .All()
                .Where(i =>
                    i.Id != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                    i.Id != Guid.Parse("00000000-0000-0000-0000-000000000002"))
                .OrderBy(i => i.Name)
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

    public virtual IEnumerable<Bucket> GetSystemBuckets()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .All()
                .Where(i =>
                    i.Id == Guid.Parse("00000000-0000-0000-0000-000000000001") ||
                    i.Id == Guid.Parse("00000000-0000-0000-0000-000000000002"))
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

    public virtual IEnumerable<Bucket> GetActiveBuckets(DateOnly validFrom)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var result = baseRepository
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

    public virtual BucketVersion GetLatestVersion(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketVersionRepository = CreateBucketVersionRepository(dbConnection);
            var result = bucketVersionRepository
                .All()
                .Where(i => i.BucketId == bucketId)
                .OrderByDescending(i => i.ValidFrom)
                .ToList()
                .FirstOrDefault(i => i!.ValidFrom <= yearMonth, null);
            if (result is null) throw new EntityNotFoundException("Unable to find Bucket with the given id and month.");
            return result;
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

    public virtual BucketFigures GetFigures(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var bucketWithTransactions = baseRepository.ByIdWithTransactions(bucketId) ?? throw new EntityNotFoundException("Unable to find Bucket with the given id.");
            var bucketWithMovements = baseRepository.ByIdWithMovements(bucketId) ?? throw new EntityNotFoundException("Unable to find Bucket with the given id.");

            decimal input = 0, output = 0, balance = 0;

            // Calculate Balance
            var bucketTransactionHistory = bucketWithTransactions.BudgetedTransactions?
                .Where(i => i.Transaction.TransactionDate < yearMonth.AddMonths(1))
                .ToList() ?? [];
            var bucketMovementsHistory = bucketWithMovements.BucketMovements?
                .Where(i => i.MovementDate < yearMonth.AddMonths(1))
                .ToList() ?? [];

            balance = bucketTransactionHistory.Sum(i => i.Amount);
            balance += bucketMovementsHistory.Sum(i => i.Amount);


            // Calculate In & Out
            var bucketTransactionsCurrentMonth = bucketWithTransactions.BudgetedTransactions?
                .Where(i =>
                    i.Transaction.TransactionDate.Year == yearMonth.Year &&
                    i.Transaction.TransactionDate.Month == yearMonth.Month)
                .ToList() ?? [];

            foreach (var bucketTransaction in bucketTransactionsCurrentMonth)
            {
                if (bucketTransaction.Amount < 0)
                    output += bucketTransaction.Amount;
                else
                    input += bucketTransaction.Amount;
            }

            var bucketMovementsCurrentMonth = bucketWithMovements.BucketMovements?
                .Where(i =>
                    i.MovementDate.Year == yearMonth.Year &&
                    i.MovementDate.Month == yearMonth.Month)
                .ToList() ?? [];

            foreach (var bucketMovement in bucketMovementsCurrentMonth)
            {
                if (bucketMovement.Amount < 0)
                    output += bucketMovement.Amount;
                else
                    input += bucketMovement.Amount;
            }

            return new BucketFigures(balance, input, output);
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

    public virtual decimal GetBalance(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var bucketWithTransactions = baseRepository.ByIdWithTransactions(bucketId) ?? throw new EntityNotFoundException("Unable to find Bucket with the given id.");
            var bucketWithMovements = baseRepository.ByIdWithMovements(bucketId) ?? throw new EntityNotFoundException("Unable to find Bucket with the given id.");

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

    public virtual BucketFigures GetInAndOut(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var bucketWithTransactions = baseRepository.ByIdWithTransactions(bucketId) ?? throw new EntityNotFoundException("Unable to find Bucket with the given id.");
            var bucketWithMovements = baseRepository.ByIdWithMovements(bucketId) ?? throw new EntityNotFoundException("Unable to find Bucket with the given id.");

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

            return new BucketFigures(null, input, output);
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

    public override Bucket Create(Bucket entity)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            if (entity.CurrentVersion is null) throw new EntityUpdateException("No Bucket Version defined.");

            entity.CurrentVersion.Version = 1;
            entity.BucketVersions = new List<BucketVersion>();
            entity.BucketVersions.Add(entity.CurrentVersion);

            baseRepository.Create(entity);
            return entity;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to create Bucket in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override Bucket Update(Bucket entity)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            if (entity.CurrentVersion is not null)
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
                    var latestVersion = GetLatestVersion(entity.Id, DateOnly.FromDateTime(DateTime.Today));
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

            baseRepository.Update(entity);
            return entity;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to update Bucket in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public virtual void Close(Guid id, DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var budgetedTransactionRepository = CreateBudgetedTransactionRepository(dbConnection);
            var bucketMovementRepository = CreateBucketMovementRepository(dbConnection);

            if (GetBalance(id, yearMonth) != 0) throw new EntityUpdateException("Balance must be 0 to close a Bucket.");

            if (budgetedTransactionRepository.All().Any(i => i.BucketId == id) ||
                bucketMovementRepository.All().Any(i => i.BucketId == id))
            {
                // Update: Bucket will be set to inactive for the next month
                var entity = baseRepository.ById(id);
                if (entity is null) throw new EntityUpdateException("Unable to find Bucket with the given id.");
                if (entity.IsInactive) throw new EntityUpdateException("Bucket has been already set to inactive.");
                entity.IsInactive = true;
                entity.IsInactiveFrom = yearMonth.AddMonths(1);
                baseRepository.Update(entity);
            }
            else
            {
                // Delete: Bucket has no transactions & movements, so it can be directly deleted from the database
                baseRepository.Delete(id);
            }
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to close Bucket in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }

    }

    public override void Delete(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var budgetedTransactionRepository = CreateBudgetedTransactionRepository(dbConnection);
            var bucketMovementRepository = CreateBucketMovementRepository(dbConnection);
            var bucketRuleSetRepository = CreateBucketRuleSetRepository(dbConnection);

            if (budgetedTransactionRepository.All().Any(i => i.BucketId == id) ||
                bucketMovementRepository.All().Any(i => i.BucketId == id))
            {
                throw new EntityUpdateException("Cannot delete a Bucket with assigned Transactions or Bucket Movements.");
            }

            // Delete Bucket
            baseRepository.Delete(id);

            // Delete all BucketRuleSet which refer to this Bucket
            var bucketRuleSetIds = bucketRuleSetRepository
                .All()
                .Where(i => i.TargetBucketId == id)
                .Select(i => i.Id)
                .ToList();
            if (bucketRuleSetIds.Count != 0) bucketRuleSetRepository.DeleteRange(bucketRuleSetIds);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to delete Bucket in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public virtual BucketMovement CreateMovement(Guid bucketId, decimal amount, DateOnly movementDate)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketMovementRepository = CreateBucketMovementRepository(dbConnection);
            var newBucketMovement = new BucketMovement()
            {
                Id = Guid.Empty,
                BucketId = bucketId,
                Amount = amount,
                MovementDate = movementDate
            };
            var result = bucketMovementRepository.Create(newBucketMovement);
            if (result == 0) throw new EntityUpdateException("Bucket Movement has not been created.");
            return newBucketMovement;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to create Bucket Movement in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }

    }
}
