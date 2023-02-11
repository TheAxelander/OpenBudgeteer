using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels;

/// <summary>
/// Helper ViewModel to handle the multi-assignment of Buckets to one <see cref="BankTransaction"/>
/// </summary>
public class PartialBucketViewModelItem : ViewModelBase
{
    private Bucket _selectedBucket;
    /// <summary>
    /// Affected Bucket
    /// </summary>
    public Bucket SelectedBucket
    {
        get => _selectedBucket;
        set => Set(ref _selectedBucket, value);
    }

    private string _selectedBucketOutput;
    /// <summary>
    /// Helper property to generate an output for the Bucket including the assigned amount
    /// </summary>
    public string SelectedBucketOutput
    {
        get => _selectedBucketOutput;
        set => Set(ref _selectedBucketOutput, value);
    }

    private decimal _amount;
    /// <summary>
    /// Money that will be assigned to this Bucket
    /// </summary>
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
    /// <summary>
    /// Helper collection with all available Buckets
    /// </summary>
    public ObservableCollection<Bucket> AvailableBuckets
    {
        get => _availableBuckets;
        set => Set(ref _availableBuckets, value);
    }

    /// <summary>
    /// EventHandler which should be invoked once amount assigned to this Bucket has been changed. Can be used
    /// to start further consistency checks and other calculations based on this change 
    /// </summary>
    public event EventHandler<AmountChangedArgs> AmountChanged;
    /// <summary>
    /// EventHandler which should be invoked in case this instance should start its deletion process. Can be used
    /// in case the way how this instance will be deleted is handled outside of this class
    /// </summary>
    public event EventHandler<DeleteAssignmentRequestArgs> DeleteAssignmentRequest;
    
    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="yearMonth">Current YearMonth</param>
    public PartialBucketViewModelItem(DbContextOptions<DatabaseContext> dbOptions, DateTime yearMonth)
    {
        AvailableBuckets = new ObservableCollection<Bucket>
        {
            new Bucket {BucketId = Guid.Empty, BucketGroupId = Guid.Empty, Name = "No Selection"}
        };
        // Add empty Bucket for empty pre-selection
        using (var dbContext = new DatabaseContext(dbOptions))
        {
            AvailableBuckets.Add(dbContext.Bucket.First(i =>
                i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000001")));
            AvailableBuckets.Add(dbContext.Bucket.First(i =>
                i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000002")));

            var query = dbContext.Bucket
                .Where(i => i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                            i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000002") &&
                            i.ValidFrom <= yearMonth &&
                            (i.IsInactive == false ||
                             (i.IsInactive && i.IsInactiveFrom > yearMonth)))
                .OrderBy(i => i.Name);

            foreach (var availableBucket in query.ToList())
            {
                AvailableBuckets.Add(availableBucket);
            }
        }            
        SelectedBucket = AvailableBuckets.First();
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Bucket"/> object and the final amount to be assigned
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="yearMonth">Current YearMonth</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="amount">Amount to be assigned to this Bucket</param>
    public PartialBucketViewModelItem(DbContextOptions<DatabaseContext> dbOptions, DateTime yearMonth,  Bucket bucket, decimal amount) : this(dbOptions, yearMonth)
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

    /// <summary>
    /// Triggers <see cref="DeleteAssignmentRequest"/>
    /// </summary>
    public void DeleteBucket()
    {
        DeleteAssignmentRequest?.Invoke(this, new DeleteAssignmentRequestArgs(this));
    }
}
