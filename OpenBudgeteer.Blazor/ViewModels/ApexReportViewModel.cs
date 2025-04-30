using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Blazor.Common;
using OpenBudgeteer.Blazor.Common.Services;
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
    
    private readonly AppSettingService _appSettingService;
    
    public ApexReportViewModel(IServiceManager serviceManager, AppSettingService appSettingService) : base(serviceManager)
    {
        _appSettingService = appSettingService;
    }
    
    public async Task LoadDataAsync()
    {
        var loadTasks = new List<Task>()
        {
            LoadMonthBalancesReportAsync(_appSettingService.CurrentSettings.ReportCounts.MonthBalance),
            LoadBankBalancesReportAsync(_appSettingService.CurrentSettings.ReportCounts.BankBalance),
            LoadMonthIncomeExpensesReportAsync(_appSettingService.CurrentSettings.ReportCounts.MonthIncomeExpenses),
            LoadYearIncomeExpensesReportAsync(_appSettingService.CurrentSettings.ReportCounts.YearIncomeExpenses),
            LoadMonthExpensesBucketReportAsync(_appSettingService.CurrentSettings.ReportCounts.MonthBucketExpenses)
        };
        await Task.WhenAll(loadTasks);
    }
    
    private async Task LoadMonthBalancesReportAsync(int months)
    {
        MonthBalances.Clear();
        foreach (var (month, balance) in await LoadMonthBalancesAsync(months))
        {
            MonthBalances.Add(new ReportRecord(month.ToString("yyyy-MM"), balance));
        }
    }

    private async Task LoadBankBalancesReportAsync(int months)
    {
        BankBalances.Clear();
        foreach (var (month, balance) in await LoadBankBalancesAsync(months))
        {
            BankBalances.Add(new ReportRecord(month.ToString("yyyy-MM"), balance));
        }
    }
    
    private async Task LoadMonthIncomeExpensesReportAsync(int months)
    {
        MonthIncome.Clear();
        MonthExpenses.Clear();
        foreach (var (month, income, expenses) in await LoadMonthIncomeExpensesAsync(months))
        {
            MonthIncome.Add(new ReportRecord(month.ToString("yyyy-MM"), income));
            MonthExpenses.Add(new ReportRecord(month.ToString("yyyy-MM"), expenses));
        }
    }
    
    private async Task LoadYearIncomeExpensesReportAsync(int years)
    {
        YearIncome.Clear();
        YearExpenses.Clear();
        foreach (var (year, income, expenses) in await LoadYearIncomeExpensesAsync(years))
        {
            YearIncome.Add(new ReportRecord(year.ToString("yyyy"), income));
            YearExpenses.Add(new ReportRecord(year.ToString("yyyy"), expenses));
        }
    }
    
    private async Task LoadMonthExpensesBucketReportAsync(int months)
    {
        MonthBucketExpenses.Clear();
        foreach (var item in await LoadMonthExpensesBucketAsync(months))
        {
            var data = item.MonthlyResults
                .Select(i => new ReportRecord(i.Item1.ToString("yyyy-MM"), i.Item2))
                .ToList();
            MonthBucketExpenses.Add(new(item.BucketName, data));
        }
    }
}