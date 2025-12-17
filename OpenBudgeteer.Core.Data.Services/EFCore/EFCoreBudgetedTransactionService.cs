using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Repository.EFCore;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBudgetedTransactionService : GenericBudgetedTransactionService<DatabaseContext>
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBudgetedTransactionService> _logger;

    public EFCoreBudgetedTransactionService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBudgetedTransactionService> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override DatabaseContext CreateDbConnection() => _dbContextFactory.CreateDbContext();
    protected override EFCoreBudgetedTransactionRepository CreateBaseRepository(DatabaseContext dbConnection) => new (dbConnection);
}