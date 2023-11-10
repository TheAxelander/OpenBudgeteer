using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class ImportProfileRepository : BaseRepository<ImportProfile>, IImportProfileRepository
{
    public ImportProfileRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }
    
    public override IQueryable<ImportProfile> All() => DatabaseContext
        .Set<ImportProfile>()
        .Include(i => i.Account)
        .AsNoTracking();

    public override IQueryable<ImportProfile> Where(Expression<Func<ImportProfile, bool>> expression) 
        => DatabaseContext
            .Set<ImportProfile>()
            .Include(i => i.Account)
            .Where(expression)
            .AsNoTracking();
    
    public override ImportProfile? ById(Guid id) => DatabaseContext
        .Set<ImportProfile>()
        .Include(i => i.Account)
        .FirstOrDefault(i => i.Id == id);
}