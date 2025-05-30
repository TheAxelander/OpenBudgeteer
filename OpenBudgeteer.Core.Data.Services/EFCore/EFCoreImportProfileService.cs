using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreImportProfileService : EFCoreBaseService<ImportProfile>, IImportProfileService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreImportProfileService> _logger;

    public EFCoreImportProfileService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreImportProfileService> logger) : base(dbContextFactory, logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override GenericImportProfileService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericImportProfileService(new ImportProfileRepository(dbContext));
    }
}