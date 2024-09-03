using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Common;

public class TestDataGenerator
{
    public static TestDataGenerator Current => new();
    
    protected TestDataGenerator() { }

    private int GenerateRandomInt(Random random, int min = 0, int max = 100)
    {
        return random.Next(min, max);
    }
    
    private string GenerateRandomString(Random random, int length = 20)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable
            .Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }
    
    private decimal GenerateRandomDecimal(Random random, int min = 0, int max = 100)
    {
        var randomDouble = random.NextDouble();
        var randomDecimal = (decimal)randomDouble + GenerateRandomInt(random, min, max);
        return Math.Round(randomDecimal, 2);
    }

    #region Account

    public Account GenerateAccount() => GenerateAccount(null);
    
    public Account GenerateAccount(int? seed)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        return new Account()
        {
            Name = GenerateRandomString(random),
            IsActive = GenerateRandomInt(random, 0, 1)
        };
    }

    #endregion

    #region BankTransaction

    public BankTransaction GenerateBankTransaction(Account account) => GenerateBankTransaction(null, account);
    
    public BankTransaction GenerateBankTransaction(int? seed, Account account)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        var date = DateTime.Now.AddDays(GenerateRandomInt(random, 1, 30));
        return new BankTransaction()
        {
            AccountId = account.Id,
            TransactionDate = new DateTime(date.Year, date.Month, date.Day),
            Payee = GenerateRandomString(random),
            Memo = GenerateRandomString(random),
            Amount = GenerateRandomDecimal(random, 0, 50)
        };
    }

    #endregion

    #region Bucket

    public Bucket GenerateBucket(BucketGroup bucketGroup) 
        => GenerateBucket(bucketGroup, GenerateBucketVersion());
    public Bucket GenerateBucket(BucketGroup bucketGroup, BucketVersion bucketVersion) 
        => GenerateBucket(null, bucketGroup, bucketVersion);
    
    public Bucket GenerateBucket(int? seed, BucketGroup bucketGroup, BucketVersion bucketVersion)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        var date = DateTime.Now.AddDays(GenerateRandomInt(random, 1, 30));
        var isInactive = Convert.ToBoolean(GenerateRandomInt(random, 0, 1));
        return new Bucket()
        {
            Name = GenerateRandomString(random),
            BucketGroupId = bucketGroup.Id,
            ColorCode = GenerateRandomString(random),
            TextColorCode = GenerateRandomString(random),
            ValidFrom = new DateTime(date.Year, date.Month, date.Day),
            IsInactive = isInactive,
            IsInactiveFrom = isInactive ? new DateTime(date.Year, date.Month, date.Day) : DateTime.MinValue,
            BucketVersions = new List<BucketVersion>() { bucketVersion }
        };
    }

    #endregion

    #region BucketGroup

    public BucketGroup GenerateBucketGroup() => GenerateBucketGroup(null);
    
    public BucketGroup GenerateBucketGroup(int? seed)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        return new BucketGroup()
        {
            Name = GenerateRandomString(random),
            Position = GenerateRandomInt(random, 0, 10)
        };
    }

    #endregion

    #region BucketMovement

    public BucketMovement GenerateBucketMovement(Bucket bucket) => GenerateBucketMovement(null, bucket);
    
    public BucketMovement GenerateBucketMovement(int? seed, Bucket bucket)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        var date = DateTime.Now.AddDays(GenerateRandomInt(random, 1, 30));
        return new BucketMovement()
        {
            BucketId = bucket.Id,
            Amount = GenerateRandomDecimal(random, 0, 50),
            MovementDate = new DateTime(date.Year, date.Month, date.Day)
        };
    }

    #endregion

    #region BucketRuleSet

    public BucketRuleSet GenerateBucketRuleSet(Bucket bucket) => GenerateBucketRuleSet(null, bucket);
    
    public BucketRuleSet GenerateBucketRuleSet(int? seed, Bucket bucket)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        return new BucketRuleSet()
        {
            Priority = GenerateRandomInt(random, 1, 100),
            Name = GenerateRandomString(random),
            TargetBucketId = bucket.Id
        };
    }

    #endregion

    #region BucketVersion

    // No active assignment to a Bucket to prevent deadlock with Bucket generation
    
    public BucketVersion GenerateBucketVersion() => GenerateBucketVersion(null);
    
    public BucketVersion GenerateBucketVersion(int? seed)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        var date = DateTime.Now.AddDays(GenerateRandomInt(random, 1, 30));
        return new BucketVersion()
        {
            Version = GenerateRandomInt(random, 1, 20),
            BucketType = GenerateRandomInt(random, 1, 4),
            BucketTypeXParam = GenerateRandomInt(random, 1, 50),
            BucketTypeYParam = GenerateRandomDecimal(random, 0, 50),
            BucketTypeZParam = new DateTime(date.Year, date.Month, date.Day),
            Notes = GenerateRandomString(random),
            ValidFrom = new DateTime(date.Year, date.Month, date.Day)
        };
    }

    #endregion

    #region BudgetedTransaction

    public BudgetedTransaction GenerateBudgetedTransaction(Bucket bucket, BankTransaction bankTransaction) 
        => GenerateBudgetedTransaction(null, bucket, bankTransaction);
    
    public BudgetedTransaction GenerateBudgetedTransaction(int? seed, Bucket bucket, BankTransaction bankTransaction)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        return new BudgetedTransaction()
        {
            BucketId = bucket.Id,
            TransactionId = bankTransaction.Id,
            Amount = GenerateRandomDecimal(random, 0, 50)
        };
    }

    #endregion

    #region ImportProfile

    public ImportProfile GenerateImportProfile(Account account) => GenerateImportProfile(null, account);
    
    public ImportProfile GenerateImportProfile(int? seed, Account account)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        return new ImportProfile()
        {
            ProfileName = GenerateRandomString(random),
            AccountId = account.Id,
            HeaderRow = GenerateRandomInt(random, 0, 4),
            Delimiter = '|',
            TextQualifier = '"',
            DateFormat = GenerateRandomString(random, 5),
            NumberFormat = GenerateRandomString(random, 5),
            TransactionDateColumnName = GenerateRandomString(random),
            PayeeColumnName = GenerateRandomString(random),
            MemoColumnName = GenerateRandomString(random),
            AmountColumnName = GenerateRandomString(random),
            AdditionalSettingCreditValue = GenerateRandomInt(random, 0, 4),
            CreditColumnName = GenerateRandomString(random),
            CreditColumnIdentifierColumnName = GenerateRandomString(random),
            CreditColumnIdentifierValue = GenerateRandomString(random),
            AdditionalSettingAmountCleanup = Convert.ToBoolean(GenerateRandomInt(random, 0, 1)),
            AdditionalSettingAmountCleanupValue = GenerateRandomString(random)
        };
    }

    #endregion

    #region MappingRule

    public MappingRule GenerateMappingRule(BucketRuleSet bucketRuleSet) => GenerateMappingRule(null, bucketRuleSet);
    
    public MappingRule GenerateMappingRule(int? seed, BucketRuleSet bucketRuleSet)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        return new MappingRule()
        {
            BucketRuleSetId = bucketRuleSet.Id,
            ComparisonField = GenerateRandomInt(random, 1, 4),
            ComparisonType = GenerateRandomInt(random, 1, 4),
            ComparisonValue = GenerateRandomString(random)
        };
    }

    #endregion

    #region RecurringBankTransaction

    public RecurringBankTransaction GenerateRecurringBankTransaction(Account account) 
        => GenerateRecurringBankTransaction(null, account);
    
    public RecurringBankTransaction GenerateRecurringBankTransaction(int? seed, Account account)
    {
        var random = seed != null ? new Random((int)seed) : new Random();
        var date = DateTime.Now.AddDays(GenerateRandomInt(random, 1, 30));
        return new RecurringBankTransaction()
        {
            AccountId = account.Id,
            RecurrenceType = GenerateRandomInt(random, 1, 4),
            RecurrenceAmount = GenerateRandomInt(random, 1, 10),
            FirstOccurrenceDate = new DateTime(date.Year, date.Month, date.Day),
            Payee = GenerateRandomString(random),
            Memo = GenerateRandomString(random),
            Amount = GenerateRandomDecimal(random, 0, 50)
        };
    }

    #endregion
}