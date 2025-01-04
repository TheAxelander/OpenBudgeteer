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

public class ImportProfileDatabaseTest : BaseDatabaseTest<ImportProfile>
{
    private Account _testAccount1 = new();
    private Account _testAccount2 = new();
    
    protected override void CompareEntities(ImportProfile expected, ImportProfile actual)
    {
        Assert.Equal(expected.ProfileName, actual.ProfileName);
        Assert.Equal(expected.AccountId, actual.AccountId);
        Assert.Equal(expected.HeaderRow, actual.HeaderRow);
        Assert.Equal(expected.Delimiter, actual.Delimiter);
        Assert.Equal(expected.TextQualifier, actual.TextQualifier);
        Assert.Equal(expected.DateFormat, actual.DateFormat);
        Assert.Equal(expected.NumberFormat, actual.NumberFormat);
        Assert.Equal(expected.TransactionDateColumnName, actual.TransactionDateColumnName);
        Assert.Equal(expected.PayeeColumnName, actual.PayeeColumnName);
        Assert.Equal(expected.MemoColumnName, actual.MemoColumnName);
        Assert.Equal(expected.AmountColumnName, actual.AmountColumnName);
        Assert.Equal(expected.AdditionalSettingCreditValue, actual.AdditionalSettingCreditValue);
        Assert.Equal(expected.CreditColumnName, actual.CreditColumnName);
        Assert.Equal(expected.CreditColumnIdentifierColumnName, actual.CreditColumnIdentifierColumnName);
        Assert.Equal(expected.CreditColumnIdentifierValue, actual.CreditColumnIdentifierValue);
        Assert.Equal(expected.AdditionalSettingAmountCleanup, actual.AdditionalSettingAmountCleanup);
        Assert.Equal(expected.AdditionalSettingAmountCleanupValue, actual.AdditionalSettingAmountCleanupValue);
    }
    
    public static IEnumerable<object[]> TestData_Repository
    {
        get
        {
            var mockDb = new MockDatabase();
            return new[]
            {
                new object[]
                {
                    new MockImportProfileRepository(mockDb),
                    new MockAccountRepository(mockDb)
                },
                new object[]
                {
                    new ImportProfileRepository(new DatabaseContext(MariaDbContextOptions)),
                    new AccountRepository(new DatabaseContext(MariaDbContextOptions))
                }
            };
        }
    }
    
    private List<ImportProfile> SetupTestData(
        IImportProfileRepository importProfileRepository,
        IAccountRepository accountRepository)
    {
        DeleteAllExtension<IImportProfileRepository, ImportProfile>.DeleteAll(importProfileRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);

        _testAccount1 = TestDataGenerator.Current.GenerateAccount();
        _testAccount2 = TestDataGenerator.Current.GenerateAccount();
        accountRepository.Create(_testAccount1);
        accountRepository.Create(_testAccount2);

        var result = new List<ImportProfile>();
        for (var i = 1; i <= 4; i++)
        {
            var importProfile = i < 4
                ? TestDataGenerator.Current.GenerateImportProfile(_testAccount1)
                : TestDataGenerator.Current.GenerateImportProfile(_testAccount2);
            result.Add(importProfile);
            var repositoryResult = importProfileRepository.Create(importProfile);
            Assert.Equal(1, repositoryResult);
            Assert.NotEqual(Guid.Empty, importProfile.Id);
        }
        
        return result;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(
        IImportProfileRepository importProfileRepository,
        IAccountRepository accountRepository)
    {
        var importProfiles = SetupTestData(importProfileRepository, accountRepository);
        RunChecks(importProfileRepository, importProfiles);

        DeleteAllExtension<IImportProfileRepository, ImportProfile>.DeleteAll(importProfileRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(
        IImportProfileRepository importProfileRepository,
        IAccountRepository accountRepository)
    {
        var importProfiles = SetupTestData(importProfileRepository, accountRepository);
        var random = new Random();

        foreach (var importProfile in importProfiles)
        {
            var importProfileId = importProfile.Id;
            importProfile.ProfileName += "Update";
            importProfile.AccountId =
                importProfile.AccountId == _testAccount1.Id ? _testAccount2.Id : _testAccount1.Id;
            importProfile.HeaderRow += 1;
            importProfile.Delimiter = (char)random.Next(32, 127); // Generate a random printable ASCII character
            importProfile.TextQualifier = (char)random.Next(32, 127);
            importProfile.DateFormat += "Update";
            importProfile.NumberFormat += "Update";
            importProfile.TransactionDateColumnName += "Update";
            importProfile.PayeeColumnName += "Update";
            importProfile.MemoColumnName += "Update";
            importProfile.AmountColumnName += "Update";
            importProfile.AdditionalSettingCreditValue += 1;
            importProfile.CreditColumnName += "Update";
            importProfile.CreditColumnIdentifierColumnName += "Update";
            importProfile.CreditColumnIdentifierValue += "Update";
            importProfile.AdditionalSettingAmountCleanup = !importProfile.AdditionalSettingAmountCleanup;
            importProfile.AdditionalSettingAmountCleanupValue += "Update";
            
            var result = importProfileRepository.Update(importProfile);
            Assert.Equal(1, result);
            Assert.Equal(importProfileId, importProfile.Id); // Check if no new Guid has been generated (no CREATE)
        }
        
        RunChecks(importProfileRepository, importProfiles);

        DeleteAllExtension<IImportProfileRepository, ImportProfile>.DeleteAll(importProfileRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(
        IImportProfileRepository importProfileRepository,
        IAccountRepository accountRepository)
    {
        var importProfiles = SetupTestData(importProfileRepository, accountRepository);

        var deleteResult1 = importProfileRepository.Delete(importProfiles.First().Id);
        var deleteResult2 = importProfileRepository.Delete(importProfiles.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        importProfiles.Remove(importProfiles.First());
        importProfiles.Remove(importProfiles.Last());
        
        RunChecks(importProfileRepository, importProfiles);

        DeleteAllExtension<IImportProfileRepository, ImportProfile>.DeleteAll(importProfileRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
    }
}