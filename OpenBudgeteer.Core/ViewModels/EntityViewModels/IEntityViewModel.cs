using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public interface IEntityViewModel<TEntity> where TEntity : IEntity
{
    protected void ConvertFromDto(TEntity entity);
    protected TEntity ConvertToDto();
}