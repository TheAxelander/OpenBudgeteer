using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models;

public class BucketMovement : BaseObject
{
    private Guid _bucketMovementId;
    public Guid BucketMovementId
    {
        get => _bucketMovementId;
        set => Set(ref _bucketMovementId, value);
    }

    private Guid _bucketId;
    [Required]
    public Guid BucketId
    {
        get => _bucketId;
        set => Set(ref _bucketId, value);
    }

    private decimal _amount;
    [Column(TypeName = "decimal(65, 2)")]
    public decimal Amount
    {
        get => _amount;
        set => Set(ref _amount, value);
    }

    private DateTime _movementDate;
    public DateTime MovementDate
    {
        get => _movementDate;
        set => Set(ref _movementDate, value);
    }

    public BucketMovement() { }

    public BucketMovement(Bucket bucket, decimal amount, DateTime date) : this()
    {
        BucketId = bucket.BucketId;
        Amount = amount;
        MovementDate = date;
    }
}
