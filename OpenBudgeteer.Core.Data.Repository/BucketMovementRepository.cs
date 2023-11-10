using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketMovementRepository : BaseRepository<BucketMovement>, IBucketMovementRepository
{
    public BucketMovementRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }
}