using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexCharts;
using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Blazor.Common;
using OpenBudgeteer.Blazor.ViewModels;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Report : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private AppSettings AppSettings { get; set; } = null!;
    
    private ApexChart<ReportRecord>? _monthBalanceChart;
    private ApexChart<ReportRecord>? _bankBalanceChart;
    private ApexChart<ReportRecord>? _monthIncomeExpensesChart;
    private ApexChart<ReportRecord>? _yearIncomeExpensesChart;
    private List<ApexChart<ReportRecord>> _monthBucketExpensesCharts = new();
    private ApexChart<ReportRecord> InjectMonthBucketExpensesChart
    {
        set => _monthBucketExpensesCharts.Add(value);
    }
    private Theme BaseTheme => new()
    {
        Mode = AppSettings.Mode == AppSettings.ThemeMode.Dark ? Mode.Dark : Mode.Light, 
        Palette = PaletteType.Palette1
    };
    
    private ApexReportViewModel _apexContext = null!;
    private List<Tuple<string, List<ReportRecord>>> _monthBucketExpensesConfigsLeft = null!;
    private List<Tuple<string, List<ReportRecord>>> _monthBucketExpensesConfigsRight = null!;

    protected override void OnInitialized()
    {
        _monthBucketExpensesConfigsLeft = new List<Tuple<string, List<ReportRecord>>>();
        _monthBucketExpensesConfigsRight = new List<Tuple<string, List<ReportRecord>>>();
        _monthBucketExpensesCharts = new();
    
        _apexContext = new ApexReportViewModel(ServiceManager);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        
        await _apexContext.LoadDataAsync();
        var halfIndex = _apexContext.MonthBucketExpenses.Count / 2;
        _monthBucketExpensesConfigsLeft.AddRange(_apexContext.MonthBucketExpenses.GetRange(0,halfIndex));
        _monthBucketExpensesConfigsRight.AddRange(_apexContext.MonthBucketExpenses.GetRange(halfIndex,_apexContext.MonthBucketExpenses.Count - halfIndex));
        
        var tasks = new List<Task>();
        if (_monthBalanceChart is not null) tasks.Add(_monthBalanceChart.UpdateSeriesAsync());
        if (_bankBalanceChart is not null) tasks.Add(_bankBalanceChart.UpdateSeriesAsync());
        if (_monthIncomeExpensesChart is not null) tasks.Add(_monthIncomeExpensesChart.UpdateSeriesAsync());
        if (_yearIncomeExpensesChart is not null) tasks.Add(_yearIncomeExpensesChart.UpdateSeriesAsync());
        
        tasks.AddRange(_monthBucketExpensesCharts
            .Select(monthBucketExpensesChart => monthBucketExpensesChart.UpdateSeriesAsync()));

        await Task.WhenAll(tasks);
        StateHasChanged();
    }
}