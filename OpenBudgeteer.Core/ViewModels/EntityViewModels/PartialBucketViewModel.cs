using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class PartialBucketViewModel : ViewModelBase
{
    #region Properties & Fields
    
    private Guid _selectedBucketId;
    /// <summary>
    /// Database Id of the selected Bucket
    /// </summary>
    public Guid SelectedBucketId 
    { 
        get => _selectedBucketId;
        set => Set(ref _selectedBucketId, value);
    }
    
    private string _selectedBucketName;
    /// <summary>
    /// Name of the selected Bucket
    /// </summary>
    public string SelectedBucketName
    {
        get => _selectedBucketName;
        set => Set(ref _selectedBucketName, value);
    }
    
    private string _selectedBucketColorCode;
    /// <summary>
    /// Name of the color based from <see cref="Color"/>
    /// </summary>
    public string SelectedBucketColorCode 
    { 
        get => _selectedBucketColorCode;
        set => Set(ref _selectedBucketColorCode, value);
    }
    
    /// <summary>
    /// <see cref="Color"/> of the selected Bucket 
    /// </summary>
    public Color SelectedBucketColor => string.IsNullOrEmpty(SelectedBucketColorCode) ? Color.LightGray : Color.FromName(SelectedBucketColorCode);

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
    /// EventHandler which should be invoked once amount assigned to this Bucket has been changed. Can be used
    /// to start further consistency checks and other calculations based on this change 
    /// </summary>
    public event EventHandler<AmountChangedArgs>? AmountChanged;
    /// <summary>
    /// EventHandler which should be invoked in case this instance should start its deletion process. Can be used
    /// in case the way how this instance will be deleted is handled outside of this class
    /// </summary>
    public event EventHandler<DeleteAssignmentRequestArgs>? DeleteAssignmentRequest;
    
    #endregion
    
    #region Constructors

    /// <summary>
    /// Initialize ViewModel based on a existing <see cref="Bucket"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="amount">Amount to be assigned to this Bucket</param>
    protected PartialBucketViewModel(IServiceManager serviceManager, Bucket? bucket, decimal amount) : base(serviceManager)
    {
        _selectedBucketOutput = string.Empty;
        _amount = amount;

        if (bucket == null)
        {
            // Create a "No Selection" Bucket
            var noSelectionBucket = new Bucket()
            {
                Id = Guid.Empty,
                BucketGroupId = Guid.Empty,
                BucketGroup = new BucketGroup(),
                Name = "No Selection"
            };
            
            _selectedBucketId = noSelectionBucket.Id;
            _selectedBucketName = noSelectionBucket.Name ?? string.Empty;
            _selectedBucketColorCode = noSelectionBucket.ColorCode ?? string.Empty;
        }
        else
        {
            _selectedBucketId = bucket.Id;
            _selectedBucketName = bucket.Name ?? string.Empty;
            _selectedBucketColorCode = bucket.ColorCode ?? string.Empty;
        }
        
        
    }

    /// <summary>
    /// Initialize ViewModel with a "No Selection" <see cref="Bucket"/>
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="amount">Amount to be assigned to this Bucket</param>
    public static PartialBucketViewModel CreateNoSelection(IServiceManager serviceManager, decimal amount = 0)
    {
        return new PartialBucketViewModel(serviceManager, null, amount);
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Bucket"/> object and the final amount to be assigned
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="amount">Amount to be assigned to this Bucket</param>
    public static PartialBucketViewModel CreateFromBucket(IServiceManager serviceManager, Bucket bucket, decimal amount)
    {
        return new PartialBucketViewModel(serviceManager, bucket, amount);
    }
    
    #endregion
    
    #region Modification Handler

    /// <summary>
    /// Triggers <see cref="DeleteAssignmentRequest"/>
    /// </summary>
    public void DeleteBucket()
    {
        DeleteAssignmentRequest?.Invoke(this, new DeleteAssignmentRequestArgs(this));
    }

    /// <summary>
    /// Updates ViewModels Bucket data based on passed <see cref="BucketViewModel"/> object
    /// </summary>
    /// <param name="bucketViewModel">Newly selected Bucket</param>
    public void UpdateSelectedBucket(BucketViewModel bucketViewModel)
    {
        SelectedBucketId = bucketViewModel.BucketId;
        SelectedBucketName = bucketViewModel.Name;
        SelectedBucketColorCode = bucketViewModel.ColorCode;
    }
    
    #endregion
}