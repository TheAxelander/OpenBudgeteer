using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class BucketVersion : IEntity
{
    [Key, Column("BucketVersionId")]
    public Guid Id { get; set; }
    
    [Required]
    public Guid BucketId { get; set; }

    [JsonIgnore]
    public Bucket Bucket { get; set; } = null!;

    [Required]
    public int Version { get; set; }
        
    [Required]
    public int BucketType { get; set; }

    public int BucketTypeXParam { get; set; }

    [Column(TypeName = "decimal(65, 2)")]
    public decimal BucketTypeYParam { get; set; }

    public DateTime BucketTypeZParam { get; set; }

    public string? Notes { get; set; }

    [Required]
    public DateTime ValidFrom { get; set; }
}
