using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.ViewModels.Helper;

public class BucketListingViewModel : ViewModelBase
{
    private ObservableCollection<BucketGroupViewModel> _bucketGroups;
    /// <summary>
    /// Collection of Groups which contains a set of Buckets
    /// </summary>
    public ObservableCollection<BucketGroupViewModel> BucketGroups
    {
        get => _bucketGroups;
        protected set => Set(ref _bucketGroups, value);
    }

    protected readonly YearMonthSelectorViewModel YearMonthViewModel;
    
    protected bool DefaultCollapseState; // Keep Collapse State e.g. after YearMonth change of ViewModel reload

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="yearMonthViewModel">ViewModel instance to handle selection of a year and month</param>
    public BucketListingViewModel(IServiceManager serviceManager, YearMonthSelectorViewModel? yearMonthViewModel) 
        : base(serviceManager)
    {
        _bucketGroups = new ObservableCollection<BucketGroupViewModel>();
        YearMonthViewModel = yearMonthViewModel ?? new YearMonthSelectorViewModel(serviceManager);
    }

    /// <summary>
    /// Initialize ViewModel and load data from database
    /// </summary>
    /// <param name="excludeInactive">Exclude Buckets which are marked as inactive</param>
    /// <param name="includeDefaults">Include system default Buckets like Transfer and Income</param>
    /// <returns>Object which contains information and results of this method</returns>
    public virtual async Task<ViewModelOperationResult> LoadDataAsync(bool excludeInactive = false, bool includeDefaults = false)
    {
        try
        {
            BucketGroups.Clear();
            var bucketGroups = includeDefaults 
                ? ServiceManager.BucketGroupService.GetAllFull().ToList() 
                : ServiceManager.BucketGroupService.GetAll().ToList();

            foreach (var bucketGroup in bucketGroups)
            {
                var newBucketGroup = BucketGroupViewModel.CreateFromBucketGroup(ServiceManager, bucketGroup, YearMonthViewModel.CurrentMonth);
                newBucketGroup.IsCollapsed = DefaultCollapseState;
                var buckets = ServiceManager.BucketGroupService.GetBuckets(bucketGroup.Id);

                var bucketItemTasks = new List<Task<BucketViewModel>>();
                
                foreach (var bucket in buckets)
                {
                    if (excludeInactive && bucket.IsInactive) continue; // Skip as inactive Buckets should be excluded
                    if (bucket.ValidFrom > YearMonthViewModel.CurrentMonth) continue; // Bucket not yet active for selected month
                    if (bucket.IsInactive && bucket.IsInactiveFrom <= YearMonthViewModel.CurrentMonth) continue; // Bucket no longer active for selected month
                    var newBucketItemTask = includeDefaults
                        ? BucketViewModel.CreateForListingAsync(ServiceManager, bucket, YearMonthViewModel.CurrentMonth) // Including defaults, hence no modifications expected
                        : BucketViewModel.CreateForModificationAsync(ServiceManager, bucketGroups, bucket, YearMonthViewModel.CurrentMonth);
                    bucketItemTasks.Add(newBucketItemTask);
                }

                foreach (var bucket in await Task.WhenAll(bucketItemTasks))
                {
                    newBucketGroup.Buckets.Add(bucket);
                }
                newBucketGroup.TotalBalance = newBucketGroup.Buckets.Sum(i => i.Balance);
                newBucketGroup.TotalWant = newBucketGroup.Buckets.Where(i => i.Want > 0).Sum(i => i.Want);
                newBucketGroup.TotalIn = newBucketGroup.Buckets.Sum(i => i.In);
                newBucketGroup.TotalActivity = newBucketGroup.Buckets.Sum(i => i.Activity);
                BucketGroups.Add(newBucketGroup);
            }
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
    }

    /// <summary>
    /// Helper method to set Collapse status for all <see cref="BucketGroup"/>
    /// </summary>
    /// <param name="collapse">New collapse status</param>
    public void ChangeBucketGroupCollapse(bool collapse = true)
    {
        DefaultCollapseState = collapse;
        foreach (var bucketGroup in BucketGroups)
        {
            bucketGroup.IsCollapsed = collapse;
        }
    }
}