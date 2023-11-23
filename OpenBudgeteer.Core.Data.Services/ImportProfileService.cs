using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class ImportProfileService : BaseService<ImportProfile>, IImportProfileService
{
    internal ImportProfileService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions, new ImportProfileRepository(new DatabaseContext(dbContextOptions)))
    {
    }
}