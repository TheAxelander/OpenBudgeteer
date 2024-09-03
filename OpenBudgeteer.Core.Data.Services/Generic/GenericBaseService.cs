using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericBaseService<TEntity> : IBaseService<TEntity> 
    where TEntity : class, IEntity
{
    private readonly IBaseRepository<TEntity> _baseRepository;

    protected GenericBaseService(IBaseRepository<TEntity> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public virtual TEntity Get(Guid id)
    {
        var result = _baseRepository.ById(id);
        if (result == null) throw new EntityNotFoundException();
        return result;
    }

    public virtual IEnumerable<TEntity> GetAll()
    {
        return _baseRepository.All().ToList();
    }

    public virtual TEntity Create(TEntity entity)
    {
        var result = _baseRepository.Create(entity);
        if (result == 0) throw new EntityUpdateException();
        return entity;
    }

    public virtual TEntity Update(TEntity entity)
    {
        var result = _baseRepository.Update(entity);
        if (result == 0) throw new EntityUpdateException();
        return entity;
    }

    public virtual void Delete(Guid id)
    {
        var result = _baseRepository.Delete(id);
        if (result == 0) throw new EntityUpdateException();
    }
}