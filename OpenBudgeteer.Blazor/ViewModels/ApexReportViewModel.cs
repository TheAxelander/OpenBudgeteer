using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Blazor.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.ViewModels;

public class ApexReportViewModel : ReportPageViewModel
{
    public List<ReportRecord> MonthBalances { get; private set; } = new();
    public List<ReportRecord> BankBalances { get; private set; } = new();
    public List<ReportRecord> MonthIncome { get; private set; } = new();
    public List<ReportRecord> MonthExpenses { get; private set; } = new();
    public List<ReportRecord> YearIncome { get; private set; } = new();
    public List<ReportRecord> YearExpenses { get; private set; } = new();
    public List<Tuple<string, List<ReportRecord>>> MonthBucketExpenses { get; private set; } = new();
    
    public ApexReportViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
    }
    
    public async Task LoadDataAsync()
    {
        var loadTasks = new List<Task>()
        {
            LoadMonthBalancesReportAsync(),
            LoadBankBalancesReportAsync(),
            LoadMonthIncomeExpensesReportAsync(),
            LoadYearIncomeExpensesReportAsync(),
            LoadMonthExpensesBucketReportAsync()
        };
        await Task.WhenAll(loadTasks);
    }
    
    private async Task LoadMonthBalancesReportAsync()
    {
        MonthBalances.Clear();
        foreach (var (month, balance) in await LoadMonthBalancesAsync())
        {
            MonthBalances.Add(new ReportRecord(month.ToString("yyyy-MM"), balance));
        }
    }

    private async Task LoadBankBalancesReportAsync()
    {
        BankBalances.Clear();
        foreach (var (month, balance) in await LoadBankBalancesAsync())
        {
            BankBalances.Add(new ReportRecord(month.ToString("yyyy-MM"), balance));
        }
    }
    
    private async Task LoadMonthIncomeExpensesReportAsync()
    {
        MonthIncome.Clear();
        MonthExpenses.Clear();
        foreach (var (month, income, expenses) in await LoadMonthIncomeExpensesAsync())
        {
            MonthIncome.Add(new ReportRecord(month.ToString("yyyy-MM"), income));
            MonthExpenses.Add(new ReportRecord(month.ToString("yyyy-MM"), expenses));
        }
    }
    
    private async Task LoadYearIncomeExpensesReportAsync()
    {
        YearIncome.Clear();
        YearExpenses.Clear();
        foreach (var (year, income, expenses) in await LoadYearIncomeExpensesAsync())
        {
            YearIncome.Add(new ReportRecord(year.ToString("yyyy"), income));
            YearExpenses.Add(new ReportRecord(year.ToString("yyyy"), expenses));
        }
    }
    
    private async Task LoadMonthExpensesBucketReportAsync()
    {
        MonthBucketExpenses.Clear();
        foreach (var item in await LoadMonthExpensesBucketAsync())
        {
            var data = item.MonthlyResults
                .Select(i => new ReportRecord(i.Item1.ToString("yyyy-MM"), i.Item2))
                .ToList();
            MonthBucketExpenses.Add(new(item.BucketName, data));
        }
    }
}