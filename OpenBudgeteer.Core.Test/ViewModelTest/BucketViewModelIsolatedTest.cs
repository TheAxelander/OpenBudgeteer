using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Core.ViewModels.PageViewModels;
using Xunit;

namespace OpenBudgeteer.Core.Test.ViewModelTest;

[CollectionDefinition("BucketPageViewModelIsolatedTest", DisableParallelization = true)]
public class BucketPageViewModelIsolatedTest : BaseTest
{
    public BucketPageViewModelIsolatedTest() : base(nameof(BucketPageViewModelIsolatedTest))
    {
    }
    
    [Fact]
    public async Task LoadDataAsync_CheckBucketGroupsNamesAndPositions()
    {
        Cleanup();

        var bucketGroups = new List<BucketGroup>()
        {
            new() { Name = "Bucket Group 1", Position = 1 },
            new() { Name = "Bucket Group 3", Position = 2 },
            new() { Name = "Bucket Group 2", Position = 3 }
        };
        foreach (var bucketGroup in bucketGroups)
        {
            ServiceManager.BucketGroupService.Create(bucketGroup);
        }

        var monthSelectorViewModel = new YearMonthSelectorViewModel(ServiceManager);
        var viewModel = new BucketPageViewModel(ServiceManager, monthSelectorViewModel);
        await viewModel.LoadDataAsync();

        Assert.Equal(3, viewModel.BucketGroups.Count);
        Assert.Equal("Bucket Group 1", viewModel.BucketGroups.ElementAt(0).Name);
        Assert.Equal("Bucket Group 3", viewModel.BucketGroups.ElementAt(1).Name);
        Assert.Equal("Bucket Group 2", viewModel.BucketGroups.ElementAt(2).Name);

        Assert.Equal(1, viewModel.BucketGroups.ElementAt(0).Position);
        Assert.Equal(2, viewModel.BucketGroups.ElementAt(1).Position);
        Assert.Equal(3, viewModel.BucketGroups.ElementAt(2).Position);
    }
    
    [Fact]
    public async Task CreateGroup_CheckGroupCreationAndPositions()
    {
        Cleanup();
        
        var bucketGroups = new List<BucketGroup>()
        {
            new() { Name = "Bucket Group 1", Position = 1 },
            new() { Name = "Bucket Group 3", Position = 2 },
            new() { Name = "Bucket Group 2", Position = 3 }
        };
        foreach (var bucketGroup in bucketGroups)
        {
            ServiceManager.BucketGroupService.Create(bucketGroup);
        }

        var monthSelectorViewModel = new YearMonthSelectorViewModel(ServiceManager);
        var viewModel = new BucketPageViewModel(ServiceManager, monthSelectorViewModel);
        await viewModel.LoadDataAsync();
        Assert.Equal(3, viewModel.BucketGroups.Count);
        
        viewModel.CreateEmptyGroup();
        var result = viewModel.NewBucketGroup!.CreateGroup();
        await viewModel.LoadDataAsync();
        
        Assert.True(result.IsSuccessful);
        Assert.Equal(4, viewModel.BucketGroups.Count);
        Assert.Equal("New Bucket Group", viewModel.BucketGroups.ElementAt(0).Name);
        Assert.Equal("Bucket Group 1", viewModel.BucketGroups.ElementAt(1).Name);
        Assert.Equal("Bucket Group 3", viewModel.BucketGroups.ElementAt(2).Name);
        Assert.Equal("Bucket Group 2", viewModel.BucketGroups.ElementAt(3).Name);
        Assert.Equal(1, viewModel.BucketGroups.ElementAt(0).Position);
        Assert.Equal(2, viewModel.BucketGroups.ElementAt(1).Position);
        Assert.Equal(3, viewModel.BucketGroups.ElementAt(2).Position);
        Assert.Equal(4, viewModel.BucketGroups.ElementAt(3).Position);
    }
    
    [Fact]
    public async Task DeleteGroup_CheckGroupDeletionAndPositions()
    {
        Cleanup();
        
        var bucketGroups = new List<BucketGroup>()
        {
            new() { Name = "Bucket Group 1", Position = 1 },
            new() { Name = "Bucket Group 3", Position = 2 },
            new() { Name = "Bucket Group 2", Position = 3 }
        };
        foreach (var bucketGroup in bucketGroups)
        {
            ServiceManager.BucketGroupService.Create(bucketGroup);
        }

        var monthSelectorViewModel = new YearMonthSelectorViewModel(ServiceManager);
        var viewModel = new BucketPageViewModel(ServiceManager, monthSelectorViewModel);
        await viewModel.LoadDataAsync();

        var groupToDelete = viewModel.BucketGroups.ElementAt(1);
        var result = groupToDelete.DeleteGroup();
        
        Assert.True(result.IsSuccessful);
        Assert.True(result.ViewModelReloadRequired);

        await viewModel.LoadDataAsync();
        
        Assert.Equal(2, viewModel.BucketGroups.Count);
        Assert.Equal("Bucket Group 1", viewModel.BucketGroups.ElementAt(0).Name);
        Assert.Equal("Bucket Group 2", viewModel.BucketGroups.ElementAt(1).Name);
        Assert.Equal(1, viewModel.BucketGroups.ElementAt(0).Position);
        Assert.Equal(2, viewModel.BucketGroups.ElementAt(1).Position);
        
        // Reload ViewModel to see if changes has been also reflected onto the database
        await viewModel.LoadDataAsync();
        
        Assert.Equal(2, viewModel.BucketGroups.Count);
        Assert.Equal("Bucket Group 1", viewModel.BucketGroups.ElementAt(0).Name);
        Assert.Equal("Bucket Group 2", viewModel.BucketGroups.ElementAt(1).Name);
        Assert.Equal(1, viewModel.BucketGroups.ElementAt(0).Position);
        Assert.Equal(2, viewModel.BucketGroups.ElementAt(1).Position);
    }
}
