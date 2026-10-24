using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericBaseService<TEntity, TDatabase> : IBaseService<TEntity>
    where TEntity : class, IEntity 
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;

    protected GenericBaseService(ILogger logger)
    {
        _logger = logger;
    }
    
    protected abstract TDatabase CreateDbConnection();
    protected abstract IBaseRepository<TEntity> CreateBaseRepository(TDatabase dbConnection);

    public virtual TEntity Get(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var result = baseRepository.ById(id);
            if (result is null) throw new EntityNotFoundException($"Unable to find {typeof(TEntity).Name} with the given id.");
            return result;
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public virtual IEnumerable<TEntity> GetAll()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            return baseRepository.All().ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public virtual TEntity Create(TEntity entity)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var result = baseRepository.Create(entity);
            if (result == 0) throw new EntityUpdateException($"{typeof(TEntity).Name} hasn't been created.");
            return entity;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to create {typeof(TEntity).Name} in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public virtual TEntity Update(TEntity entity)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var result = baseRepository.Update(entity);
            if (result == 0) throw new EntityUpdateException($"{typeof(TEntity).Name} hasn't been updated.");
            return entity;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to update {typeof(TEntity).Name} in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public virtual void Delete(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var baseRepository = CreateBaseRepository(dbConnection);
            var result = baseRepository.Delete(id);
            if (result == 0) throw new EntityUpdateException($"{typeof(TEntity).Name} hasn't been deleted.");
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to delete {typeof(TEntity).Name} in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}