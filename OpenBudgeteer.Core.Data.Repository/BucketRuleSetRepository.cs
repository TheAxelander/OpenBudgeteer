using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketRuleSetRepository : BaseRepository<BucketRuleSet>, IBucketRuleSetRepository
{
    public BucketRuleSetRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }

    public override IQueryable<BucketRuleSet> All() => DatabaseContext
        .Set<BucketRuleSet>()
        .Include(i => i.TargetBucket)
        .AsNoTracking();

    public override IQueryable<BucketRuleSet> Where(Expression<Func<BucketRuleSet, bool>> expression) 
        => DatabaseContext
            .Set<BucketRuleSet>()
            .Include(i => i.TargetBucket)
            .Where(expression)
            .AsNoTracking();
    
    public override BucketRuleSet? ById(Guid id) => DatabaseContext
        .Set<BucketRuleSet>()
        .Include(i => i.TargetBucket)
        .FirstOrDefault(i => i.Id == id);
}