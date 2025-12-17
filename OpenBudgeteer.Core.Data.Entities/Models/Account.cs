using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Data.Entities.Models;

[Table(nameof(Account))]
public class Account : IEntity
{
    [Key, Column("AccountId")]
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public int IsActive { get; set; }
}
