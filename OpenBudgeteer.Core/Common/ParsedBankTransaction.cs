using System;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Common;

public record ParsedBankTransaction
{
    public DateTime TransactionDate { get; set; }
    public string Memo { get; set; } = string.Empty;
    public string Payee { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public BankTransaction AsBankTransaction()
    {
        return new BankTransaction()
        {
            TransactionDate = TransactionDate,
            Memo = Memo,
            Payee = Payee,
            Amount = Amount
        };
    }
}