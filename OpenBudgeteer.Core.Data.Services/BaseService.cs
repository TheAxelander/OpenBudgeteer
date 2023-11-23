using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Services;

internal abstract class BaseService<TEntity> : IBaseService<TEntity> 
    where TEntity : class, IEntity
{
    protected readonly DbContextOptions<DatabaseContext> DbContextOptions;
    private readonly IBaseRepository<TEntity> _baseRepository;
    
    protected BaseService(DbContextOptions<DatabaseContext> dbContextOptions, IBaseRepository<TEntity> baseRepository)
    {
        DbContextOptions = dbContextOptions;
        _baseRepository = baseRepository;
    }
   
    public virtual TEntity Get(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            
            var result = _baseRepository.ById(id);
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
            
            return _baseRepository.All().ToList();
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
            
            var result = _baseRepository.Create(entity);
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
            
            var result = _baseRepository.Update(entity);
            if (result == 0) throw new Exception($"Unable to update {typeof(TEntity)} in database");
            return entity;
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
            using var dbContext = new DatabaseContext(DbContextOptions);

            var result = _baseRepository.Delete(id);
            if (result == 0) throw new Exception($"Unable to delete {typeof(TEntity)} in database");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}