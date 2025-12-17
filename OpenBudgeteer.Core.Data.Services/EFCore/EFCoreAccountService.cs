using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreAccountService : EFCoreBaseService<Account>, IAccountService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreAccountService> _logger;

    public EFCoreAccountService(IDbContextFactory<DatabaseContext> dbContextFactory, ILogger<EFCoreAccountService> logger) : base(dbContextFactory, logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override GenericAccountService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericAccountService(
            new AccountRepository(dbContext),
            new BankTransactionRepository(dbContext));
    }
    
    public IEnumerable<Account> GetActiveAccounts()
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var genericAccountService = CreateBaseService(dbContext);
            return genericAccountService.GetActiveAccounts();
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

    public Account CloseAccount(Guid id)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var genericAccountService = CreateBaseService(dbContext);
            return genericAccountService.CloseAccount(id);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to close Account: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}