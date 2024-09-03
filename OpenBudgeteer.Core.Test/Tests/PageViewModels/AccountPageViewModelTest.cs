using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Test.Mocking;
using OpenBudgeteer.Core.Test.Mocking.Services;
using OpenBudgeteer.Core.ViewModels.PageViewModels;
using Xunit;

namespace OpenBudgeteer.Core.Test.Tests.PageViewModels;

public class AccountPageViewModelTest
{
    [Fact]
    public void LoadData_CheckNameAndLoadOnlyActiveAccounts()
    {
        var serviceManager = new MockServiceManager(new MockDatabase());

        var accounts = new List<Account>()
        {
            new() {Name = "Test Account1", IsActive = 1},
            new() {Name = "Test Account2", IsActive = 1},
            new() {Name = "Test Account3", IsActive = 0}
        };
        foreach (var account in accounts)
        {
            serviceManager.AccountService.Create(account);
        }

        var viewModel = new AccountPageViewModel(serviceManager);
        viewModel.LoadData();

        Assert.Equal(2, viewModel.Accounts.Count);

        var testItem1 = viewModel.Accounts.ElementAt(0);
        var testItem2 = viewModel.Accounts.ElementAt(1);

        Assert.Equal("Test Account1", testItem1.Name);
        Assert.Equal("Test Account2", testItem2.Name);
    }
    
    public static IEnumerable<object[]> TestData_LoadData_CheckTransactionCalculations
    {
        get
        {
            return new[]
            {
                new object[] {new List<decimal> {12.34m, -12.34m, 12.34m}, 12.34m, 24.68m, -12.34m},
                new object[] {new List<decimal> {0}, 0, 0, 0}
            };
        }
    }
    
    [Theory]
    [MemberData(nameof(TestData_LoadData_CheckTransactionCalculations))]
    public void LoadData_CheckTransactionCalculations(
        List<decimal> transactionAmounts, 
        decimal expectedBalance, 
        decimal expectedIn, 
        decimal expectedOut)
    {
        var serviceManager = new MockServiceManager(new MockDatabase());
        
        var testAccount = new Account() { Name = "Test Account", IsActive = 1 };
        serviceManager.AccountService.Create(testAccount);
            
        foreach (var transactionAmount in transactionAmounts)
        {
            serviceManager.BankTransactionService.Create(new BankTransaction()
            {
                AccountId = testAccount.Id,
                TransactionDate = DateTime.Now,
                Amount = transactionAmount
            });
        }
            
        var viewModel = new AccountPageViewModel(serviceManager);
        viewModel.LoadData();
        var testItem1 = viewModel.Accounts
            .FirstOrDefault(i => i.AccountId == testAccount.Id);
        
        Assert.NotNull(testItem1);
        Assert.Equal(expectedBalance, testItem1.Balance);
        Assert.Equal(expectedIn, testItem1.In);
        Assert.Equal(expectedOut, testItem1.Out);
    }
}
