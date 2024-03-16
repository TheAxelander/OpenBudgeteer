using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.PageViewModels;
using Xunit;

namespace OpenBudgeteer.Core.Test.ViewModelTest;

public class AccountViewModelTest : BaseTest
{
    public AccountViewModelTest() : base(nameof(AccountViewModelTest))
    {
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
        var testAccount = new Account() {Name = "Test Account", IsActive = 1};
        ServiceManager.AccountService.Create(testAccount);
            
        foreach (var transactionAmount in transactionAmounts)
        {
            ServiceManager.BankTransactionService.Create(new BankTransaction()
            {
                AccountId = testAccount.Id,
                TransactionDate = DateTime.Now,
                Amount = transactionAmount
            });
        }
            
        var viewModel = new AccountPageViewModel(ServiceManager);
        viewModel.LoadData();
        var testItem1 = viewModel.Accounts
            .FirstOrDefault(i => i.AccountId == testAccount.Id);
        
        Assert.NotNull(testItem1);
        Assert.Equal(expectedBalance, testItem1.Balance);
        Assert.Equal(expectedIn, testItem1.In);
        Assert.Equal(expectedOut, testItem1.Out);
    }
}
