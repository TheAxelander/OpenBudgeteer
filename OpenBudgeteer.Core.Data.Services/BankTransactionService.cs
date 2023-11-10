using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class BankTransactionService : BaseService<BankTransaction>, IBankTransactionService
{
    internal BankTransactionService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions)
    {
    }
    
    public IEnumerable<BankTransaction> GetAll(DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BankTransactionRepository(dbContext);
            
            return repository
                .Where(i =>
                    i.TransactionDate >= periodStart &&
                    i.TransactionDate <= periodEnd)
                .OrderByDescending(i => i.TransactionDate)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, int limit = 0)
    {
        return GetFromAccount(accountId, DateTime.MinValue, DateTime.MaxValue, limit);
    }
    
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, DateTime periodStart, DateTime periodEnd, int limit = 0)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BankTransactionRepository(dbContext);
            
            var result = repository
                .Where(i =>
                    i.TransactionDate >= periodStart &&
                    i.TransactionDate <= periodEnd &&
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
    
    public BankTransaction Create(BankTransaction entity, IEnumerable<BudgetedTransaction> budgetedTransactions)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bankTransactionRepository = new BankTransactionRepository(dbContext);
            
            // Create BankTransaction in DB
            bankTransactionRepository.Create(entity);
            
            // Create new bucket assignments
            var newBudgetedTransactions = budgetedTransactions.ToList();
            var result = budgetedTransactionRepository.CreateRange(newBudgetedTransactions);
            if (result != newBudgetedTransactions.Count) 
                throw new Exception("Unable to create new Bucket Assignments for that Transaction");
            
            transaction.Commit();
            return entity;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public BankTransaction Update(BankTransaction entity, IEnumerable<BudgetedTransaction> budgetedTransactions)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bankTransactionRepository = new BankTransactionRepository(dbContext);
            
            // Update BankTransaction in DB
            bankTransactionRepository.Update(entity);
            
            // Delete all previous bucket assignments for transaction
            var oldBudgetedTransactions = budgetedTransactionRepository
                .Where(i => i.TransactionId == entity.Id)
                .ToList();
            var result = budgetedTransactionRepository.DeleteRange(oldBudgetedTransactions);
            if (result != oldBudgetedTransactions.Count) 
                throw new Exception("Unable to delete old Bucket Assignments of that Transaction");
            
            // Create new bucket assignments
            var newBudgetedTransactions = budgetedTransactions.ToList();
            result = budgetedTransactionRepository.CreateRange(newBudgetedTransactions);
            if (result != newBudgetedTransactions.Count) 
                throw new Exception("Unable to create new Bucket Assignments for that Transaction");
            
            transaction.Commit();
            return entity;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override BankTransaction Delete(BankTransaction entity)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var budgetedTransactionRepository = new BudgetedTransactionRepository(dbContext);
            var bankTransactionRepository = new BankTransactionRepository(dbContext);
            
            // Delete all previous bucket assignments for transaction
            var budgetedTransactions = budgetedTransactionRepository
                .Where(i => i.TransactionId == entity.Id)
                .ToList();
            var result = budgetedTransactionRepository.DeleteRange(budgetedTransactions);
            if (result != budgetedTransactions.Count) 
                throw new Exception("Unable to delete all Bucket Assignments of that Transaction");
            
            // Delete BankTransaction in DB
            result = bankTransactionRepository.Delete(entity);
            if (result == 0) throw new Exception("Unable to delete Bank Transaction");
            
            transaction.Commit();
            return entity;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}