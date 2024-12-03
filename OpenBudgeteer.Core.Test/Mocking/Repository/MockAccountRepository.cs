using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockAccountRepository : IAccountRepository
{
    private readonly MockDatabase _mockDatabase;
    
    public MockAccountRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }
    
    public IQueryable<Account> All()
    {
        return _mockDatabase.Accounts.Values.AsQueryable();
    }

    public IQueryable<Account> AllWithIncludedEntities()
    {
        return All();
    }

    public Account? ById(Guid id)
    {
        _mockDatabase.Accounts.TryGetValue(id, out var result);
        return result;
    }

    public Account? ByIdWithIncludedEntities(Guid id)
    {
        return ById(id);
    }

    public int Create(Account entity)
    {
        entity.Id = Guid.NewGuid();
        _mockDatabase.Accounts[entity.Id] = entity;
        return 1;
    }

    public int CreateRange(IEnumerable<Account> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(Account entity)
    {
        try
        {
            _mockDatabase.Accounts[entity.Id] = entity;
            return 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int UpdateRange(IEnumerable<Account> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        try
        {
            return _mockDatabase.Accounts.Remove(id) ? 1 : 0;
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