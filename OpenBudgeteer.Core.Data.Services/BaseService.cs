using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal abstract class BaseService<TEntity> : IBaseService<TEntity> 
    where TEntity : class, IEntity
{
    protected readonly DbContextOptions<DatabaseContext> DbContextOptions;
    
    protected BaseService(DbContextOptions<DatabaseContext> dbContextOptions)
    {
        DbContextOptions = dbContextOptions;
    }
   
    public virtual TEntity Get(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BaseRepository<TEntity>(dbContext);
            
            var result = repository.ById(id);
            if (result == null) throw new Exception($"{typeof(TEntity)} not found in database");
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public virtual IEnumerable<TEntity> GetAll()
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BaseRepository<TEntity>(dbContext);
            
            return repository.All().ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public virtual IEnumerable<TEntity> GetWith(Expression<Func<TEntity, bool>> expression)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BaseRepository<TEntity>(dbContext);
            
            return repository.Where(expression).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public virtual TEntity Create(TEntity entity)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BaseRepository<TEntity>(dbContext);
            
            var result = repository.Create(entity);
            if (result == 0) throw new Exception($"Unable to create {typeof(TEntity)} in database");
            return entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public virtual TEntity Update(TEntity entity)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BaseRepository<TEntity>(dbContext);
            
            var result = repository.Update(entity);
            if (result == 0) throw new Exception($"Unable to update {typeof(TEntity)} in database");
            return entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public virtual TEntity Delete(TEntity entity)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BaseRepository<TEntity>(dbContext);
            
            var result = repository.Delete(entity);
            if (result == 0) throw new Exception($"Unable to delete {typeof(TEntity)} in database");
            return entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}