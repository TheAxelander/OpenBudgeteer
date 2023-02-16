using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;

namespace OpenBudgeteer.Core.ViewModels;

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
    public ViewModelOperationResult LoadData()
    {
        try
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
                        foreach (var transaction in transactionDbContext.BankTransaction
                                     .Where(i => i.AccountId == account.AccountId))
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

                    Accounts.Add(newAccountItem);
                }
            }
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Creates an initial <see cref="AccountViewModelItem"/> which can be used for further manipulation
    /// </summary>
    /// <returns>Newly initialized <see cref="AccountViewModelItem"/></returns>
    public AccountViewModelItem PrepareNewAccount()
    {
        var result = new AccountViewModelItem(_dbOptions)
        {
            Account = new Account { AccountId = Guid.Empty, Name = "New Account", IsActive = 1 },
            Balance = 0,
            In = 0,
            Out = 0
        };
        return result;
    }
}
