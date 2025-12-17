using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.EFCore;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBankTransactionService : GenericBankTransactionService<DatabaseContext>
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBankTransactionService> _logger;

    public EFCoreBankTransactionService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBankTransactionService> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
    
    protected override DatabaseContext CreateDbConnection() => _dbContextFactory.CreateDbContext();
    protected override EFCoreBankTransactionRepository CreateBaseRepository(DatabaseContext dbConnection) => new(dbConnection);
    protected override EFCoreBudgetedTransactionRepository CreateBudgetedTransactionRepository(DatabaseContext dbConnection) => new(dbConnection);

    public override IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities)
    {
        using var dbConnection = CreateDbConnection();
        using var transaction = dbConnection.Database.BeginTransaction();
        try
        {
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var newTransactions = entities.ToList();
            bankTransactionRepository.CreateRange(newTransactions);
            return newTransactions;
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