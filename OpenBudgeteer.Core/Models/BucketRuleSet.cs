using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models;

public class BucketRuleSet : BaseObject
{
    private Guid _bucketRuleSetId;
    public Guid BucketRuleSetId
    {
        get => _bucketRuleSetId;
        set => Set(ref _bucketRuleSetId, value);
    }

    private int _priority;
    [Required]
    public int Priority
    {
        get => _priority;
        set => Set(ref _priority, value);
    }

    private string _name;
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    private Guid _targetBucketId;
    [Required]
    public Guid TargetBucketId
    {
        get => _targetBucketId;
        set => Set(ref _targetBucketId, value);
    }

    private Bucket _targetBucket;
    public Bucket TargetBucket
    {
        get => _targetBucket;
        set => Set(ref _targetBucket, value);
    }
}
