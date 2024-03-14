using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text.Json.Serialization;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class Bucket : IEntity
{
    [Key, Column("BucketId")]
    public Guid Id { get; set; }

    public string? Name { get; set; }

    [Required]
    public Guid BucketGroupId { get; set; }

    [JsonIgnore]
    public BucketGroup BucketGroup { get; set; } = null!;

    public string? ColorCode { get; set; }
    
    public string? TextColorCode { get; set; }
    
    [Required]
    public DateTime ValidFrom { get; set; }

    public bool IsInactive { get; set; }

    public DateTime IsInactiveFrom { get; set; }
    
    [NotMapped]
    public BucketVersion? CurrentVersion { get; set; }
    
    public ICollection<BucketVersion>? BucketVersions { get; set; }
    
    public ICollection<BudgetedTransaction>? BudgetedTransactions { get; set; }
    
    public ICollection<BucketMovement>? BucketMovements { get; set; }
}
