using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;

namespace OpenBudgeteer.Core.ViewModels
{
    public class AccountViewModel : ViewModelBase
    {
        private ObservableCollection<AccountViewModelItem> _accounts;
        /// <summary>
        /// Collection of ViewModelItems for Model <see cref="Account"/>
        /// </summary>
        public ObservableCollection<AccountViewModelItem> Accounts
        {
            get => _accounts;
            set => Set(ref _accounts, value);
        }

        /// <summary>
        /// EventHandler which should be invoked in case the whole ViewModel has to be reloaded
        /// e.g. due to various database record changes 
        /// </summary>
        public event EventHandler<ViewModelReloadEventArgs> ViewModelReloadRequired;

        private readonly DbContextOptions<DatabaseContext> _dbOptions;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="dbOptions">Options to connect to a database</param>
        public AccountViewModel(DbContextOptions<DatabaseContext> dbOptions)
        {
            _dbOptions = dbOptions;
            Accounts = new ObservableCollection<AccountViewModelItem>();
        }

        /// <summary>
        /// Initialize ViewModel and load data from database
        /// </summary>
        public void LoadData()
        {
            Accounts.Clear();

            using (var accountDbContext = new DatabaseContext(_dbOptions))
            {
                foreach (var account in accountDbContext.Account
                    .Where(i => i.IsActive == 1)
                    .OrderBy(i => i.Name))
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

        /// <summary>
        /// Creates an initial <see cref="AccountViewModelItem"/> which can be used for further manipulation
        /// </summary>
        /// <returns>Newly initialized <see cref="AccountViewModelItem"/></returns>
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

        /// <summary>
        /// Forces reload of ViewModel to revoke unsaved changes
        /// </summary>
        public void CancelEditMode()
        {
            ViewModelReloadRequired?.Invoke(this, new ViewModelReloadEventArgs(this));
        }
    }
}
