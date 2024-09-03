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

public class BudgetedTransactionDatabaseTest : BaseDatabaseTest<BudgetedTransaction>
{
    private readonly Bucket _incomeBucket = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), 
        Name = "Income"
    };
    private readonly Bucket _transferBucket = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), 
        Name = "Transfer"
    };

    private Account _testAccount = new();
    private BankTransaction _testBankTransaction1 = new();
    private BankTransaction _testBankTransaction2 = new();
    private BucketGroup _testBucketGroup = new();
    private Bucket _testBucket1 = new();
    private Bucket _testBucket2 = new();
    
    protected override void CompareEntities(BudgetedTransaction expected, BudgetedTransaction actual)
    {
        Assert.Equal(expected.TransactionId, actual.TransactionId);
        Assert.Equal(expected.BucketId, actual.BucketId);
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
                    new MockBudgetedTransactionRepository(mockDb),
                    new MockAccountRepository(mockDb),
                    new MockBankTransactionRepository(mockDb),
                    new MockBucketRepository(mockDb),
                    new MockBucketGroupRepository(mockDb)
                },
                new object[]
                {
                    new BudgetedTransactionRepository(new DatabaseContext(MariaDbContextOptions)),
                    new AccountRepository(new DatabaseContext(MariaDbContextOptions)),
                    new BankTransactionRepository(new DatabaseContext(MariaDbContextOptions)),
                    new BucketRepository(new DatabaseContext(MariaDbContextOptions)),
                    new BucketGroupRepository(new DatabaseContext(MariaDbContextOptions))
                }
            };
        }
    }
    
    private List<BudgetedTransaction> SetupTestData(
        IBudgetedTransactionRepository budgetedTransactionRepository,
        IAccountRepository accountRepository,
        IBankTransactionRepository bankTransactionRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        DeleteAllExtension<IBudgetedTransactionRepository, BudgetedTransaction>.DeleteAll(budgetedTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
        DeleteAllExtension<IBankTransactionRepository, BankTransaction>.DeleteAll(bankTransactionRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);

        _testAccount = TestDataGenerator.Current.GenerateAccount();
        accountRepository.Create(_testAccount);
        
        _testBankTransaction1 = TestDataGenerator.Current.GenerateBankTransaction(_testAccount);
        _testBankTransaction2 = TestDataGenerator.Current.GenerateBankTransaction(_testAccount);
        _testBankTransaction2.Amount = -25;
        bankTransactionRepository.Create(_testBankTransaction1);
        bankTransactionRepository.Create(_testBankTransaction2);

        _testBucketGroup = TestDataGenerator.Current.GenerateBucketGroup();
        bucketGroupRepository.Create(_testBucketGroup);

        _testBucket1 = TestDataGenerator.Current.GenerateBucket(_testBucketGroup);
        _testBucket2 = TestDataGenerator.Current.GenerateBucket(_testBucketGroup);
        bucketRepository.Create(_testBucket1);
        bucketRepository.Create(_testBucket2);
        
        var budgetedTransactions = new List<BudgetedTransaction>();
        budgetedTransactions.Add(new()
        {
            BucketId = _testBucket1.Id,
            TransactionId = _testBankTransaction1.Id,
            Amount = _testBankTransaction1.Amount
        });
        budgetedTransactions.Add(new()
        {
            BucketId = _testBucket1.Id,
            TransactionId = _testBankTransaction2.Id,
            Amount = -15
        });
        budgetedTransactions.Add(new()
        {
            BucketId = _testBucket2.Id,
            TransactionId = _testBankTransaction2.Id,
            Amount = -10
        });
        
        foreach (var budgetedTransaction in budgetedTransactions)
        {
            var result = budgetedTransactionRepository.Create(budgetedTransaction);
            Assert.Equal(1, result);
            Assert.NotEqual(Guid.Empty, budgetedTransaction.Id);
        }
    
        return budgetedTransactions;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(
        IBudgetedTransactionRepository budgetedTransactionRepository,
        IAccountRepository accountRepository,
        IBankTransactionRepository bankTransactionRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var budgetedTransactions = SetupTestData(
            budgetedTransactionRepository,
            accountRepository,
            bankTransactionRepository, 
            bucketRepository, 
            bucketGroupRepository);
        RunChecks(budgetedTransactionRepository, budgetedTransactions);
    
        DeleteAllExtension<IBudgetedTransactionRepository, BudgetedTransaction>.DeleteAll(budgetedTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
        DeleteAllExtension<IBankTransactionRepository, BankTransaction>.DeleteAll(bankTransactionRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(
        IBudgetedTransactionRepository budgetedTransactionRepository,
        IAccountRepository accountRepository,
        IBankTransactionRepository bankTransactionRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var budgetedTransactions = SetupTestData(
            budgetedTransactionRepository,
            accountRepository,
            bankTransactionRepository, 
            bucketRepository, 
            bucketGroupRepository);
    
        foreach (var budgetedTransaction in budgetedTransactions)
        {
            Assert.Throws<NotSupportedException>(() => budgetedTransactionRepository.Update(budgetedTransaction));
            // var budgetedTransactionId = budgetedTransaction.Id;
            // if (budgetedTransaction.TransactionId == _testBankTransaction1.Id) continue;
            // budgetedTransaction.Amount = budgetedTransaction.BucketId == _testBucket1.Id ? -10 : -15;
            // var result = budgetedTransactionRepository.Update(budgetedTransaction);
            // Assert.Equal(1, result);
            // Assert.Equal(budgetedTransactionId, budgetedTransaction.Id); // Check if no new Guid has been generated (no CREATE)
        }
    
        RunChecks(budgetedTransactionRepository, budgetedTransactions);
        
        DeleteAllExtension<IBudgetedTransactionRepository, BudgetedTransaction>.DeleteAll(budgetedTransactionRepository);
        DeleteAllExtension<IAccountRepository, Account>.DeleteAll(accountRepository);
        DeleteAllExtension<IBankTransactionRepository, BankTransaction>.DeleteAll(bankTransactionRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(
        IBudgetedTransactionRepository budgetedTransactionRepository,
        IAccountRepository accountRepository,
        IBankTransactionRepository bankTransactionRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var budgetedTransactions = SetupTestData(
            budgetedTransactionRepository,
            accountRepository,
            bankTransactionRepository, 
            bucketRepository, 
            bucketGroupRepository);
        
        var deleteResult1 = budgetedTransactionRepository.Delete(budgetedTransactions.First().Id);
        var deleteResult2 = budgetedTransactionRepository.Delete(budgetedTransactions.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        budgetedTransactions.Remove(budgetedTransactions.First());
        budgetedTransactions.Remove(budgetedTransactions.Last());
        
        RunChecks(budgetedTransactionRepository, budgetedTransactions);
        
        DeleteAllExtension<IBudgetedTransactionRepository, BudgetedTransaction>.DeleteAll(budgetedTransactionRepository);
    }
}