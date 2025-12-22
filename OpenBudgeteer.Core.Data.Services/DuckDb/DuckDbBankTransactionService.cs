using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbBankTransactionService : GenericBankTransactionService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBankTransactionService> _logger;

    public DuckDbBankTransactionService(
        Func<DbConnection> dbConnectionFactory,
        ILogger<DuckDbBankTransactionService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBankTransactionRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbBankTransactionRepository(dbConnection);
    protected override IBudgetedTransactionRepository CreateBudgetedTransactionRepository(DbConnection dbConnection) => new DuckDbBudgetedTransactionRepository(dbConnection);

    public override IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities)
    {
        using var dbConnection = CreateDbConnection();
        using var transaction = dbConnection.BeginTransaction();
        try
        {
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var newTransactions = entities.ToList();
            bankTransactionRepository.CreateRange(newTransactions);
            transaction.Commit();
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
