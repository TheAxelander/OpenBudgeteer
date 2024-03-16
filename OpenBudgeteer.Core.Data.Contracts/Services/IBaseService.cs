using System.Linq.Expressions;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBaseService<T> where T : IEntity
{
    public T Get(Guid id);
    public IEnumerable<T> GetAll();
    public T Create(T entity);
    public T Update(T entity);
    public void Delete(Guid id);
}