using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Common;

public static class DeleteAllExtension<TRepository, TEntity> 
    where TRepository : IBaseRepository<TEntity>
    where TEntity : IEntity
{
    public static void DeleteAll(TRepository repository)
    {
        foreach (var entity in repository.All().ToList())
        {
            repository.Delete(entity.Id);
        }
    }
}