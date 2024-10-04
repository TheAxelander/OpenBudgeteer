using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericRecurringBankTransactionService : GenericBaseService<RecurringBankTransaction>, IRecurringBankTransactionService
{
    private readonly IRecurringBankTransactionRepository _recurringBankTransactionRepository;
    private readonly IBankTransactionRepository _bankTransactionRepository;
    
    public GenericRecurringBankTransactionService(
        IRecurringBankTransactionRepository recurringBankTransactionRepository, 
        IBankTransactionRepository bankTransactionRepository) : base(recurringBankTransactionRepository)
    {
        _recurringBankTransactionRepository = recurringBankTransactionRepository;
        _bankTransactionRepository = bankTransactionRepository;
    }

    public RecurringBankTransaction GetWithEntities(Guid id)
    {
        var result = _recurringBankTransactionRepository.ByIdWithIncludedEntities(id);
        if (result == null) throw new EntityNotFoundException();
        return result;
    }
    
    public IEnumerable<RecurringBankTransaction> GetAllWithEntities()
    {
        return _recurringBankTransactionRepository
            .AllWithIncludedEntities()
            .ToList();
    }

    public async Task<IEnumerable<BankTransaction>> GetPendingBankTransactionAsync(DateTime yearMonth)
    {
        var recurringBankTransactionTasks = new List<Task<List<BankTransaction>>>();
        var recurringBankTransactions = _recurringBankTransactionRepository.AllWithIncludedEntities().ToList();
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

    public async Task<IEnumerable<BankTransaction>> CreatePendingBankTransactionAsync(DateTime yearMonth)
    {
        var transactions = (await GetPendingBankTransactionAsync(yearMonth)).ToList();
        if (transactions.Any(i => i.Account?.IsActive == 0))
            throw new EntityUpdateException("Identified Transactions which would be assigned to an inactive Account");

        // Prevent creation of new accounts
        foreach (var transaction in transactions)
        {
            transaction.Account = null!;
        }

        _bankTransactionRepository.CreateRange(transactions);
                
        return transactions;
    }
}