using System.Linq.Expressions;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Repositories;

public interface IBaseRepository<T> where T : IEntity
{
    IQueryable<T> All(); 
    IQueryable<T> Where(Expression<Func<T, bool>> expression); 
    T? ById(Guid id);
    int Create(T entity); 
    int CreateRange(IEnumerable<T> entities);
    int Update(T entity); 
    int UpdateRange(IEnumerable<T> entities);
    int Delete(T entity);
    int DeleteRange(IEnumerable<T> entities);
}