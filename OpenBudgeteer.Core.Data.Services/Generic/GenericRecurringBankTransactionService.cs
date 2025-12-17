using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericRecurringBankTransactionService<TDatabase> : GenericBaseService<RecurringBankTransaction, TDatabase>, IRecurringBankTransactionService
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;
    
    public GenericRecurringBankTransactionService(ILogger logger) : base(logger)
    {
        _logger = logger;
    }
    
    protected abstract override IRecurringBankTransactionRepository CreateBaseRepository(TDatabase dbConnection);
    protected abstract IBankTransactionRepository CreateBankTransactionRepository(TDatabase dbConnection);

    public virtual RecurringBankTransaction GetWithEntities(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var result = baseRepository.ByIdWithIncludedEntities(id);
            if (result is null) throw new EntityNotFoundException("Unable to find Recurring Bank Transaction with the given id.");
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
    
    public virtual IEnumerable<RecurringBankTransaction> GetAllWithEntities()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository
                .AllWithIncludedEntities()
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

    public virtual async Task<IEnumerable<BankTransaction>> GetPendingBankTransactionAsync(DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            
            var recurringBankTransactionTasks = new List<Task<List<BankTransaction>>>();
            var recurringBankTransactions = baseRepository.AllWithIncludedEntities().ToList();
            foreach (var recurringBankTransaction in recurringBankTransactions)
            {
                recurringBankTransactionTasks.Add(Task.Run(() =>
                {
                    // Check if RecurringBankTransaction need to be created in current month
                
                    // Iterate until Occurrence Date is no longer in the past
                    var newOccurrenceDate = recurringBankTransaction.FirstOccurrenceDate;
                    while (newOccurrenceDate < yearMonth)
                    {
                        newOccurrenceDate = recurringBankTransaction.GetNextIterationDate(newOccurrenceDate);
                    }

                    // Check if Occurrence Date is in current month and if yes how often it may occur 
                    // Otherwise RecurringBankTransaction not relevant for current month
                    var transactionsToBeCreated = new List<BankTransaction>();
                    while (newOccurrenceDate.Month == yearMonth.Month &&
                           newOccurrenceDate.Year == yearMonth.Year)
                    {
                        // Collect new BankTransactions                                
                        transactionsToBeCreated.Add(recurringBankTransaction.GetAsBankTransaction(newOccurrenceDate));
                    
                        // Move to next iteration
                        newOccurrenceDate = recurringBankTransaction.GetNextIterationDate(newOccurrenceDate);
                    }

                    return transactionsToBeCreated;
                }));
            }

            List<BankTransaction> result = [];
            foreach (var taskResult in await Task.WhenAll(recurringBankTransactionTasks))
            {
                result.AddRange(taskResult);
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

    public virtual async Task<IEnumerable<BankTransaction>> CreatePendingBankTransactionAsync(DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bankTransactionRepository = CreateBankTransactionRepository(dbConnection);
            var transactions = (await GetPendingBankTransactionAsync(yearMonth)).ToList();
            if (transactions.Any(i => i.Account.IsActive == 0))
                throw new EntityUpdateException("Identified Transactions which would be assigned to an inactive Account");

            // Prevent creation of new accounts
            foreach (var transaction in transactions)
            {
                transaction.Account = null!;
            }

            bankTransactionRepository.CreateRange(transactions);
                
            return transactions;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to create RecurringBankTransaction in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}