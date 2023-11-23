using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class BankTransactionService : BaseService<BankTransaction>, IBankTransactionService
{
    internal BankTransactionService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions, new BankTransactionRepository(new DatabaseContext(dbContextOptions)))
    {
    }
    
    public BankTransaction GetWithEntities(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BankTransactionRepository(dbContext);

            var result = repository.ByIdWithIncludedEntities(id);
            if (result == null) throw new Exception($"{typeof(BankTransaction)} not found in database");
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public IEnumerable<BankTransaction> GetAll(DateTime? periodStart, DateTime? periodEnd, int limit = 0)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BankTransactionRepository(dbContext);
            
            var result = repository
                .AllWithIncludedEntities()
                .Where(i =>
                    i.TransactionDate >= (periodStart ?? DateTime.MinValue) &&
                    i.TransactionDate <= (periodEnd ?? DateTime.MaxValue))
                .OrderByDescending(i => i.TransactionDate)
                .ToList();
            return limit > 0
                ? result.Take(limit)
                : result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, int limit = 0)
    {
        return GetFromAccount(accountId, null, null, limit);
    }
    
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, DateTime? periodStart, DateTime? periodEnd, int limit = 0)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BankTransactionRepository(dbContext);
            
            var result = repository
                .AllWithIncludedEntities()
                .Where(i =>
                    i.TransactionDate >= (periodStart ?? DateTime.MinValue) &&
                    i.TransactionDate <= (periodEnd ?? DateTime.MaxValue) &&
                    i.AccountId == accountId)
                .OrderByDescending(i => i.TransactionDate)
                .ToList();
            return limit > 0
                ? result.Take(limit)
                : result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var repository = new BankTransactionRepository(dbContext);
            
            var newTransactions = entities.ToList();
            repository.CreateRange(newTransactions);
            transaction.Commit();
            return newTransactions;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override BankTransaction Update(BankTransaction entity)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bankTransactionRepository = new BankTransactionRepository(dbContext);

            if (entity.BudgetedTransactions != null && entity.BudgetedTransactions.Any())
            {
                // Delete all existing bucket assignments, as they will be replaced by passed assignments
                var deletedIds =
                    budgetedTransactionRepository.All()
                        .Where(i => i.TransactionId == entity.Id)
                        .Select(i => i.Id)
                        .ToList();
                        
                if (deletedIds.Any())
                {
                    var result = budgetedTransactionRepository.DeleteRange(deletedIds);
                    if (result != deletedIds.Count) 
                        throw new Exception("Unable to delete old Bucket Assignments of that Transaction");
                }
                
                // Reset all Guid for re-creation
                foreach (var budgetedTransaction in entity.BudgetedTransactions)
                {
                    budgetedTransaction.Id = Guid.Empty;
                }
            }
            
            // Update BankTransaction including bucket assignments (if available) in DB
            bankTransactionRepository.Update(entity);
            
            transaction.Commit();
            return entity;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override void Delete(Guid id)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        try
        {
            var bankTransactionRepository = new BankTransactionRepository(dbContext);
            var result = bankTransactionRepository.Delete(id);
            if (result == 0) throw new Exception("Unable to delete Bank Transaction");
        }
        catch (Exception e)
        {
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}