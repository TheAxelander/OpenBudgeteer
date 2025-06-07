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
    public List<ApexRecord> MonthBalances { get; private set; } = [];
    public List<ApexRecord> BankBalances { get; private set; } = [];
    public List<ApexRecord> MonthIncome { get; private set; } = [];
    public List<ApexRecord> MonthExpenses { get; private set; } = [];
    public List<ApexRecord> YearIncome { get; private set; } = [];
    public List<ApexRecord> YearExpenses { get; private set; } = [];
    public List<ApexRecord> BalanceDistributionBucketGroup { get; private set; } = [];
    public List<ApexRecord> BalanceDistributionBucket { get; private set; } = [];
    public List<ApexRecord> InDistributionBucketGroup { get; private set; } = [];
    public List<ApexRecord> InDistributionBucket { get; private set; } = [];
    public List<ApexRecord> ActivityDistributionBucketGroup { get; private set; } = [];
    public List<ApexRecord> ActivityDistributionBucket { get; private set; } = [];
    public List<BucketGroupApexRecord> BucketGroupsBucketBudgets { get; private set; } = [];
    public List<Tuple<string, List<ApexRecord>>> MonthBucketExpenses { get; private set; } = [];
    
    private readonly AppSettingService _appSettingService;
    
    public ApexReportViewModel(IServiceManager serviceManager, AppSettingService appSettingService) : base(serviceManager)
    {
        _appSettingService = appSettingService;
    }
    
    public async Task LoadDataAsync()
    {
        var loadTasks = new List<Task>
        {
            LoadMonthBalancesReportAsync(_appSettingService.CurrentSettings.ReportCounts.MonthBalance),
            LoadBankBalancesReportAsync(_appSettingService.CurrentSettings.ReportCounts.BankBalance),
            LoadMonthIncomeExpensesReportAsync(_appSettingService.CurrentSettings.ReportCounts.MonthIncomeExpenses),
            LoadYearIncomeExpensesReportAsync(_appSettingService.CurrentSettings.ReportCounts.YearIncomeExpenses),
            LoadMonthExpensesBucketReportAsync(_appSettingService.CurrentSettings.ReportCounts.MonthBucketExpenses),
            LoadBucketReportsAsync()
        };
        await Task.WhenAll(loadTasks);
    }
    
    private async Task LoadMonthBalancesReportAsync(int months)
    {
        MonthBalances.Clear();
        foreach (var (month, balance) in await LoadMonthBalancesAsync(months))
        {
            MonthBalances.Add(new ApexRecord(month.ToString("yyyy-MM"), balance));
        }
    }

    private async Task LoadBankBalancesReportAsync(int months)
    {
        BankBalances.Clear();
        foreach (var (month, balance) in await LoadBankBalancesAsync(months))
        {
            BankBalances.Add(new ApexRecord(month.ToString("yyyy-MM"), balance));
        }
    }
    
    private async Task LoadMonthIncomeExpensesReportAsync(int months)
    {
        MonthIncome.Clear();
        MonthExpenses.Clear();
        foreach (var (month, income, expenses) in await LoadMonthIncomeExpensesAsync(months))
        {
            MonthIncome.Add(new ApexRecord(month.ToString("yyyy-MM"), income));
            MonthExpenses.Add(new ApexRecord(month.ToString("yyyy-MM"), expenses));
        }
    }
    
    private async Task LoadYearIncomeExpensesReportAsync(int years)
    {
        YearIncome.Clear();
        YearExpenses.Clear();
        foreach (var (year, income, expenses) in await LoadYearIncomeExpensesAsync(years))
        {
            YearIncome.Add(new ApexRecord(year.ToString("yyyy"), income));
            YearExpenses.Add(new ApexRecord(year.ToString("yyyy"), expenses));
        }
    }
    
    private async Task LoadMonthExpensesBucketReportAsync(int months)
    {
        MonthBucketExpenses.Clear();
        foreach (var item in await LoadMonthExpensesBucketAsync(months))
        {
            var data = item.MonthlyResults
                .Select(i => new ApexRecord(i.Item1.ToString("yyyy-MM"), i.Item2))
                .ToList();
            MonthBucketExpenses.Add(new(item.BucketName, data));
        }
    }

    private async Task LoadBucketReportsAsync()
    {
        BalanceDistributionBucketGroup.Clear();
        BalanceDistributionBucket.Clear();
        InDistributionBucketGroup.Clear();
        InDistributionBucket.Clear();
        ActivityDistributionBucketGroup.Clear();
        ActivityDistributionBucket.Clear();
        BucketGroupsBucketBudgets.Clear();

        var bucketReportResult = await LoadBucketStatisticsAsync();

        BalanceDistributionBucketGroup.AddRange(bucketReportResult.BalancesPerBucketGroup
            .Select(i => new ApexRecord(i.Item1, i.Item2)));
        BalanceDistributionBucket.AddRange(bucketReportResult.BalancesPerBucket
            .Select(i => new ApexRecord(i.Item1, i.Item2)));
        InDistributionBucketGroup.AddRange(bucketReportResult.InPerBucketGroup
            .Select(i => new ApexRecord(i.Item1, i.Item2)));
        InDistributionBucket.AddRange(bucketReportResult.InPerBucket
            .Select(i => new ApexRecord(i.Item1, i.Item2)));
        ActivityDistributionBucketGroup.AddRange(bucketReportResult.ActivityPerBucketGroup
            .Select(i => new ApexRecord(i.Item1, i.Item2 * -1)));
        ActivityDistributionBucket.AddRange(bucketReportResult.ActivityPerBucket
            .Select(i => new ApexRecord(i.Item1, i.Item2 * -1)));

        foreach (var (bucketGroup, buckets) in bucketReportResult.BudgetConsumptionPerBucket)
        {
            BucketGroupsBucketBudgets.Add(new(
                bucketGroup, 
                buckets
                    .Select(i => new BucketApexRecord(i.Item1, ConvertToGaugeItemSource(i)))
                    .ToList()
                )
            );
        }
        return;

        List<ApexRecord> ConvertToGaugeItemSource(Tuple<string, decimal> source)
        {
            return [new ApexRecord("Remaining", Math.Round(source.Item2, 0))];
        }
    }
}