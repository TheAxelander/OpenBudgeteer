using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;

namespace OpenBudgeteer.Core.ViewModels
{
    public class AccountViewModel : ViewModelBase
    {
        private ObservableCollection<AccountViewModelItem> _accounts;
        public ObservableCollection<AccountViewModelItem> Accounts
        {
            get => _accounts;
            set => Set(ref _accounts, value);
        }

        public event EventHandler<ViewModelReloadEventArgs> ViewModelReloadRequired;

        private readonly DbContextOptions<DatabaseContext> _dbOptions;

        public AccountViewModel(DbContextOptions<DatabaseContext> dbOptions)
        {
            _dbOptions = dbOptions;
            Accounts = new ObservableCollection<AccountViewModelItem>();
        }

        public void LoadData()
        {
            Accounts.Clear();

            using (var accountDbContext = new DatabaseContext(_dbOptions))
            {
                foreach (var account in accountDbContext.Account.Where(i => i.IsActive == 1).OrderBy(i => i.Name))
                {
                    var newAccountItem = new AccountViewModelItem(_dbOptions, account);
                    decimal newIn = 0;
                    decimal newOut = 0;

                    using (var transactionDbContext = new DatabaseContext(_dbOptions))
                    {
                        var transactions = transactionDbContext.BankTransaction
                            .Where(i => i.AccountId == account.AccountId);

                        foreach (var transaction in transactions)
                        {
                            if (transaction.Amount > 0)
                                newIn += transaction.Amount;
                            else
                                newOut += transaction.Amount;
                        }
                    }

                    newAccountItem.Balance = newIn + newOut;
                    newAccountItem.In = newIn;
                    newAccountItem.Out = newOut;

                    newAccountItem.ViewModelReloadRequired += (sender, args) => 
                        ViewModelReloadRequired?.Invoke(this, new ViewModelReloadEventArgs(args.ViewModel));
                    Accounts.Add(newAccountItem);
                }
            }
        }

        public AccountViewModelItem PrepareNewAccount()
        {
            var result = new AccountViewModelItem(_dbOptions)
            {
                Account = new Account { AccountId = 0, Name = "New Account", IsActive = 1 },
                Balance = 0,
                In = 0,
                Out = 0
            };
            result.ViewModelReloadRequired += (sender, args) =>
                ViewModelReloadRequired?.Invoke(this, new ViewModelReloadEventArgs(args.ViewModel));
            return result;
        }

        public void CancelEditMode()
        {
            ViewModelReloadRequired?.Invoke(this, new ViewModelReloadEventArgs(this));
        }
    }
}
