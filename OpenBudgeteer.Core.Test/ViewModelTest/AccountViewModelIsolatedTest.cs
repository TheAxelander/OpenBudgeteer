using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.PageViewModels;
using Xunit;

namespace OpenBudgeteer.Core.Test.ViewModelTest;

[CollectionDefinition("AccountViewModelIsolatedTest", DisableParallelization = true)]
public class AccountViewModelIsolatedTest : BaseTest
{
    public AccountViewModelIsolatedTest() : base(nameof(AccountViewModelIsolatedTest))
    {
    }

    [Fact]
    public void LoadData_CheckNameAndLoadOnlyActiveAccounts()
    {
        Cleanup();

        var accounts = new List<Account>()
        {
            new() {Name = "Test Account1", IsActive = 1},
            new() {Name = "Test Account2", IsActive = 1},
            new() {Name = "Test Account3", IsActive = 0}
        };
        foreach (var account in accounts)
        {
            ServiceManager.AccountService.Create(account);
        }

        var viewModel = new AccountPageViewModel(ServiceManager);
        viewModel.LoadData();

        Assert.Equal(2, viewModel.Accounts.Count);

        var testItem1 = viewModel.Accounts.ElementAt(0);
        var testItem2 = viewModel.Accounts.ElementAt(1);

        Assert.Equal("Test Account1", testItem1.Name);
        Assert.Equal("Test Account2", testItem2.Name);
    }
}
