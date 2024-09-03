using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericImportProfileService : GenericBaseService<ImportProfile>, IImportProfileService
{
    private readonly IImportProfileRepository _importProfileRepository;
    
    public GenericImportProfileService(
        IImportProfileRepository importProfileRepository) : base(importProfileRepository)
    {
        _importProfileRepository = importProfileRepository;
    }
}