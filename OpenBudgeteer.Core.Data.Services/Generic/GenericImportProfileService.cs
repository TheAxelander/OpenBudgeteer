using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericImportProfileService<TDatabase> : GenericBaseService<ImportProfile, TDatabase>, IImportProfileService
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;
    
    public GenericImportProfileService(ILogger logger) : base(logger)
    {
        _logger = logger;
    }

    public override ImportProfile Create(ImportProfile entity)
    {
        try
        {
            if (string.IsNullOrEmpty(entity.ProfileName)) throw new EntityUpdateException("Profile Name cannot be empty.");
            return base.Create(entity);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to create ImportProfile in database: {e.Message}", _logger);
        }
    }

    public override ImportProfile Update(ImportProfile entity)
    {
        try
        {
            if (string.IsNullOrEmpty(entity.ProfileName)) throw new EntityUpdateException("Profile Name cannot be empty.");
            return base.Update(entity);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to update ImportProfile in database: {e.Message}", _logger);
        }
    }
}