using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels;
using Xunit;

namespace OpenBudgeteer.Core.Test.ViewModelTest;

[CollectionDefinition("BucketViewModelIsolatedTest", DisableParallelization = true)]
public class BucketViewModelIsolatedTest
{
    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    public BucketViewModelIsolatedTest()
    {
        _dbOptions = DbConnector.GetDbContextOptions(nameof(BucketViewModelIsolatedTest));
        DbConnector.CleanupDatabase(nameof(BucketViewModelIsolatedTest));
    }
    
    [Fact]
    public async Task LoadDataAsync_CheckBucketGroupsNamesAndPositions()
    {
        DbConnector.CleanupDatabase(nameof(BucketViewModelIsolatedTest));
        
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            dbContext.CreateBucketGroups(new[]
            {
                new BucketGroup() { Name = "Bucket Group 1", Position = 1},
                new BucketGroup() { Name = "Bucket Group 2", Position = 3},
                new BucketGroup() { Name = "Bucket Group 3", Position = 2}
            });
        }

        var monthSelectorViewModel = new YearMonthSelectorViewModel();
        var viewModel = new BucketViewModel(_dbOptions, monthSelectorViewModel);
        await viewModel.LoadDataAsync();

        Assert.Equal(3, viewModel.BucketGroups.Count);
        Assert.Equal("Bucket Group 1", viewModel.BucketGroups.ElementAt(0).BucketGroup.Name);
        Assert.Equal("Bucket Group 3", viewModel.BucketGroups.ElementAt(1).BucketGroup.Name);
        Assert.Equal("Bucket Group 2", viewModel.BucketGroups.ElementAt(2).BucketGroup.Name);

        Assert.Equal(1, viewModel.BucketGroups.ElementAt(0).BucketGroup.Position);
        Assert.Equal(2, viewModel.BucketGroups.ElementAt(1).BucketGroup.Position);
        Assert.Equal(3, viewModel.BucketGroups.ElementAt(2).BucketGroup.Position);
    }
    
    [Fact]
    public async Task CreateGroup_CheckGroupCreationAndPositions()
    {
        DbConnector.CleanupDatabase(nameof(BucketViewModelIsolatedTest));
        
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            dbContext.CreateBucketGroups(new[]
            {
                new BucketGroup() { Name = "Bucket Group 1", Position = 1 },
                new BucketGroup() { Name = "Bucket Group 2", Position = 3 },
                new BucketGroup() { Name = "Bucket Group 3", Position = 2 }
            });
        }

        var monthSelectorViewModel = new YearMonthSelectorViewModel();
        var viewModel = new BucketViewModel(_dbOptions, monthSelectorViewModel);
        await viewModel.LoadDataAsync();

        var result = viewModel.CreateGroup();
        
        Assert.True(result.IsSuccessful);
        Assert.Equal(4, viewModel.BucketGroups.Count);
        Assert.Equal("New Bucket Group", viewModel.BucketGroups.ElementAt(0).BucketGroup.Name);
        Assert.Equal("Bucket Group 1", viewModel.BucketGroups.ElementAt(1).BucketGroup.Name);
        Assert.Equal("Bucket Group 3", viewModel.BucketGroups.ElementAt(2).BucketGroup.Name);
        Assert.Equal("Bucket Group 2", viewModel.BucketGroups.ElementAt(3).BucketGroup.Name);
        Assert.Equal(1, viewModel.BucketGroups.ElementAt(0).BucketGroup.Position);
        Assert.Equal(2, viewModel.BucketGroups.ElementAt(1).BucketGroup.Position);
        Assert.Equal(3, viewModel.BucketGroups.ElementAt(2).BucketGroup.Position);
        Assert.Equal(4, viewModel.BucketGroups.ElementAt(3).BucketGroup.Position);
        Assert.True(viewModel.BucketGroups.First().InModification);
        
        // Reload ViewModel to see if changes has been also reflected onto the database
        await viewModel.LoadDataAsync();

        Assert.True(result.IsSuccessful);
        Assert.Equal(4, viewModel.BucketGroups.Count);
        Assert.Equal("New Bucket Group", viewModel.BucketGroups.ElementAt(0).BucketGroup.Name);
        Assert.Equal("Bucket Group 1", viewModel.BucketGroups.ElementAt(1).BucketGroup.Name);
        Assert.Equal("Bucket Group 3", viewModel.BucketGroups.ElementAt(2).BucketGroup.Name);
        Assert.Equal("Bucket Group 2", viewModel.BucketGroups.ElementAt(3).BucketGroup.Name);
        Assert.Equal(1, viewModel.BucketGroups.ElementAt(0).BucketGroup.Position);
        Assert.Equal(2, viewModel.BucketGroups.ElementAt(1).BucketGroup.Position);
        Assert.Equal(3, viewModel.BucketGroups.ElementAt(2).BucketGroup.Position);
        Assert.Equal(4, viewModel.BucketGroups.ElementAt(3).BucketGroup.Position);
        Assert.False(viewModel.BucketGroups.First().InModification);
    }
    
    [Fact]
    public async Task DeleteGroup_CheckGroupDeletionAndPositions()
    {
        DbConnector.CleanupDatabase(nameof(BucketViewModelIsolatedTest));
        
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            dbContext.CreateBucketGroups(new[]
            {
                new BucketGroup() { Name = "Bucket Group 1", Position = 1},
                new BucketGroup() { Name = "Bucket Group 2", Position = 3},
                new BucketGroup() { Name = "Bucket Group 3", Position = 2}
            });
        }

        var monthSelectorViewModel = new YearMonthSelectorViewModel();
        var viewModel = new BucketViewModel(_dbOptions, monthSelectorViewModel);
        await viewModel.LoadDataAsync();

        var groupToDelete = viewModel.BucketGroups.ElementAt(1);
        var result = viewModel.DeleteGroup(groupToDelete);
        
        Assert.True(result.IsSuccessful);
        Assert.True(result.ViewModelReloadRequired);

        await viewModel.LoadDataAsync();
        
        Assert.Equal(2, viewModel.BucketGroups.Count);
        Assert.Equal("Bucket Group 1", viewModel.BucketGroups.ElementAt(0).BucketGroup.Name);
        Assert.Equal("Bucket Group 2", viewModel.BucketGroups.ElementAt(1).BucketGroup.Name);
        Assert.Equal(1, viewModel.BucketGroups.ElementAt(0).BucketGroup.Position);
        Assert.Equal(2, viewModel.BucketGroups.ElementAt(1).BucketGroup.Position);
        
        // Reload ViewModel to see if changes has been also reflected onto the database
        await viewModel.LoadDataAsync();
        
        Assert.Equal(2, viewModel.BucketGroups.Count);
        Assert.Equal("Bucket Group 1", viewModel.BucketGroups.ElementAt(0).BucketGroup.Name);
        Assert.Equal("Bucket Group 2", viewModel.BucketGroups.ElementAt(1).BucketGroup.Name);
        Assert.Equal(1, viewModel.BucketGroups.ElementAt(0).BucketGroup.Position);
        Assert.Equal(2, viewModel.BucketGroups.ElementAt(1).BucketGroup.Position);
    }
}
