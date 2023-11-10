using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class PartialBucketViewModel : ViewModelBase
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

    /// <summary>
    /// Helper collection with all available Buckets
    /// </summary>
    public readonly ObservableCollection<Bucket> AvailableBuckets;

    /// <summary>
    /// EventHandler which should be invoked once amount assigned to this Bucket has been changed. Can be used
    /// to start further consistency checks and other calculations based on this change 
    /// </summary>
    public event EventHandler<AmountChangedArgs>? AmountChanged;
    /// <summary>
    /// EventHandler which should be invoked in case this instance should start its deletion process. Can be used
    /// in case the way how this instance will be deleted is handled outside of this class
    /// </summary>
    public event EventHandler<DeleteAssignmentRequestArgs>? DeleteAssignmentRequest;

    /// <summary>
    /// Initialize ViewModel with a "No Selection" <see cref="Bucket"/>
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    protected PartialBucketViewModel(IServiceManager serviceManager, IEnumerable<Bucket> availableBuckets) 
        : base(serviceManager)
    {
        _selectedBucketOutput = string.Empty;
        
        // Add empty Bucket for empty pre-selection
        AvailableBuckets = new ObservableCollection<Bucket>
        {
            new()
            {
                Id = Guid.Empty, 
                BucketGroupId = Guid.Empty, 
                Name = "No Selection"
            }
        };
        
        foreach (var bucket in availableBuckets)
        {
            AvailableBuckets.Add(bucket);
        }
        _selectedBucket = AvailableBuckets.First();
    }

    /// <summary>
    /// Initialize ViewModel with a "No Selection" <see cref="Bucket"/>
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    /// <param name="amount">Amount to be assigned to this Bucket</param>
    public static PartialBucketViewModel CreateNoSelection(IServiceManager serviceManager,
        IEnumerable<Bucket> availableBuckets, decimal amount = 0)
    {
        return new PartialBucketViewModel(serviceManager, availableBuckets)
        {
            Amount = amount
        };
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Bucket"/> object and the final amount to be assigned
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="amount">Amount to be assigned to this Bucket</param>
    public static PartialBucketViewModel CreateFromBucketWithAmount(IServiceManager serviceManager,
        IEnumerable<Bucket> availableBuckets, Bucket bucket, decimal amount)
    {
        // TODO: Test what happens, if a Bucket gets assigned back to "No Selection"
        var result = CreateNoSelection(serviceManager, availableBuckets);
        result.Amount = amount;
        result.SelectedBucket = bucket;

        return result;
    }

    /// <summary>
    /// Triggers <see cref="DeleteAssignmentRequest"/>
    /// </summary>
    public void DeleteBucket()
    {
        DeleteAssignmentRequest?.Invoke(this, new DeleteAssignmentRequestArgs(this));
    }
}