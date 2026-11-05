using System;
using System.Data.Common;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.DuckDb.Migrations;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Test.Common;
using Xunit;

namespace OpenBudgeteer.Core.Test.Tests.Database;

/// <summary>
/// Foreign Keys are only enforced by DuckDB (not by Mock and EF Core InMemory), so these tests run on DuckDB only.
/// They cover the columns that intentionally have no Foreign Key (see InitialCreate migration) and
/// verify that existing Foreign Keys are enforced.
/// </summary>
public class DuckDbForeignKeyDatabaseTest
{
    private static DbConnection GetConnection()
    {
        var connection = new DuckDBConnection("DataSource=:memory:");
        connection.Open();
        DuckDbMigrationRunner.ApplyMigrations(connection);
        return connection;
    }

    private static Bucket CreateBucket(DbConnection connection)
    {
        var bucketGroup = TestDataGenerator.Current.GenerateBucketGroup();
        new DuckDbBucketGroupRepository(connection).Create(bucketGroup);
        var bucket = TestDataGenerator.Current.GenerateBucket(bucketGroup);
        new DuckDbBucketRepository(connection).Create(bucket);
        return bucket;
    }

    [Fact]
    public void UpdateBankTransaction_ChangeAccount_WithBudgetedTransactions()
    {
        using var connection = GetConnection();
        var accountRepository = new DuckDbAccountRepository(connection);
        var bankTransactionRepository = new DuckDbBankTransactionRepository(connection);

        var account1 = TestDataGenerator.Current.GenerateAccount();
        var account2 = TestDataGenerator.Current.GenerateAccount();
        accountRepository.Create(account1);
        accountRepository.Create(account2);
        var bankTransaction = TestDataGenerator.Current.GenerateBankTransaction(account1);
        bankTransactionRepository.Create(bankTransaction);
        new DuckDbBudgetedTransactionRepository(connection).Create(
            TestDataGenerator.Current.GenerateBudgetedTransaction(CreateBucket(connection), bankTransaction));

        bankTransaction.AccountId = account2.Id;
        Assert.Equal(1, bankTransactionRepository.Update(bankTransaction));
        Assert.Equal(account2.Id, bankTransactionRepository.ById(bankTransaction.Id)!.AccountId);
    }

    [Fact]
    public void UpdateBucketRuleSet_ChangeTargetBucket_WithMappingRules()
    {
        using var connection = GetConnection();
        var bucketRuleSetRepository = new DuckDbBucketRuleSetRepository(connection);

        var bucketRuleSet = TestDataGenerator.Current.GenerateBucketRuleSet(CreateBucket(connection));
        bucketRuleSetRepository.Create(bucketRuleSet);
        new DuckDbMappingRuleRepository(connection).Create(
            TestDataGenerator.Current.GenerateMappingRule(bucketRuleSet));

        var newTargetBucket = CreateBucket(connection);
        bucketRuleSet.TargetBucketId = newTargetBucket.Id;
        Assert.Equal(1, bucketRuleSetRepository.Update(bucketRuleSet));
        Assert.Equal(newTargetBucket.Id, bucketRuleSetRepository.ById(bucketRuleSet.Id)!.TargetBucketId);
    }

    [Fact]
    public void DeleteBankTransaction_WithBudgetedTransactions()
    {
        using var connection = GetConnection();
        var account = TestDataGenerator.Current.GenerateAccount();
        new DuckDbAccountRepository(connection).Create(account);
        var bankTransactionRepository = new DuckDbBankTransactionRepository(connection);
        var bankTransaction = TestDataGenerator.Current.GenerateBankTransaction(account);
        bankTransactionRepository.Create(bankTransaction);
        new DuckDbBudgetedTransactionRepository(connection).Create(
            TestDataGenerator.Current.GenerateBudgetedTransaction(CreateBucket(connection), bankTransaction));

        Assert.Equal(2, bankTransactionRepository.Delete(bankTransaction.Id)); // BankTransaction + BudgetedTransaction
        Assert.Null(bankTransactionRepository.ById(bankTransaction.Id));
    }

    [Fact]
    public void DeleteBucketRuleSet_WithMappingRules()
    {
        using var connection = GetConnection();
        var bucketRuleSetRepository = new DuckDbBucketRuleSetRepository(connection);
        var bucketRuleSet = TestDataGenerator.Current.GenerateBucketRuleSet(CreateBucket(connection));
        bucketRuleSetRepository.Create(bucketRuleSet);
        new DuckDbMappingRuleRepository(connection).Create(
            TestDataGenerator.Current.GenerateMappingRule(bucketRuleSet));

        Assert.Equal(2, bucketRuleSetRepository.Delete(bucketRuleSet.Id)); // BucketRuleSet + MappingRule
        Assert.Null(bucketRuleSetRepository.ById(bucketRuleSet.Id));
    }

    [Fact]
    public void CreateBucketMovement_WithoutExistingBucket_IsRejected()
    {
        using var connection = GetConnection();
        var bucketMovement = TestDataGenerator.Current.GenerateBucketMovement(new Bucket { Id = Guid.NewGuid() });

        Assert.Throws<DuckDBException>(() => new DuckDbBucketMovementRepository(connection).Create(bucketMovement));
    }

    [Fact]
    public void DeleteAccount_WithRecurringBankTransactions_IsRejected()
    {
        using var connection = GetConnection();
        var accountRepository = new DuckDbAccountRepository(connection);
        var account = TestDataGenerator.Current.GenerateAccount();
        accountRepository.Create(account);
        new DuckDbRecurringBankTransactionRepository(connection).Create(
            TestDataGenerator.Current.GenerateRecurringBankTransaction(account));

        Assert.Throws<DuckDBException>(() => accountRepository.Delete(account.Id));
        Assert.NotNull(accountRepository.ById(account.Id));
    }
}
