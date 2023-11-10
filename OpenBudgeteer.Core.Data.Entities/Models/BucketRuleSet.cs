using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class BucketRuleSet : IEntity
{
    [Key, Column("BucketRuleSetId")]
    public Guid Id { get; set; }

    [Required]
    public int Priority { get; set; }

    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid TargetBucketId { get; set; }

    public Bucket? TargetBucket { get; set; }
}
