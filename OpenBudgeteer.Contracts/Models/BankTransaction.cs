using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models;

public class BankTransaction : BaseObject
{
    private Guid _transactionId;
    [Key]
    public Guid TransactionId
    {
        get => _transactionId;
        set => Set(ref _transactionId, value);
    }

    private Guid _accountId;
    [Required]
    public Guid AccountId
    {
        get => _accountId;
        set => Set(ref _accountId, value);
    }

    private Account _account;

    public Account Account
    {
        get => _account;
        set => Set(ref _account, value);
    }

    private DateTime _transactionDate;
    public DateTime TransactionDate
    {
        get => _transactionDate;
        set => Set(ref _transactionDate, value);
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
}
