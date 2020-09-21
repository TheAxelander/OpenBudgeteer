using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Models;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels
{
    public class AccountViewModelItem : ViewModelBase
    {
        private Account _account;
        public Account Account
        {
            get => _account;
            internal set => Set(ref _account, value);
        }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set => Set(ref _balance, value);
        }

        private decimal _in;
        public decimal In
        {
            get => _in;
            set => Set(ref _in, value);
        }

        private decimal _out;
        public decimal Out
        {
            get => _out;
            set => Set(ref _out, value);
        }

        private bool _inModification;
        public bool InModification
        {
            get => _inModification;
            set => Set(ref _inModification, value);
        }

        public event EventHandler<ViewModelReloadEventArgs> ViewModelReloadRequired;
        
        private readonly DbContextOptions<DatabaseContext> _dbOptions;

        public AccountViewModelItem(DbContextOptions<DatabaseContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public AccountViewModelItem(DbContextOptions<DatabaseContext> dbOptions, Account account) : this(dbOptions)
        {
            _account = account;
        }

        public Tuple<bool, string> CreateUpdateAccount()
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var result = Account.AccountId == 0 ? dbContext.CreateAccount(Account) : dbContext.UpdateAccount(Account);
                if (result == 0)
                {
                    return new Tuple<bool, string>(false, "Unable to save changes to database");
                }
                ViewModelReloadRequired?.Invoke(this, new ViewModelReloadEventArgs(this));
                return new Tuple<bool, string>(true, string.Empty);
            }
        }

        public Tuple<bool, string> CloseAccount()
        {
            if (Balance != 0) return new Tuple<bool, string>(false, "Balance must be 0 to close an Account");

            Account.IsActive = 0;
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var result = dbContext.UpdateAccount(Account);
                if (result == 0)
                {
                    return new Tuple<bool, string>(false, "Unable to save changes to database");
                }
                ViewModelReloadRequired?.Invoke(this, new ViewModelReloadEventArgs(this));
                return new Tuple<bool, string>(true, string.Empty);
            }
        }
    }
}
