using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Repository.EFCore;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreImportProfileService : GenericImportProfileService<DatabaseContext>
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreImportProfileService> _logger;

    public EFCoreImportProfileService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreImportProfileService> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override DatabaseContext CreateDbConnection() => _dbContextFactory.CreateDbContext();
    protected override EFCoreImportProfileRepository CreateBaseRepository(DatabaseContext dbConnection) => new (dbConnection);
}