using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BaseRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    public virtual IQueryable<T> All() => DatabaseContext.Set<T>().AsNoTracking();

    public virtual IQueryable<T> Where(Expression<Func<T, bool>> expression) 
        => DatabaseContext.Set<T>().Where(expression).AsNoTracking();

    public virtual T? ById(Guid id) => DatabaseContext.Set<T>().Find(id);

    public virtual int Create(T entity)
    {
        DatabaseContext.Set<T>().Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public virtual int CreateRange(IEnumerable<T> entities)
    {
        DatabaseContext.Set<T>().AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public virtual int Update(T entity)
    {
        DatabaseContext.Set<T>().Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public virtual int UpdateRange(IEnumerable<T> entities)
    {
        DatabaseContext.Set<T>().UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    } 

    public virtual int Delete(T entity)
    {
        DatabaseContext.Set<T>().Remove(entity);
        return DatabaseContext.SaveChanges();
    }


    public virtual int DeleteRange(IEnumerable<T> entities)
    {
        DatabaseContext.Set<T>().RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }

    protected DatabaseContext DatabaseContext { get; }

    public BaseRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
}