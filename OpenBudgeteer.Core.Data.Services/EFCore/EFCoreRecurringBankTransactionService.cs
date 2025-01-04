using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreRecurringBankTransactionService : EFCoreBaseService<RecurringBankTransaction>, IRecurringBankTransactionService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    public EFCoreRecurringBankTransactionService(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected override GenericRecurringBankTransactionService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericRecurringBankTransactionService(
            new RecurringBankTransactionRepository(dbContext),
            new BankTransactionRepository(dbContext));
    }

    public RecurringBankTransaction GetWithEntities(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetWithEntities(id);
        }
        catch (EntityNotFoundException e)
        {
            Console.WriteLine(e);
            throw new Exception($"{typeof(RecurringBankTransaction)} not found in database");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public IEnumerable<RecurringBankTransaction> GetAllWithEntities()
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllWithEntities();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public async Task<IEnumerable<BankTransaction>> GetPendingBankTransactionAsync(DateTime yearMonth)
    {
        try
        {
            await using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return await baseService.GetPendingBankTransactionAsync(yearMonth);
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
            await using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return await baseService.CreatePendingBankTransactionAsync(yearMonth);
        }
        catch (EntityUpdateException e)
        {
            Console.WriteLine(e);
            throw new Exception(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}