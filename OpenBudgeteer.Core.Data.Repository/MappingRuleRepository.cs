using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class MappingRuleRepository : BaseRepository<MappingRule>, IMappingRuleRepository
{
    public MappingRuleRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }
}