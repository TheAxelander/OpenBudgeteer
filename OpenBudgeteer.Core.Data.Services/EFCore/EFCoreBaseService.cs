using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public abstract class EFCoreBaseService<TEntity> : IBaseService<TEntity> 
    where TEntity : class, IEntity
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger _logger;

    protected EFCoreBaseService(IDbContextFactory<DatabaseContext> dbContextFactory, ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected abstract IBaseService<TEntity> CreateBaseService(DatabaseContext dbContext);
    
    public virtual TEntity Get(Guid id)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.Get(id);
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
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAll();
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
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.Create(entity);
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
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.Update(entity);
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
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            baseService.Delete(id);
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