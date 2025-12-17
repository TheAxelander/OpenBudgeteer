using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Repository.EFCore;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreAccountService : GenericAccountService<DatabaseContext>
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreAccountService> _logger;
    
    public EFCoreAccountService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreAccountService> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
    
    protected override DatabaseContext CreateDbConnection() => _dbContextFactory.CreateDbContext();
    protected override EFCoreAccountRepository CreateBaseRepository(DatabaseContext dbConnection) => new(dbConnection);
    protected override EFCoreBankTransactionRepository CreateBankTransactionRepository(DatabaseContext dbConnection) => new(dbConnection);
}
