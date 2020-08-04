using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models
{
    public class BucketMovement : BaseObject
    {
        private int _bucketMovementId;
        public int BucketMovementId
        {
            get => _bucketMovementId;
            set => Set(ref _bucketMovementId, value);
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

        private DateTime _movementDate;
        public DateTime MovementDate
        {
            get => _movementDate;
            set => Set(ref _movementDate, value);
        }

        public BucketMovement() { }

        public BucketMovement(Bucket bucket, decimal amount, DateTime date) : this()
        {
            BucketId = bucket.BucketId;
            Amount = amount;
            MovementDate = date;
        }
    }
}
