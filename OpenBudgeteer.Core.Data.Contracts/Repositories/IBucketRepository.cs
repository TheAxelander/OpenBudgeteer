using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Repositories;

public interface IBucketRepository : IBaseRepository<Bucket>
{
    public Bucket? ByIdWithVersions(Guid id);
    public Bucket? ByIdWithMovements(Guid id);
    public Bucket? ByIdWithTransactions(Guid id);
    public IEnumerable<Bucket> AllWithVersions();
    public IEnumerable<Bucket> AllWithActivities();
}