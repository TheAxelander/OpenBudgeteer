using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels
{
    /// <summary>
    /// ViewModel to handle the multi-assignment of Buckets to one <see cref="BankTransaction"/>
    /// </summary>
    public class PartialBucketViewModelItem : ViewModelBase
    {
        private Bucket _selectedBucket;
        public Bucket SelectedBucket
        {
            get => _selectedBucket;
            set => Set(ref _selectedBucket, value);
        }

        private string _selectedBucketOutput;
        public string SelectedBucketOutput
        {
            get => _selectedBucketOutput;
            set => Set(ref _selectedBucketOutput, value);
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                Set(ref _amount, value);
                AmountChanged?.Invoke(this, new AmountChangedArgs(this, value));
            }
        }

        private ObservableCollection<Bucket> _availableBuckets;
        public ObservableCollection<Bucket> AvailableBuckets
        {
            get => _availableBuckets;
            set => Set(ref _availableBuckets, value);
        }

        public event AmountChangedHandler AmountChanged;
        public event DeleteAssignmentRequestHandler DeleteAssignmentRequest;

        public delegate void AmountChangedHandler(object sender, AmountChangedArgs changedArgs);
        public delegate void DeleteAssignmentRequestHandler(object sender,
            DeleteAssignmentRequestArgs deleteRequestArgs);

        public PartialBucketViewModelItem(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel)
        {
            AvailableBuckets = new ObservableCollection<Bucket>();
            // Add empty Bucket for empty pre-selection
            AvailableBuckets.Add(new Bucket
            {
                BucketId = 0,
                BucketGroupId = 0,
                Name = "No Selection"
            });
            using (var dbContext = new DatabaseContext(dbOptions))
            {
                foreach (var availableBucket in dbContext.Bucket.Where(i => i.BucketId <= 2))
                {
                    AvailableBuckets.Add(availableBucket);
                }

                var query = dbContext.Bucket
                    .Where(i => i.BucketId > 2 &&
                                i.ValidFrom <= yearMonthViewModel.CurrentMonth &&
                                (i.IsInactive == false ||
                                 (i.IsInactive && i.IsInactiveFrom > yearMonthViewModel.CurrentMonth)))
                    .OrderBy(i => i.Name);

                foreach (var availableBucket in query.ToList())
                {
                    AvailableBuckets.Add(availableBucket);
                }
            }            
            SelectedBucket = AvailableBuckets.First();
        }

        public PartialBucketViewModelItem(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel,  Bucket bucket, decimal amount) : this(dbOptions, yearMonthViewModel)
        {
            Amount = amount;
            foreach (var availableBucket in AvailableBuckets)
            {
                if (availableBucket.BucketId == bucket.BucketId)
                {
                    SelectedBucket = availableBucket;
                }
            }
            // Pre-select "No Selection" Bucket if no Bucket was found
            if (SelectedBucket == null) SelectedBucket = AvailableBuckets.First();
        }

        public void DeleteBucket()
        {
            DeleteAssignmentRequest?.Invoke(this, new DeleteAssignmentRequestArgs(this));
        }
    }
}
