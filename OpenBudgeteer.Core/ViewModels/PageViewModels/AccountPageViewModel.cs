using System;
using System.Collections.ObjectModel;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

public class AccountPageViewModel : ViewModelBase
{
    private ObservableCollection<AccountViewModel> _accounts;
    /// <summary>
    /// Collection of ViewModelItems for Model <see cref="AccountViewModel"/>
    /// </summary>
    public ObservableCollection<AccountViewModel> Accounts
    {
        get => _accounts;
        private set => Set(ref _accounts, value);
    }

    public AccountPageViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
        _accounts = new();
    }
    
    /// <summary>
    /// Initialize ViewModel and load data from database
    /// </summary>
    public ViewModelOperationResult LoadData()
    {
        try
        {
            Accounts.Clear();

            foreach (var account in ServiceManager.AccountService.GetActiveAccounts())
            {
                var newAccountItem = AccountViewModel.CreateFromAccount(ServiceManager, account);
                decimal newIn = 0;
                decimal newOut = 0;

                foreach (var transaction in ServiceManager.BankTransactionService.GetFromAccount(account.Id))
                {
                    if (transaction.Amount > 0)
                        newIn += transaction.Amount;
                    else
                        newOut += transaction.Amount;
                }

                newAccountItem.Balance = newIn + newOut;
                newAccountItem.In = newIn;
                newAccountItem.Out = newOut;

                Accounts.Add(newAccountItem);
            }
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
        return new ViewModelOperationResult(true);
    }
}