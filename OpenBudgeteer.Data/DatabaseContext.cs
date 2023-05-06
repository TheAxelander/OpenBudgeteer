﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Models;

namespace OpenBudgeteer.Core.Common.Database;

public class DatabaseContext : DbContext
{
    public DbSet<Account> Account { get; set; }
    public DbSet<BankTransaction> BankTransaction { get; set; }
    public DbSet<RecurringBankTransaction> RecurringBankTransaction { get; set; }
    public DbSet<Bucket> Bucket { get; set; }
    public DbSet<BucketGroup> BucketGroup { get; set; }
    public DbSet<BucketMovement> BucketMovement { get; set; }
    public DbSet<BucketVersion> BucketVersion { get; set; }
    public DbSet<BudgetedTransaction> BudgetedTransaction { get; set; }
    public DbSet<ImportProfile> ImportProfile { get; set; }
    public DbSet<BucketRuleSet> BucketRuleSet { get; set; }
    public DbSet<MappingRule> MappingRule { get; set; }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    #region Create

    public int CreateAccount(Account account)
        => CreateAccounts(new List<Account>() { account });

    public int CreateAccounts(IEnumerable<Account> accounts)
    {
        Account.AddRange(accounts);
        return SaveChanges();
    }
    
    public int CreateBankTransaction(BankTransaction bankTransaction)
        => CreateBankTransactions(new List<BankTransaction>() { bankTransaction });

    public int CreateBankTransactions(IEnumerable<BankTransaction> bankTransactions)
    {
        BankTransaction.AddRange(bankTransactions);
        return SaveChanges();
    }
    
    public int CreateRecurringBankTransaction(RecurringBankTransaction recurringBankTransaction)
        => CreateRecurringBankTransactions(new List<RecurringBankTransaction>() { recurringBankTransaction });

    public int CreateRecurringBankTransactions(IEnumerable<RecurringBankTransaction> recurringBankTransactions)
    {
        RecurringBankTransaction.AddRange(recurringBankTransactions);
        return SaveChanges();
    }

    public int CreateBucket(Bucket bucket)
        => CreateBuckets(new List<Bucket>() { bucket });

    public int CreateBuckets(IEnumerable<Bucket> buckets)
    {
        Bucket.AddRange(buckets);
        return SaveChanges();
    }

    public int CreateBucketGroup(BucketGroup bucketGroup)
        => CreateBucketGroups(new List<BucketGroup>() { bucketGroup });

    public int CreateBucketGroups(IEnumerable<BucketGroup> bucketGroups)
    {
        BucketGroup.AddRange(bucketGroups);
        return SaveChanges();
    }

    public int CreateBucketMovement(BucketMovement bucketMovement)
        => CreateBucketMovements(new List<BucketMovement>() { bucketMovement });

    public int CreateBucketMovements(IEnumerable<BucketMovement> bucketMovements)
    {
        BucketMovement.AddRange(bucketMovements);
        return SaveChanges();
    }

    public int CreateBucketVersion(BucketVersion bucketVersion)
        => CreateBucketVersions(new List<BucketVersion>() { bucketVersion });

    public int CreateBucketVersions(IEnumerable<BucketVersion> bucketVersions)
    {
        BucketVersion.AddRange(bucketVersions);
        return SaveChanges();
    }

    public int CreateBudgetedTransaction(BudgetedTransaction budgetedTransaction)
        => CreateBudgetedTransactions(new List<BudgetedTransaction>() { budgetedTransaction });

    public int CreateBudgetedTransactions(IEnumerable<BudgetedTransaction> budgetedTransactions)
    {
        BudgetedTransaction.AddRange(budgetedTransactions);
        return SaveChanges();
    }

    public int CreateImportProfile(ImportProfile importProfile)
        => CreateImportProfiles(new List<ImportProfile>() { importProfile });

    public int CreateImportProfiles(IEnumerable<ImportProfile> importProfiles)
    {
        ImportProfile.AddRange(importProfiles);
        return SaveChanges();
    }

    public int CreateBucketRuleSet(BucketRuleSet bucketRuleSet)
        => CreateBucketRuleSets(new List<BucketRuleSet>() { bucketRuleSet });

    public int CreateBucketRuleSets(IEnumerable<BucketRuleSet> bucketRuleSets)
    {
        BucketRuleSet.AddRange(bucketRuleSets);
        return SaveChanges();
    }

    public int CreateMappingRule(MappingRule mappingRule)
        => CreateMappingRules(new List<MappingRule>() { mappingRule });

    public int CreateMappingRules(IEnumerable<MappingRule> mappingRules)
    {
        MappingRule.AddRange(mappingRules);
        return SaveChanges();
    }

    #endregion

    #region Update

    public int UpdateAccount(Account account)
        => UpdateAccounts(new List<Account>() { account });

    public int UpdateAccounts(IEnumerable<Account> accounts)
    {
        foreach (var account in accounts)
        {
            var dbAccount = Account.First(i => i.AccountId == account.AccountId);
            Entry(dbAccount).CurrentValues.SetValues(account);
        }
        return SaveChanges();
    }

    public int UpdateBankTransaction(BankTransaction bankTransaction)
        => UpdateBankTransactions(new List<BankTransaction>() { bankTransaction });

    public int UpdateBankTransactions(IEnumerable<BankTransaction> bankTransactions)
    {
        foreach (var bankTransaction in bankTransactions)
        {
            var dbBankTransaction = BankTransaction.First(i => i.TransactionId == bankTransaction.TransactionId);
            Entry(dbBankTransaction).CurrentValues.SetValues(bankTransaction);
        }
        return SaveChanges();
    }
    
    public int UpdateRecurringBankTransaction(RecurringBankTransaction recurringBankTransaction)
        => UpdateRecurringBankTransactions(new List<RecurringBankTransaction>() { recurringBankTransaction });

    public int UpdateRecurringBankTransactions(IEnumerable<RecurringBankTransaction> recurringBankTransactions)
    {
        foreach (var recurringBankTransaction in recurringBankTransactions)
        {
            var dbRecurringBankTransaction = RecurringBankTransaction.First(i => i.TransactionId == recurringBankTransaction.TransactionId);
            Entry(dbRecurringBankTransaction).CurrentValues.SetValues(recurringBankTransaction);
        }
        return SaveChanges();
    }

    public int UpdateBucket(Bucket bucket)
        => UpdateBuckets(new List<Bucket>() { bucket });

    public int UpdateBuckets(IEnumerable<Bucket> buckets)
    {
        foreach (var bucket in buckets)
        {
            var dbBucket = Bucket.First(i => i.BucketId == bucket.BucketId);
            Entry(dbBucket).CurrentValues.SetValues(bucket);
        }
        return SaveChanges();
    }

    public int UpdateBucketGroup(BucketGroup bucketGroup)
        => UpdateBucketGroups(new List<BucketGroup>() { bucketGroup });

    public int UpdateBucketGroups(IEnumerable<BucketGroup> bucketGroups)
    {
        foreach (var bucketGroup in bucketGroups)
        {
            var dbBucketGroup = BucketGroup.First(i => i.BucketGroupId == bucketGroup.BucketGroupId);
            Entry(dbBucketGroup).CurrentValues.SetValues(bucketGroup);
        }
        return SaveChanges();
    }

    public int UpdateBucketMovement(BucketMovement bucketMovement)
        => UpdateBucketMovements(new List<BucketMovement>() { bucketMovement });

    public int UpdateBucketMovements(IEnumerable<BucketMovement> bucketMovements)
    {
        foreach (var bucketMovement in bucketMovements)
        {
            var dbBucketMovement = BucketMovement.First(i => i.BucketMovementId == bucketMovement.BucketMovementId);
            Entry(dbBucketMovement).CurrentValues.SetValues(bucketMovement);
        }
        return SaveChanges();
    }

    public int UpdateBucketVersion(BucketVersion bucketVersion)
        => UpdateBucketVersions(new List<BucketVersion>() { bucketVersion });

    public int UpdateBucketVersions(IEnumerable<BucketVersion> bucketVersions)
    {
        foreach (var bucketVersion in bucketVersions)
        {
            var dbBucketVersion = BucketVersion.First(i => i.BucketVersionId == bucketVersion.BucketVersionId);
            Entry(dbBucketVersion).CurrentValues.SetValues(bucketVersion);
        }
        return SaveChanges();
    }

    public int UpdateBudgetedTransaction(BudgetedTransaction budgetedTransaction)
        => UpdateBudgetedTransactions(new List<BudgetedTransaction>() { budgetedTransaction });

    public int UpdateBudgetedTransactions(IEnumerable<BudgetedTransaction> budgetedTransactions)
    {
        foreach (var budgetedTransaction in budgetedTransactions)
        {
            var dbBudgetedTransaction = BudgetedTransaction.First(i => i.BudgetedTransactionId == budgetedTransaction.BudgetedTransactionId);
            Entry(dbBudgetedTransaction).CurrentValues.SetValues(budgetedTransaction);
        }
        return SaveChanges();
    }

    public int UpdateImportProfile(ImportProfile importProfile)
        => UpdateImportProfiles(new List<ImportProfile>() { importProfile });

    public int UpdateImportProfiles(IEnumerable<ImportProfile> importProfiles)
    {
        foreach (var importProfile in importProfiles)
        {
            var dbImportProfile = ImportProfile.First(i => i.ImportProfileId == importProfile.ImportProfileId);
            Entry(dbImportProfile).CurrentValues.SetValues(importProfile);
        }
        return SaveChanges();
    }

    public int UpdateBucketRuleSet(BucketRuleSet bucketRuleSet)
        => UpdateBucketRuleSets(new List<BucketRuleSet>() { bucketRuleSet });

    public int UpdateBucketRuleSets(IEnumerable<BucketRuleSet> bucketRuleSets)
    {
        foreach (var bucketRuleSet in bucketRuleSets)
        {
            var dbBucketRuleSet = BucketRuleSet.First(i => i.BucketRuleSetId == bucketRuleSet.BucketRuleSetId);
            Entry(dbBucketRuleSet).CurrentValues.SetValues(bucketRuleSet);
        }
        return SaveChanges();
    }

    public int UpdateMappingRule(MappingRule mappingRule)
        => UpdateMappingRules(new List<MappingRule>() { mappingRule });

    public int UpdateMappingRules(IEnumerable<MappingRule> mappingRules)
    {
        foreach (var mappingRule in mappingRules)
        {
            var dbMappingRule = MappingRule.First(i => i.MappingRuleId == mappingRule.MappingRuleId);
            Entry(dbMappingRule).CurrentValues.SetValues(mappingRule);
        }
        return SaveChanges();
    }

    #endregion

    #region Delete

    public int DeleteAccount(Account account)
        => DeleteAccounts(new List<Account>() { account });

    public int DeleteAccounts(IEnumerable<Account> accounts)
    {
        Account.RemoveRange(accounts);
        return SaveChanges();
    }

    public int DeleteBankTransaction(BankTransaction bankTransaction)
        => DeleteBankTransactions(new List<BankTransaction>() { bankTransaction });

    public int DeleteBankTransactions(IEnumerable<BankTransaction> bankTransactions)
    {
        BankTransaction.RemoveRange(bankTransactions);
        return SaveChanges();
    }
    
    public int DeleteRecurringBankTransaction(RecurringBankTransaction recurringBankTransaction)
        => DeleteRecurringBankTransactions(new List<RecurringBankTransaction>() { recurringBankTransaction });

    public int DeleteRecurringBankTransactions(IEnumerable<RecurringBankTransaction> recurringBankTransactions)
    {
        RecurringBankTransaction.RemoveRange(recurringBankTransactions);
        return SaveChanges();
    }

    public int DeleteBucket(Bucket bucket)
        => DeleteBuckets(new List<Bucket>() { bucket });

    public int DeleteBuckets(IEnumerable<Bucket> buckets)
    {
        var validDeletions = buckets
            .Where(i =>
                i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000002"))
            .ToList();
        Bucket.RemoveRange(validDeletions);
        return SaveChanges();
    }

    public int DeleteBucketGroup(BucketGroup bucketGroup)
        => DeleteBucketGroups(new List<BucketGroup>() { bucketGroup });

    public int DeleteBucketGroups(IEnumerable<BucketGroup> bucketGroups)
    {
        var validDeletions = bucketGroups
            .Where(i => i.BucketGroupId != Guid.Parse("00000000-0000-0000-0000-000000000001"))
            .ToList();
        BucketGroup.RemoveRange(validDeletions);
        return SaveChanges();
    }

    public int DeleteBucketMovement(BucketMovement bucketMovement)
        => DeleteBucketMovements(new List<BucketMovement>() { bucketMovement });

    public int DeleteBucketMovements(IEnumerable<BucketMovement> bucketMovements)
    {
        BucketMovement.RemoveRange(bucketMovements);
        return SaveChanges();
    }

    public int DeleteBucketVersion(BucketVersion bucketVersion)
        => DeleteBucketVersions(new List<BucketVersion>() { bucketVersion });

    public int DeleteBucketVersions(IEnumerable<BucketVersion> bucketVersions)
    {
        BucketVersion.RemoveRange(bucketVersions);
        return SaveChanges();
    }

    public int DeleteBudgetedTransaction(BudgetedTransaction budgetedTransaction)
        => DeleteBudgetedTransactions(new List<BudgetedTransaction>() { budgetedTransaction });

    public int DeleteBudgetedTransactions(IEnumerable<BudgetedTransaction> budgetedTransactions)
    {
        BudgetedTransaction.RemoveRange(budgetedTransactions);
        return SaveChanges();
    }

    public int DeleteImportProfile(ImportProfile importProfile)
        => DeleteImportProfiles(new List<ImportProfile>() { importProfile });

    public int DeleteImportProfiles(IEnumerable<ImportProfile> importProfiles)
    {
        ImportProfile.RemoveRange(importProfiles);
        return SaveChanges();
    }

    public int DeleteBucketRuleSet(BucketRuleSet bucketRuleSet)
        => DeleteBucketRuleSets(new List<BucketRuleSet>() { bucketRuleSet });

    public int DeleteBucketRuleSets(IEnumerable<BucketRuleSet> bucketRuleSets)
    {
        BucketRuleSet.RemoveRange(bucketRuleSets);
        return SaveChanges();
    }

    public int DeleteMappingRule(MappingRule mappingRule)
        => DeleteMappingRules(new List<MappingRule>() { mappingRule });

    public int DeleteMappingRules(IEnumerable<MappingRule> mappingRules)
    {
        MappingRule.RemoveRange(mappingRules);
        return SaveChanges();
    }

    #endregion
}

