using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class BucketGroup : IEntity
{
    [Key, Column("BucketGroupId")]
    public Guid Id { get; set; }

    public string? Name { get; set; }

    [Required]
    public int Position { get; set; }
}
