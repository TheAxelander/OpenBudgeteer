using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketGroupRepository : BaseRepository<BucketGroup>, IBucketGroupRepository
{
    public BucketGroupRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }

    public override int Delete(BucketGroup entity)
    {
        if (entity.Id == Guid.Parse("00000000-0000-0000-0000-000000000001")) return 0;
        return base.Delete(entity);
    }

    public override int DeleteRange(IEnumerable<BucketGroup> entities)
    {
        var cleansedEntities = entities
            .Where(i => i.Id != Guid.Parse("00000000-0000-0000-0000-000000000001"))
            .ToList();
        
        return base.DeleteRange(cleansedEntities);
    }
}