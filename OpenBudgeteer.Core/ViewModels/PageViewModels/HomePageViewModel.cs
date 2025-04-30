using System;
using System.Collections.ObjectModel;
using System.Linq;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

public class HomePageViewModel : ViewModelBase
{
    private int _totalAccounts;
    /// <summary>
    /// Total amount of all <see cref="Account"/> availabe in database
    /// </summary>
    public int TotalAccounts
    {
        get => _totalAccounts;
        private set => Set(ref _totalAccounts, value);
    }
    
    private int _totalTransactions;
    /// <summary>
    /// Total amount of all <see cref="BankTransaction"/> availabe in database
    /// </summary>
    public int TotalTransactions
    {
        get => _totalTransactions;
        private set => Set(ref _totalTransactions, value);
    }
    
    private int _totalBuckets;
    /// <summary>
    /// Total amount of all <see cref="Bucket"/> availabe in database
    /// </summary>
    public int TotalBuckets
    {
        get => _totalBuckets;
        private set => Set(ref _totalBuckets, value);
    }
    
    private decimal _bankBalanceToday;
    /// <summary>
    /// Total bank balance from today
    /// </summary>
    public decimal BankBalanceToday
    {
        get => _bankBalanceToday;
        private set => Set(ref _bankBalanceToday, value);
    }
    
    private decimal _bankBalanceLastYear;
    /// <summary>
    /// Total bank balance from last year at the same day
    /// </summary>
    public decimal BankBalanceLastYear
    {
        get => _bankBalanceLastYear;
        private set => Set(ref _bankBalanceLastYear, value);
    }
    
    private decimal _bankBalanceDifference;
    /// <summary>
    /// Difference between <see cref="BankBalanceToday"/> and <see cref="_bankBalanceLastYear"/>
    /// </summary>
    public decimal BankBalanceDifference
    {
        get => _bankBalanceDifference;
        private set => Set(ref _bankBalanceDifference, value);
    }
    
    private ObservableCollection<TransactionViewModel> _highestIncomes;
    /// <summary>
    /// Top X Transactions with the highest amount
    /// </summary>
    public ObservableCollection<TransactionViewModel> HighestIncomes
    {
        get => _highestIncomes;
        private set => Set(ref _highestIncomes, value);
    }
    
    private ObservableCollection<TransactionViewModel> _highestExpenses;
    /// <summary>
    /// Top X Transactions with the lowest amount (or highest expense)
    /// </summary>
    public ObservableCollection<TransactionViewModel> HighestExpenses
    {
        get => _highestExpenses;
        private set => Set(ref _highestExpenses, value);
    }
    
    public HomePageViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
        _highestIncomes = new();
        _highestExpenses = new ();
    }

    public ViewModelOperationResult LoadData(int topTransactionsAmount)
    {
        try
        {
            TotalAccounts = ServiceManager.AccountService.GetAll().ToList().Count;
            TotalTransactions = ServiceManager.BankTransactionService.GetAll().ToList().Count;
            TotalBuckets = ServiceManager.BucketService.GetAll().ToList().Count;
            
            BankBalanceToday = ServiceManager.BankTransactionService
                .GetAll(DateOnly.MinValue, DateOnly.FromDateTime(DateTime.Now))
                .Sum(i => i.Amount);
            
            BankBalanceLastYear = ServiceManager.BankTransactionService
                .GetAll(DateOnly.MinValue, DateOnly.FromDateTime(DateTime.Now.AddYears(-1)))
                .Sum(i => i.Amount);
            
            BankBalanceDifference = BankBalanceToday - BankBalanceLastYear;

            var highestBankTransactions = ServiceManager.BudgetedTransactionService
                .GetAllIncome(
                    DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1)),
                    DateOnly.FromDateTime(DateTime.Now))
                .Select(i => i.Transaction)
                .Distinct()
                .OrderByDescending(i => i.Amount)
                .ThenBy(i => i.TransactionDate)
                .Take(topTransactionsAmount)
                .ToList();
            HighestIncomes.Clear();    
            foreach (var bankTransaction in highestBankTransactions)
            {
                HighestIncomes.Add(TransactionViewModel.CreateFromTransactionWithoutBuckets(ServiceManager, bankTransaction));
            }
            
            var lowestBankTransactions = ServiceManager.BudgetedTransactionService
                .GetAllForReporting(
                    DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1)),
                    DateOnly.FromDateTime(DateTime.Now))
                .Where(i => i.Transaction.Amount < 0)
                .Select(i => i.Transaction)
                .Distinct()
                .OrderBy(i => i.Amount)
                .ThenBy(i => i.TransactionDate)
                .Take(topTransactionsAmount)
                .ToList();
            HighestExpenses.Clear();    
            foreach (var bankTransaction in lowestBankTransactions)
            {
                HighestExpenses.Add(TransactionViewModel.CreateFromTransactionWithoutBuckets(ServiceManager, bankTransaction));
            }
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
        return new ViewModelOperationResult(true);
    }
}