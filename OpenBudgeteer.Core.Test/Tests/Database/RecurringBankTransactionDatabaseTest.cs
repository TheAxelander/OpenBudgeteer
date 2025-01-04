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

public class RecurringBankTransactionDatabaseTest : BaseDatabaseTest<RecurringBankTransaction>
{
    private Account _testAccount1 = new();
    private Account _testAccount2 = new();
    
    protected override void CompareEntities(RecurringBankTransaction expected, RecurringBankTransaction actual)
    {
        Assert.Equal(expected.AccountId, actual.AccountId);
        Assert.Equal(expected.RecurrenceType, actual.RecurrenceType);
        Assert.Equal(expected.RecurrenceAmount, actual.RecurrenceAmount);
        Assert.Equal(expected.FirstOccurrenceDate, actual.FirstOccurrenceDate);
        Assert.Equal(expected.Payee, actual.Payee);
        Assert.Equal(expected.Memo, actual.Memo);
        Assert.Equal(expected.Amount, actual.Amount);
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
                    new MockRecurringBankTransactionRepository(mockDb),
                    new MockAccountRepository(mockDb)
                },
                new object[]
                {
                    new RecurringBankTransactionRepository(new DatabaseContext(MariaDbContextOptions)),
                    new AccountRepository(new DatabaseContext(MariaDbContextOptions))
                }
            };
        }
    }
    
    private List<RecurringBankTransaction> SetupTestData(
        IRecurringBankTransactionRepository recurringBankTransactionRepository,
        IAccountRepository accountRepository)
    {
        DeleteAllExtension<IRecurringBankTransactionRepository, RecurringBankTransaction>.DeleteAll(recurringBankTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);

        _testAccount1 = TestDataGenerator.Current.GenerateAccount();
        _testAccount2 = TestDataGenerator.Current.GenerateAccount();
        accountRepository.Create(_testAccount1);
        accountRepository.Create(_testAccount2);
        
        var result = new List<RecurringBankTransaction>();
        for (var i = 1; i <= 4; i++)
        {
            var bucketMovement = i < 4
                ? TestDataGenerator.Current.GenerateRecurringBankTransaction(_testAccount1)
                : TestDataGenerator.Current.GenerateRecurringBankTransaction(_testAccount2);
            result.Add(bucketMovement);
            var repositoryResult = recurringBankTransactionRepository.Create(bucketMovement);
            Assert.Equal(1, repositoryResult);
            Assert.NotEqual(Guid.Empty, bucketMovement.Id);
        }
        
        return result;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(
        IRecurringBankTransactionRepository recurringBankTransactionRepository,
        IAccountRepository accountRepository)
    {
        var recurringBankTransactions = SetupTestData(recurringBankTransactionRepository, accountRepository);
        RunChecks(recurringBankTransactionRepository, recurringBankTransactions);
    
        DeleteAllExtension<IRecurringBankTransactionRepository, RecurringBankTransaction>.DeleteAll(recurringBankTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(
        IRecurringBankTransactionRepository recurringBankTransactionRepository,
        IAccountRepository accountRepository)
    {
        var recurringBankTransactions = SetupTestData(recurringBankTransactionRepository, accountRepository);

        foreach (var recurringBankTransaction in recurringBankTransactions)
        {
            var bucketMovementId = recurringBankTransaction.Id;
            recurringBankTransaction.AccountId = 
                recurringBankTransaction.AccountId == _testAccount1.Id ? _testAccount2.Id : _testAccount1.Id;
            recurringBankTransaction.RecurrenceType += 1;
            recurringBankTransaction.RecurrenceAmount += 1;
            recurringBankTransaction.FirstOccurrenceDate = recurringBankTransaction.FirstOccurrenceDate.AddDays(1);
            recurringBankTransaction.Payee += "Update";
            recurringBankTransaction.Memo += "Update";
            recurringBankTransaction.Amount += 1;
            
            var result = recurringBankTransactionRepository.Update(recurringBankTransaction);
            Assert.Equal(1, result);
            Assert.Equal(bucketMovementId, recurringBankTransaction.Id); // Check if no new Guid has been generated (no CREATE)
        }

        RunChecks(recurringBankTransactionRepository, recurringBankTransactions);
        
        DeleteAllExtension<IRecurringBankTransactionRepository, RecurringBankTransaction>.DeleteAll(recurringBankTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(
        IRecurringBankTransactionRepository recurringBankTransactionRepository,
        IAccountRepository accountRepository)
    {
        var recurringBankTransactions = SetupTestData(recurringBankTransactionRepository, accountRepository);

        var deleteResult1 = recurringBankTransactionRepository.Delete(recurringBankTransactions.First().Id);
        var deleteResult2 = recurringBankTransactionRepository.Delete(recurringBankTransactions.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        recurringBankTransactions.Remove(recurringBankTransactions.First());
        recurringBankTransactions.Remove(recurringBankTransactions.Last());
        
        RunChecks(recurringBankTransactionRepository, recurringBankTransactions);
        
        DeleteAllExtension<IRecurringBankTransactionRepository, RecurringBankTransaction>.DeleteAll(recurringBankTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
    }
}