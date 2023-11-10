using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketVersionRepository : BaseRepository<BucketVersion>, IBucketVersionRepository
{
    public BucketVersionRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }
}