using System.Collections.Generic;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Common.Extensions;

/// <summary>
/// Compares two <see cref="BankTransaction"/> if they are a potential match based on
/// Date, Amount and Payee or Memo
/// </summary>
internal class DuplicateMatchComparer : IEqualityComparer<BankTransaction>
{
    public bool Equals(BankTransaction? x, BankTransaction? y)
    {
        if (x is null || y is null) return false;
        return 
            x.TransactionDate.Date == y.TransactionDate.Date && 
            x.Amount == y.Amount && 
            (x.Payee == y.Payee || x.Memo == y.Memo);
    }

    public int GetHashCode(BankTransaction obj)
    {
        return new { obj.TransactionDate.Date, obj.Amount }.GetHashCode();
    }
}
