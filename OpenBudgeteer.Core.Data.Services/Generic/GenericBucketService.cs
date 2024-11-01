using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericBucketService : GenericBaseService<Bucket>, IBucketService
{
    private readonly IBucketRepository _bucketRepository;
    private readonly IBucketVersionRepository _bucketVersionRepository;
    private readonly IBudgetedTransactionRepository _budgetedTransactionRepository;
    private readonly IBucketMovementRepository _bucketMovementRepository;
    private readonly IBucketRuleSetRepository _bucketRuleSetRepository;

    public GenericBucketService(
        IBucketRepository bucketRepository, 
        IBucketVersionRepository bucketVersionRepository, 
        IBudgetedTransactionRepository budgetedTransactionRepository, 
        IBucketMovementRepository bucketMovementRepository, 
        IBucketRuleSetRepository bucketRuleSetRepository) : base(bucketRepository)
    {
        _bucketRepository = bucketRepository;
        _bucketVersionRepository = bucketVersionRepository;
        _budgetedTransactionRepository = budgetedTransactionRepository;
        _bucketMovementRepository = bucketMovementRepository;
        _bucketRuleSetRepository = bucketRuleSetRepository;
    }

    public Bucket GetWithLatestVersion(Guid id)
    {
        var result = _bucketRepository.ByIdWithVersions(id);
        if (result == null) throw new EntityNotFoundException();
        result.CurrentVersion = GetLatestVersion(id, DateTime.Now);
        result.BucketVersions = result.BucketVersions!.OrderByDescending(i => i.Version).ToList();
            
        return result;
    }

    public override IEnumerable<Bucket> GetAll()
    {
        return _bucketRepository
            .All()
            .OrderBy(i => i.Name)
            .ToList();
    }
    
    public IEnumerable<Bucket> GetAllWithoutSystemBuckets()
    {
        return _bucketRepository
            .All()
            .Where(i => 
                i.Id != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                i.Id != Guid.Parse("00000000-0000-0000-0000-000000000002"))
            .OrderBy(i => i.Name)
            .ToList();
    }

    public IEnumerable<Bucket> GetSystemBuckets()
    {
        return _bucketRepository
            .All()
            .Where(i => 
                i.Id == Guid.Parse("00000000-0000-0000-0000-000000000001") ||
                i.Id == Guid.Parse("00000000-0000-0000-0000-000000000002"))
            .ToList();
    }

    public IEnumerable<Bucket> GetActiveBuckets(DateTime validFrom)
    {
        var result = _bucketRepository
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
    
    public BucketVersion GetLatestVersion(Guid bucketId, DateTime yearMonth)
    {
        var result = _bucketVersionRepository
            .All()
            .Where(i => i.BucketId == bucketId)
            .OrderByDescending(i => i.ValidFrom)
            .ToList()
            .FirstOrDefault(i => i!.ValidFrom <= yearMonth, null);
        if (result == null) throw new EntityNotFoundException();
        return result;
    }

    public BucketFigures GetFigures(Guid bucketId, DateTime yearMonth)
    {
        var bucketWithTransactions = _bucketRepository.ByIdWithTransactions(bucketId) ?? throw new Exception("Bucket not found.");
        var bucketWithMovements = _bucketRepository.ByIdWithMovements(bucketId) ?? throw new Exception("Bucket not found.");
        
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

    public decimal GetBalance(Guid bucketId, DateTime yearMonth)
    {
        var bucketWithTransactions = _bucketRepository.ByIdWithTransactions(bucketId) ?? throw new Exception("Bucket not found.");
        var bucketWithMovements = _bucketRepository.ByIdWithMovements(bucketId) ?? throw new Exception("Bucket not found.");
            
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

    public BucketFigures GetInAndOut(Guid bucketId, DateTime yearMonth)
    {
        var bucketWithTransactions = _bucketRepository.ByIdWithTransactions(bucketId) ?? throw new Exception("Bucket not found.");
        var bucketWithMovements = _bucketRepository.ByIdWithMovements(bucketId) ?? throw new Exception("Bucket not found.");
            
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

    public override Bucket Create(Bucket entity)
    {
        if (entity.CurrentVersion == null) throw new EntityUpdateException("No Bucket Version defined");

        entity.CurrentVersion.Version = 1;
        entity.BucketVersions = new List<BucketVersion>();
        entity.BucketVersions.Add(entity.CurrentVersion);
            
        _bucketRepository.Create(entity);
        return entity;
    }

    public override Bucket Update(Bucket entity)
    {
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

        _bucketRepository.Update(entity);
        return entity;
    }

    public void Close(Guid id, DateTime yearMonth)
    {
        if (GetBalance(id, yearMonth) != 0) throw new EntityUpdateException("Balance must be 0 to close a Bucket");
            
        if (_budgetedTransactionRepository.All().Any(i => i.BucketId == id) ||
            _bucketMovementRepository.All().Any(i => i.BucketId == id))
        {
            // Update: Bucket will be set to inactive for the next month
            var entity = _bucketRepository.ById(id);
            if (entity == null) throw new EntityUpdateException("Bucket not found");
            if (entity.IsInactive) throw new EntityUpdateException("Bucket has been already set to inactive");
            entity.IsInactive = true;
            entity.IsInactiveFrom = yearMonth.AddMonths(1);
            _bucketRepository.Update(entity);
        }
        else
        {
            // Delete: Bucket has no transactions & movements, so it can be directly deleted from the database
            _bucketRepository.Delete(id);
        }
    }

    public override void Delete(Guid id)
    {
        if (_budgetedTransactionRepository.All().Any(i => i.BucketId == id) ||
            _bucketMovementRepository.All().Any(i => i.BucketId == id))
        {
            throw new EntityUpdateException("Cannot delete a Bucket with assigned Transactions or Bucket Movements");
        }
            
        // Delete Bucket
        _bucketRepository.Delete(id);
                            
        // Delete all BucketRuleSet which refer to this Bucket
        var bucketRuleSetIds = _bucketRuleSetRepository
            .All()
            .Where(i => i.TargetBucketId == id)
            .Select(i => i.Id)
            .ToList();
        if (bucketRuleSetIds.Count != 0) _bucketRuleSetRepository.DeleteRange(bucketRuleSetIds);
    }

    public BucketMovement CreateMovement(Guid bucketId, decimal amount, DateTime movementDate)
    {
        var newBucketMovement = new BucketMovement()
        {
            Id = Guid.Empty,
            BucketId = bucketId,
            Amount = amount,
            MovementDate = movementDate
        };
        var result = _bucketMovementRepository.Create(newBucketMovement);
        if (result == 0) throw new EntityUpdateException($"Unable to create {typeof(BucketMovement)} in database");
        return newBucketMovement;
    }
}