using System;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public abstract class BaseEntityViewModel<TEntity> : ViewModelBase, ICloneable where TEntity : IEntity
{
    protected BaseEntityViewModel(IServiceManager serviceManager, ILogger logger) : base(serviceManager, logger)
    {
    }
    
    internal abstract TEntity ConvertToDto();
    public abstract object Clone();
}