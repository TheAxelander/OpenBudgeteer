using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class RecurringBankTransaction: IEntity
{
    [Key, Column("TransactionId")]
    public Guid Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    [Required]
    public int RecurrenceType { get; set; }
        
    [Required]
    public int RecurrenceAmount { get; set; }
    
    public DateTime FirstOccurrenceDate { get; set; }

    public string? Payee { get; set; }

    public string? Memo { get; set; }

    [Column(TypeName = "decimal(65, 2)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Calculates the next occurrence based on the passed date
    /// </summary>
    /// <param name="currentDate">Date from which the next iteration should be calculated</param>
    /// <returns>Potential next occurrence date for a <see cref="BankTransaction"/></returns>
    /// <exception cref="Exception">
    /// <see cref="RecurrenceType"/> has a value which is not defined (not between 1-4).
    /// </exception>
    public DateTime GetNextIterationDate(DateTime currentDate)
    {
        switch (RecurrenceType)
        {
            case 1:
                return currentDate.AddDays(RecurrenceAmount * 7);          
            case 2:
                return currentDate.AddMonths(RecurrenceAmount);
            case 3:
                return currentDate.AddMonths(RecurrenceAmount * 3);
            case 4:
                return currentDate.AddYears(RecurrenceAmount);
            default:
                throw new Exception("Undefined Recurrence Type");
        }
    }

    /// <summary>
    /// Converts current <see cref="RecurringBankTransaction"/> instance to a new <see cref="BankTransaction"/> object
    /// </summary>
    /// <param name="transactionDate">Value for <see cref="BankTransaction.TransactionDate"/></param>
    /// <returns>New <see cref="BankTransaction"/> object with <see cref="BankTransaction.Id"/>=0</returns>
    public BankTransaction GetAsBankTransaction(DateTime transactionDate)
    {
        return new BankTransaction()
        {
            Id = Guid.Empty,
            TransactionDate = transactionDate,
            AccountId = AccountId,
            Account = new Account()
            {
                Id = Account!.Id,
                Name = Account.Name,
                IsActive = Account.IsActive
            },
            Payee = Payee,
            Memo = Memo,
            Amount = Amount
        };
    }
}