using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericBankTransactionService : GenericBaseService<BankTransaction>, IBankTransactionService
{
    private readonly IBankTransactionRepository _bankTransactionRepository;
    private readonly IBudgetedTransactionRepository _budgetedTransactionRepository;
    
    public GenericBankTransactionService(
        IBankTransactionRepository bankTransactionRepository, 
        IBudgetedTransactionRepository budgetedTransactionRepository) : base(bankTransactionRepository)
    {
        _bankTransactionRepository = bankTransactionRepository;
        _budgetedTransactionRepository = budgetedTransactionRepository;
    }

    public BankTransaction GetWithEntities(Guid id)
    {
        var result = _bankTransactionRepository.ByIdWithIncludedEntities(id);
        if (result == null) throw new EntityNotFoundException();
        return result;
    }
    
    public IEnumerable<BankTransaction> GetAll(DateTime? periodStart, DateTime? periodEnd, int limit = 0)
    {
        var result = _bankTransactionRepository
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
    
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, int limit = 0)
    {
        return GetFromAccount(accountId, null, null, limit);
    }
    
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, DateTime? periodStart, DateTime? periodEnd, int limit = 0)
    {
        var result = _bankTransactionRepository
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
    
    public IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities)
    {
        var newTransactions = entities.ToList();
        _bankTransactionRepository.CreateRange(newTransactions);
        return newTransactions;
    }

    public override BankTransaction Update(BankTransaction entity)
    {
        if (entity.BudgetedTransactions != null && entity.BudgetedTransactions.Any())
        {
            // Delete all existing bucket assignments, as they will be replaced by passed assignments
            var deletedIds =
                _budgetedTransactionRepository.All()
                    .Where(i => i.TransactionId == entity.Id)
                    .Select(i => i.Id)
                    .ToList();
                        
            if (deletedIds.Count != 0)
            {
                var result = _budgetedTransactionRepository.DeleteRange(deletedIds);
                if (result != deletedIds.Count) 
                    throw new EntityUpdateException("Unable to delete old Bucket Assignments of that Transaction");
            }
                
            // Reset all Guid for re-creation
            foreach (var budgetedTransaction in entity.BudgetedTransactions)
            {
                budgetedTransaction.Id = Guid.Empty;
            }
        }
            
        // Update BankTransaction including bucket assignments (if available) in DB
        _bankTransactionRepository.Update(entity);
            
        return entity;
    }

    public override void Delete(Guid id)
    {
        var result = _bankTransactionRepository.Delete(id);
        if (result == 0) throw new EntityUpdateException("Unable to delete Bank Transaction");
    }
}