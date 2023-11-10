using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class BankTransaction : IEntity
{
    [Key, Column("TransactionId")]
    public Guid Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    public Account? Account { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Payee { get; set; }

    public string? Memo { get; set; }

    [Column(TypeName = "decimal(65, 2)")]
    public decimal Amount { get; set; }
}
