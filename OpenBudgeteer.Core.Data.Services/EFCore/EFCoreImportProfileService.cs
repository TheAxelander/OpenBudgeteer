using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreImportProfileService : EFCoreBaseService<ImportProfile>, IImportProfileService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    public EFCoreImportProfileService(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected override GenericImportProfileService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericImportProfileService(new ImportProfileRepository(dbContext));
    }
}