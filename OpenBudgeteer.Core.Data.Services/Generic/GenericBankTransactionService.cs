using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericBankTransactionService<TDatabase> : GenericBaseService<BankTransaction, TDatabase>, IBankTransactionService
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;
    
    public GenericBankTransactionService(ILogger logger) : base(logger)
    {
        _logger = logger;
    }
    
    protected abstract override IBankTransactionRepository CreateBaseRepository(TDatabase dbConnection);
    protected abstract IBudgetedTransactionRepository CreateBudgetedTransactionRepository(TDatabase dbConnection);

    public virtual BankTransaction GetWithEntities(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var result = bankTransactionRepository.ByIdWithIncludedEntities(id);
            if (result is null) throw new EntityNotFoundException("Unable to find Bank Transaction with the given id.");
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
    
    public virtual IEnumerable<BankTransaction> GetAll(DateOnly? periodStart, DateOnly? periodEnd, int limit = 0)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var result = bankTransactionRepository
                .AllWithIncludedEntities()
                .Where(i =>
                    i.TransactionDate >= (periodStart ?? DateOnly.MinValue) &&
                    i.TransactionDate <= (periodEnd ?? DateOnly.MaxValue))
                .OrderByDescending(i => i.TransactionDate)
                .ToList();
            return limit > 0
                ? result.Take(limit)
                : result;
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
    
    public virtual IEnumerable<BankTransaction> GetFromAccount(Guid accountId, int limit = 0)
    {
        return GetFromAccount(accountId, null, null, limit);
    }
    
    public virtual IEnumerable<BankTransaction> GetFromAccount(Guid accountId, DateOnly? periodStart, DateOnly? periodEnd, int limit = 0)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var result = bankTransactionRepository
                .AllWithIncludedEntities()
                .Where(i =>
                    i.TransactionDate >= (periodStart ?? DateOnly.MinValue) &&
                    i.TransactionDate <= (periodEnd ?? DateOnly.MaxValue) &&
                    i.AccountId == accountId)
                .OrderByDescending(i => i.TransactionDate)
                .ToList();
            return limit > 0
                ? result.Take(limit)
                : result;
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
    
    public virtual IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var newTransactions = entities.ToList();
            bankTransactionRepository.CreateRange(newTransactions);
            return newTransactions;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to import Transactions: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override BankTransaction Update(BankTransaction entity)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var budgetedTransactionRepository = CreateBudgetedTransactionRepository(dbConnection);
            
            if (entity.BudgetedTransactions is not null && entity.BudgetedTransactions.Any())
            {
                // Delete all existing bucket assignments, as they will be replaced by passed assignments
                var deletedIds =
                    budgetedTransactionRepository.All()
                        .Where(i => i.TransactionId == entity.Id)
                        .Select(i => i.Id)
                        .ToList();
                            
                if (deletedIds.Count != 0)
                {
                    var result = budgetedTransactionRepository.DeleteRange(deletedIds);
                    if (result != deletedIds.Count) 
                        throw new EntityUpdateException("Unable to delete old Bucket Assignments of that Transaction");
                }
                    
                // Ensure that all BudgetedTransaction Guids of incoming entity are empty to enable their (re)creation
                foreach (var budgetedTransaction in entity.BudgetedTransactions)
                {
                    budgetedTransaction.Id = Guid.Empty;
                }
            }
                
            // Update BankTransaction including bucket assignments (if available) in DB
            bankTransactionRepository.Update(entity);
                
            return entity;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to update Bank Transaction: {e.Message}", _logger);
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
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var result = bankTransactionRepository.Delete(id);
            if (result == 0) throw new EntityUpdateException("Bank Transaction has not been deleted");
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to delete Bank Transaction: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}