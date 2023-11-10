using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class BudgetedTransaction : IEntity
{
    [Key, Column("BudgetedTransactionId")]
    public Guid Id { get; set; }

    [Required]
    public Guid TransactionId { get; set; }

    public BankTransaction? Transaction { get; set; }

    [Required]
    public Guid BucketId { get; set; }

    public Bucket? Bucket { get; set; }

    [Column(TypeName = "decimal(65, 2)")]
    public decimal Amount { get; set; }
}
