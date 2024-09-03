using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public abstract class EFCoreBaseService<TEntity> : IBaseService<TEntity> 
    where TEntity : class, IEntity
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    protected EFCoreBaseService(DbContextOptions<DatabaseContext> dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected abstract IBaseService<TEntity> CreateBaseService(DatabaseContext dbContext);
    
    public virtual TEntity Get(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.Get(id);
        }
        catch (EntityNotFoundException e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {typeof(TEntity)} not found in database");
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAll();
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.Create(entity);
        }
        catch (EntityUpdateException e)
        {
            Console.WriteLine(e);
            throw new Exception($"Unable to create {typeof(TEntity)} in database");
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.Update(entity);
        }
        catch (EntityUpdateException e)
        {
            Console.WriteLine(e);
            throw new Exception($"Unable to update {typeof(TEntity)} in database");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public virtual void Delete(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            baseService.Delete(id);
        }
        catch (EntityUpdateException e)
        {
            Console.WriteLine(e);
            throw new Exception($"Unable to delete {typeof(TEntity)} in database");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}