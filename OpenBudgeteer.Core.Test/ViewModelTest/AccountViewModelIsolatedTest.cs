using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels;
using Xunit;

namespace OpenBudgeteer.Core.Test.ViewModelTest;

[CollectionDefinition("AccountViewModelIsolatedTest", DisableParallelization = true)]
public class AccountViewModelIsolatedTest
{
    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    public AccountViewModelIsolatedTest()
    {
        _dbOptions = DbConnector.GetDbContextOptions(nameof(AccountViewModelIsolatedTest));
        DbConnector.CleanupDatabase(nameof(AccountViewModelIsolatedTest));
    }

    [Fact]
    public void LoadData_CheckNameAndLoadOnlyActiveAccounts()
    {
        DbConnector.CleanupDatabase(nameof(AccountViewModelIsolatedTest));
        
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            dbContext.CreateAccounts(new[]
            {
                new Account() {Name = "Test Account1", IsActive = 1},
                new Account() {Name = "Test Account2", IsActive = 1},
                new Account() {Name = "Test Account3", IsActive = 0}
            });
        }

        var viewModel = new AccountViewModel(_dbOptions);
        viewModel.LoadData();

        Assert.Equal(2, viewModel.Accounts.Count);

        var testItem1 = viewModel.Accounts.ElementAt(0);
        var testItem2 = viewModel.Accounts.ElementAt(1);

        Assert.Equal("Test Account1", testItem1.Account.Name);
        Assert.Equal("Test Account2", testItem2.Account.Name);
    }
}
