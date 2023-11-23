using System;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public abstract class BaseEntityViewModel<TEntity> : ViewModelBase where TEntity : IEntity
{
    protected BaseEntityViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
    }
    
    internal abstract TEntity ConvertToDto();
}