using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels;
using Xunit;

namespace OpenBudgeteer.Core.Test.ViewModelTest
{
    public class AccountViewModelTest
    {
        private readonly DbContextOptions<DatabaseContext> _dbOptions;

        public AccountViewModelTest()
        {
            _dbOptions = DbConnector.GetDbContextOptions(nameof(AccountViewModelTest));
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
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var testAccount = new Account() {Name = "Test Account", IsActive = 1};
                dbContext.CreateAccount(testAccount);
                    
                foreach (var transactionAmount in transactionAmounts)
                {
                    dbContext.CreateBankTransaction(
                        new BankTransaction()
                        {
                            AccountId = testAccount.AccountId,
                            TransactionDate = DateTime.Now,
                            Amount = transactionAmount
                        }
                    );
                }
                    
                var viewModel = new AccountViewModel(_dbOptions);
                viewModel.LoadData();
                var testItem1 = viewModel.Accounts
                    .FirstOrDefault(i => i.Account.AccountId == testAccount.AccountId);
                
                Assert.NotNull(testItem1);
                Assert.Equal(expectedBalance, testItem1.Balance);
                Assert.Equal(expectedIn, testItem1.In);
                Assert.Equal(expectedOut, testItem1.Out);
            }
        }
    }
}
