using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericImportProfileService : GenericBaseService<ImportProfile>, IImportProfileService
{
    private readonly IImportProfileRepository _importProfileRepository;
    
    public GenericImportProfileService(
        IImportProfileRepository importProfileRepository) : base(importProfileRepository)
    {
        _importProfileRepository = importProfileRepository;
    }

    public override ImportProfile Create(ImportProfile entity)
    {
        if (string.IsNullOrEmpty(entity.ProfileName)) throw new EntityUpdateException("Profile Name cannot be empty.");
        return base.Create(entity);
    }

    public override ImportProfile Update(ImportProfile entity)
    {
        if (string.IsNullOrEmpty(entity.ProfileName)) throw new EntityUpdateException("Profile Name cannot be empty.");
        return base.Update(entity);
    }
}