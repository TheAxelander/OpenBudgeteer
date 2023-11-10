using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketRepository : BaseRepository<Bucket>, IBucketRepository
{
    public BucketRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }
    
    public override int Delete(Bucket entity)
    {
        if (entity.Id == Guid.Parse("00000000-0000-0000-0000-000000000001") ||
            entity.Id == Guid.Parse("00000000-0000-0000-0000-000000000002")) return 0;
        return base.Delete(entity);
    }

    public override int DeleteRange(IEnumerable<Bucket> entities)
    {
        var cleansedEntities = entities
            .Where(i => 
                i.Id != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                i.Id != Guid.Parse("00000000-0000-0000-0000-000000000002"))
            .ToList();
        
        return base.DeleteRange(cleansedEntities);
    }
}