using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models
{
    public class BudgetedTransaction : BaseObject
    {
        private int _budgetedTransactionId;
        public int BudgetedTransactionId
        {
            get => _budgetedTransactionId;
            set => Set(ref _budgetedTransactionId, value);
        }

        private int _transactionId;
        [Required]
        public int TransactionId
        {
            get => _transactionId;
            set => Set(ref _transactionId, value);
        }

        private int _bucketId;
        [Required]
        public int BucketId
        {
            get => _bucketId;
            set => Set(ref _bucketId, value);
        }

        private decimal _amount;
        [Column(TypeName = "decimal(65, 2)")]
        public decimal Amount
        {
            get => _amount;
            set => Set(ref _amount, value);
        }
    }
}
