using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Repository.EFCore;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreRecurringBankTransactionService : GenericRecurringBankTransactionService<DatabaseContext>
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreRecurringBankTransactionService> _logger;

    public EFCoreRecurringBankTransactionService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreRecurringBankTransactionService> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override DatabaseContext CreateDbConnection() => _dbContextFactory.CreateDbContext();
    protected override EFCoreRecurringBankTransactionRepository CreateBaseRepository(DatabaseContext dbConnection) => new (dbConnection);
    protected override EFCoreBankTransactionRepository CreateBankTransactionRepository(DatabaseContext dbConnection) => new (dbConnection);
}