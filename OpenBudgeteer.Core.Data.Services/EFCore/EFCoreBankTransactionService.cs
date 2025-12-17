using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBankTransactionService : EFCoreBaseService<BankTransaction>, IBankTransactionService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBankTransactionService> _logger;

    public EFCoreBankTransactionService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBankTransactionService> logger) : base(dbContextFactory, logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
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
            using var dbContext = _dbContextFactory.CreateDbContext();
            var genericBankTransactionService = CreateBaseService(dbContext);
            return genericBankTransactionService.GetWithEntities(id);
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

    public IEnumerable<BankTransaction> GetAll(DateOnly? periodStart, DateOnly? periodEnd, int limit = 0)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var genericBankTransactionService = CreateBaseService(dbContext);
            return genericBankTransactionService.GetAll(periodStart, periodEnd, limit);
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

    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, int limit = 0)
    {
        return GetFromAccount(accountId, null, null, limit);
    }

    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, DateOnly? periodStart, DateOnly? periodEnd, int limit = 0)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var genericBankTransactionService = CreateBaseService(dbContext);
            return genericBankTransactionService.GetFromAccount(accountId, periodStart, periodEnd, limit);
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

    public IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction();
        var genericBankTransactionService = CreateBaseService(dbContext);
        try
        {
            var results = genericBankTransactionService.ImportTransactions(entities);
            transaction.Commit();
            return results;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to import Transactions: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}