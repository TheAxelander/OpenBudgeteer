using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class Bucket : IEntity
{
    [Key, Column("BucketId")]
    public Guid Id { get; set; }

    public string? Name { get; set; }

    [Required]
    public Guid BucketGroupId { get; set; }

    public BucketGroup? BucketGroup { get; set; }

    public string? ColorCode { get; set; }
    
    [NotMapped]
    public Color Color => string.IsNullOrEmpty(ColorCode) ? Color.LightGray : Color.FromName(ColorCode);

    [Required]
    public DateTime ValidFrom { get; set; }

    public bool IsInactive { get; set; }

    public DateTime IsInactiveFrom { get; set; }
}
