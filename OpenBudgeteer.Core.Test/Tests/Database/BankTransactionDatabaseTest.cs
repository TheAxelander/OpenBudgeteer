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

public class BankTransactionDatabaseTest : BaseDatabaseTest<BankTransaction>
{
    private Account _testAccount1 = new();
    private Account _testAccount2 = new();
    
    protected override void CompareEntities(BankTransaction expected, BankTransaction actual)
    {
        Assert.Equal(expected.AccountId, actual.AccountId);
        Assert.Equal(expected.TransactionDate, actual.TransactionDate);
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
                    new MockBankTransactionRepository(mockDb),
                    new MockAccountRepository(mockDb)
                },
                new object[]
                {
                    new BankTransactionRepository(new DatabaseContext(MariaDbContextOptions)),
                    new AccountRepository(new DatabaseContext(MariaDbContextOptions))
                }
            };
        }
    }

    private List<BankTransaction> SetupTestData(
        IBankTransactionRepository bankTransactionRepository,
        IAccountRepository accountRepository)
    {
        DeleteAllExtension<IBankTransactionRepository, BankTransaction>.DeleteAll(bankTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);

        _testAccount1 = TestDataGenerator.Current.GenerateAccount();
        _testAccount2 = TestDataGenerator.Current.GenerateAccount();
        accountRepository.Create(_testAccount1);
        accountRepository.Create(_testAccount2);

        var result = new List<BankTransaction>();
        for (var i = 1; i <= 4; i++)
        {
            var bankTransaction = i < 4
                ? TestDataGenerator.Current.GenerateBankTransaction(_testAccount1)
                : TestDataGenerator.Current.GenerateBankTransaction(_testAccount2);
            result.Add(bankTransaction);
            var repositoryResult = bankTransactionRepository.Create(bankTransaction);
            Assert.Equal(1, repositoryResult);
            Assert.NotEqual(Guid.Empty, bankTransaction.Id);
        }
        
        return result;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(
        IBankTransactionRepository bankTransactionRepository,
        IAccountRepository accountRepository)
    {
        var bankTransactions = SetupTestData(bankTransactionRepository, accountRepository);
        RunChecks(bankTransactionRepository, bankTransactions);

        DeleteAllExtension<IBankTransactionRepository, BankTransaction>.DeleteAll(bankTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(
        IBankTransactionRepository bankTransactionRepository,
        IAccountRepository accountRepository)
    {
        var bankTransactions = SetupTestData(bankTransactionRepository, accountRepository);

        foreach (var bankTransaction in bankTransactions)
        {
            var bankTransactionId = bankTransaction.Id;
            bankTransaction.TransactionDate = bankTransaction.TransactionDate.AddDays(1);
            bankTransaction.AccountId =
                bankTransaction.AccountId == _testAccount1.Id ? _testAccount2.Id : _testAccount1.Id;
            bankTransaction.Payee += "Update";
            bankTransaction.Memo += "Update";
            bankTransaction.Amount += 1;
            
            var result = bankTransactionRepository.Update(bankTransaction);
            Assert.Equal(1, result);
            Assert.Equal(bankTransactionId, bankTransaction.Id); // Check if no new Guid has been generated (no CREATE)
        }
        
        RunChecks(bankTransactionRepository, bankTransactions);

        DeleteAllExtension<IBankTransactionRepository, BankTransaction>.DeleteAll(bankTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(
        IBankTransactionRepository bankTransactionRepository,
        IAccountRepository accountRepository)
    {
        var bankTransactions = SetupTestData(bankTransactionRepository, accountRepository);

        var deleteResult1 = bankTransactionRepository.Delete(bankTransactions.First().Id);
        var deleteResult2 = bankTransactionRepository.Delete(bankTransactions.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        bankTransactions.Remove(bankTransactions.First());
        bankTransactions.Remove(bankTransactions.Last());
        
        RunChecks(bankTransactionRepository, bankTransactions);
        
        DeleteAllExtension<IBankTransactionRepository, BankTransaction>.DeleteAll(bankTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
    }
}