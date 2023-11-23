using System.Linq.Expressions;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Repositories;

public interface IBaseRepository<T> where T : IEntity
{
    IQueryable<T> All(); 
    IQueryable<T> AllWithIncludedEntities();
    T? ById(Guid id);
    T? ByIdWithIncludedEntities(Guid id);
    int Create(T entity); 
    int CreateRange(IEnumerable<T> entities);
    int Update(T entity); 
    int UpdateRange(IEnumerable<T> entities);
    int Delete(Guid id);
    int DeleteRange(IEnumerable<Guid> ids);
}