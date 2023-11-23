using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class BucketMovement : IEntity
{
    [Key, Column("BucketMovementId")]
    public Guid Id { get; set; }

    [Required]
    public Guid BucketId { get; set; }

    [JsonIgnore]
    public Bucket Bucket { get; set; } = null!;

    [Column(TypeName = "decimal(65, 2)")]
    public decimal Amount { get; set; }

    public DateTime MovementDate { get; set; }
}
