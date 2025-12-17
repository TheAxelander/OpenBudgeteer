using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreRecurringBankTransactionService : EFCoreBaseService<RecurringBankTransaction>, IRecurringBankTransactionService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreRecurringBankTransactionService> _logger;

    public EFCoreRecurringBankTransactionService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreRecurringBankTransactionService> logger) : base(dbContextFactory, logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
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
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetWithEntities(id);
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
    
    public IEnumerable<RecurringBankTransaction> GetAllWithEntities()
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllWithEntities();
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

    public async Task<IEnumerable<BankTransaction>> GetPendingBankTransactionAsync(DateOnly yearMonth)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var baseService = CreateBaseService(dbContext);
            return await baseService.GetPendingBankTransactionAsync(yearMonth);
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

    public async Task<IEnumerable<BankTransaction>> CreatePendingBankTransactionAsync(DateOnly yearMonth)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var baseService = CreateBaseService(dbContext);
            return await baseService.CreatePendingBankTransactionAsync(yearMonth);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to create Bank Transaction: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}