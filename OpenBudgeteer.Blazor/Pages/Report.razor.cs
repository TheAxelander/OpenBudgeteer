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
    
    private ApexChart<ReportRecord> _monthBalanceChart = null!;
    private ApexChart<ReportRecord> _bankBalanceChart = null!;
    private ApexChart<ReportRecord> _monthIncomeExpensesChart = null!;
    private ApexChart<ReportRecord> _yearIncomeExpensesChart = null!;
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

    protected override async Task OnInitializedAsync()
    {
        _monthBucketExpensesConfigsLeft = new List<Tuple<string, List<ReportRecord>>>();
        _monthBucketExpensesConfigsRight = new List<Tuple<string, List<ReportRecord>>>();
        _monthBucketExpensesCharts = new();
    
        _apexContext = new ApexReportViewModel(ServiceManager);
        await _apexContext.LoadDataAsync();
    
        var halfIndex = _apexContext.MonthBucketExpenses.Count / 2;
        _monthBucketExpensesConfigsLeft.AddRange(_apexContext.MonthBucketExpenses.GetRange(0,halfIndex));
        _monthBucketExpensesConfigsRight.AddRange(_apexContext.MonthBucketExpenses.GetRange(halfIndex,_apexContext.MonthBucketExpenses.Count - halfIndex));
        
        StateHasChanged();
        var tasks = new List<Task>()
        {
            _monthBalanceChart.UpdateSeriesAsync(),
            _bankBalanceChart.UpdateSeriesAsync(),
            _monthIncomeExpensesChart.UpdateSeriesAsync(),
            _yearIncomeExpensesChart.UpdateSeriesAsync()
        };
        tasks.AddRange(_monthBucketExpensesCharts
            .Select(monthBucketExpensesChart => monthBucketExpensesChart.UpdateSeriesAsync()));

        await Task.WhenAll(tasks);
    }
}