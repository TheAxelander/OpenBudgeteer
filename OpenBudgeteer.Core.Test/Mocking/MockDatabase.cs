using System;
using System.Collections.Generic;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking;

public class MockDatabase
{
    public Dictionary<Guid, Account> Accounts { get; } = new();
    public Dictionary<Guid, BankTransaction> BankTransactions { get; } = new();
    public Dictionary<Guid, Bucket> Buckets { get; } = new();
    public Dictionary<Guid, BucketGroup> BucketGroups { get; } = new();
    public Dictionary<Guid, BucketMovement> BucketMovements { get; } = new();
    public Dictionary<Guid, BucketRuleSet> BucketRuleSets { get; } = new();
    public Dictionary<Guid, BucketVersion> BucketVersions { get; } = new();
    public Dictionary<Guid, BudgetedTransaction> BudgetedTransactions { get; } = new();
    public Dictionary<Guid, ImportProfile> ImportProfiles { get; } = new();
    public Dictionary<Guid, MappingRule> MappingRules { get; } = new();
    public Dictionary<Guid, RecurringBankTransaction> RecurringBankTransactions { get; } = new();

    public void Cleanup()
    {
        Accounts.Clear();
        BankTransactions.Clear();
        Buckets.Clear();
        BucketGroups.Clear();
        BucketMovements.Clear();
        BucketRuleSets.Clear();
        BucketVersions.Clear();
        BudgetedTransactions.Clear();
        ImportProfiles.Clear();
        MappingRules.Clear();
        RecurringBankTransactions.Clear();
    }
}