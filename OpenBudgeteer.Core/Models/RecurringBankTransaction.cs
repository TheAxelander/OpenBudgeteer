using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models;

public class RecurringBankTransaction: BaseObject
{
    private int _transactionId;
    [Key]
    public int TransactionId
    {
        get => _transactionId;
        set => Set(ref _transactionId, value);
    }

    private int _accountId;
    [Required]
    public int AccountId
    {
        get => _accountId;
        set => Set(ref _accountId, value);
    }
    
    private int _recurrenceType;
    /// <summary>
    /// Parameter for type of recurrence:
    /// <para>
    /// 1 - Weeks<br />
    /// 2 - Months<br />
    /// 3 - Quarters<br />
    /// 4 - Years
    /// </para>
    /// </summary>
    [Required]
    public int RecurrenceType
    {
        get => _recurrenceType;
        set => Set(ref _recurrenceType, value);
    }

    private int _recurrenceAmount;
    [Required]
    public int RecurrenceAmount
    {
        get => _recurrenceAmount;
        set => Set(ref _recurrenceAmount, value);
    }
    
    private DateTime _firstOccurenceDate;
    public DateTime FirstOccurenceDate
    {
        get => _firstOccurenceDate;
        set => Set(ref _firstOccurenceDate, value);
    }

    private string _payee;
    public string Payee
    {
        get => _payee;
        set => Set(ref _payee, value);
    }

    private string _memo;
    public string Memo
    {
        get => _memo;
        set => Set(ref _memo, value);
    }

    private decimal _amount;
    [Column(TypeName = "decimal(65, 2)")]
    public decimal Amount
    {
        get => _amount;
        set => Set(ref _amount, value);
    }

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
    /// <returns>New <see cref="BankTransaction"/> object with <see cref="BankTransaction.TransactionId"/>=0</returns>
    public BankTransaction GetAsBankTransaction(DateTime transactionDate)
    {
        return new BankTransaction()
        {
            TransactionId = 0,
            TransactionDate = transactionDate,
            AccountId = AccountId,
            Payee = Payee,
            Memo = Memo,
            Amount = Amount
        };
    }
}