using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class RecurringBankTransactionService : BaseService<RecurringBankTransaction>, IRecurringBankTransactionService
{
    internal RecurringBankTransactionService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions, new RecurringBankTransactionRepository(new DatabaseContext(dbContextOptions)))
    {
    }

    public async Task<IEnumerable<BankTransaction>> GetPendingBankTransactionAsync(DateTime yearMonth)
    {
        try
        {
            await using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new RecurringBankTransactionRepository(dbContext);
            
            var recurringBankTransactionTasks = new List<Task<List<BankTransaction>>>();
            var recurringBankTransactions = repository.AllWithIncludedEntities().ToList();
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public async Task<IEnumerable<BankTransaction>> CreatePendingBankTransactionAsync(DateTime yearMonth)
    {
        try
        {
            await using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BankTransactionRepository(dbContext);
            
            var transactions = (await GetPendingBankTransactionAsync(yearMonth)).ToList();
            if (transactions.Any(i => i.Account?.IsActive == 0))
                throw new Exception("Identified Transactions which would be assigned to an inactive Account");

            // Ensure Account is not assigned to prevent double creation of Accounts
            foreach (var transaction in transactions)
            {
                transaction.Account = new Account();
            }
            
            repository.CreateRange(transactions);
            await dbContext.SaveChangesAsync();
                
            return transactions;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}