using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Test.Common;
using OpenBudgeteer.Core.Test.Mocking;
using OpenBudgeteer.Core.Test.Mocking.Repository;
using Xunit;

namespace OpenBudgeteer.Core.Test.Tests.Database;

public class AccountDatabaseTest : BaseDatabaseTest<Account>
{
    protected override void CompareEntities(Account expected, Account actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.IsActive, actual.IsActive);
    }
    
    public static IEnumerable<object[]> TestData_Repository
    {
        get
        {
            return new[]
            {
                new object[] { new MockAccountRepository(new MockDatabase()) },
                new object[] { new AccountRepository(new DatabaseContext(MariaDbContextOptions)) }
            };
        }
    }

    private List<Account> SetupTestData(IAccountRepository accountRepository)
    {
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
        
        var result = new List<Account>();
        for (var i = 1; i <= 4; i++)
        {
            var account = TestDataGenerator.Current.GenerateAccount(); 
            result.Add(account);
            var repositoryResult = accountRepository.Create(account);
            Assert.Equal(1, repositoryResult);
            Assert.NotEqual(Guid.Empty, account.Id);
        }
        
        return result;
    }

    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(IAccountRepository baseRepository)
    {
        var accounts = SetupTestData(baseRepository);
        RunChecks(baseRepository, accounts);
    
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(baseRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(IAccountRepository baseRepository)
    {
        var accounts = SetupTestData(baseRepository);

        foreach (var account in accounts)
        {
            var accountId = account.Id;
            account.Name += "Update";
            account.IsActive = 0;
            var result = baseRepository.Update(account);
            Assert.Equal(1, result);
            Assert.Equal(accountId, account.Id); // Check if no new Guid has been generated (no CREATE)
        }

        RunChecks(baseRepository, accounts);
        
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(baseRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(IAccountRepository baseRepository)
    {
        var accounts = SetupTestData(baseRepository);

        var deleteResult1 = baseRepository.Delete(accounts.First().Id);
        var deleteResult2 = baseRepository.Delete(accounts.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        accounts.Remove(accounts.First());
        accounts.Remove(accounts.Last());
        
        RunChecks(baseRepository, accounts);
        
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(baseRepository);
    }
}