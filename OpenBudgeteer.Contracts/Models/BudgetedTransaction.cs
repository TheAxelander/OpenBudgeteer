using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models;

public class BudgetedTransaction : BaseObject
{
    private Guid _budgetedTransactionId;
    public Guid BudgetedTransactionId
    {
        get => _budgetedTransactionId;
        set => Set(ref _budgetedTransactionId, value);
    }

    private Guid _transactionId;
    [Required]
    public Guid TransactionId
    {
        get => _transactionId;
        set => Set(ref _transactionId, value);
    }

    private BankTransaction _transaction;
    public BankTransaction Transaction
    {
        get => _transaction;
        set => Set(ref _transaction, value);
    }

    private Guid _bucketId;
    [Required]
    public Guid BucketId
    {
        get => _bucketId;
        set => Set(ref _bucketId, value);
    }

    private Bucket _bucket;
    public Bucket Bucket
    {
        get => _bucket;
        set => Set(ref _bucket, value);
    }

    private decimal _amount;
    [Column(TypeName = "decimal(65, 2)")]
    public decimal Amount
    {
        get => _amount;
        set => Set(ref _amount, value);
    }
}
