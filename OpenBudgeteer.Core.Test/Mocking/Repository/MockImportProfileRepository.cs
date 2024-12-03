using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockImportProfileRepository : IImportProfileRepository
{
    private readonly MockDatabase _mockDatabase;

    public MockImportProfileRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }

    public IQueryable<ImportProfile> All()
    {
        return _mockDatabase.ImportProfiles.Values.AsQueryable();
    }

    public IQueryable<ImportProfile> AllWithIncludedEntities()
    {
        var mockAccountRepository = new MockAccountRepository(_mockDatabase);
        var importProfiles = All().ToList();
        foreach (var importProfile in importProfiles)
        {
            importProfile.Account = mockAccountRepository.ById(importProfile.AccountId) 
                                         ?? throw new Exception("Account doesn't exist");
        }

        return importProfiles.AsQueryable();
    }

    public ImportProfile? ById(Guid id)
    {
        _mockDatabase.ImportProfiles.TryGetValue(id, out var result);
        return result;
    }

    public ImportProfile? ByIdWithIncludedEntities(Guid id)
    {
        var mockAccountRepository = new MockAccountRepository(_mockDatabase);
        var importProfile = ById(id);
        if (importProfile == null) return importProfile;
        importProfile.Account = mockAccountRepository.ById(importProfile.AccountId) 
                                ?? throw new Exception("Account doesn't exist");

        return importProfile;
    }

    public int Create(ImportProfile entity)
    {
        entity.Id = Guid.NewGuid();
        _mockDatabase.ImportProfiles[entity.Id] = entity;
        return 1;
    }

    public int CreateRange(IEnumerable<ImportProfile> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(ImportProfile entity)
    {
        try
        {
            _mockDatabase.ImportProfiles[entity.Id] = entity;
            return 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int UpdateRange(IEnumerable<ImportProfile> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        try
        {
            return _mockDatabase.ImportProfiles.Remove(id) ? 1 : 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        return ids.Sum(Delete);
    }
}