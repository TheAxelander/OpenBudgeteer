using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBankTransactionService : EFCoreBaseService<BankTransaction>, IBankTransactionService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    public EFCoreBankTransactionService(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected override GenericBankTransactionService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBankTransactionService(
            new BankTransactionRepository(dbContext),
            new BudgetedTransactionRepository(dbContext));
    }

    public BankTransaction GetWithEntities(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var genericBankTransactionService = CreateBaseService(dbContext);
            return genericBankTransactionService.GetWithEntities(id);
        }
        catch (EntityNotFoundException e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: Bank Transaction not found in database");
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var genericBankTransactionService = CreateBaseService(dbContext);
            return genericBankTransactionService.GetAll(periodStart, periodEnd, limit);
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var genericBankTransactionService = CreateBaseService(dbContext);
            return genericBankTransactionService.GetFromAccount(accountId, periodStart, periodEnd, limit);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities)
    {
        using var dbContext = new DatabaseContext(_dbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        var genericBankTransactionService = CreateBaseService(dbContext);
        try
        {
            var results = genericBankTransactionService.ImportTransactions(entities);
            transaction.Commit();
            return results;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}