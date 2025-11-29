using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexCharts;
using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Blazor.Common;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Blazor.ViewModels;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.Helper;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Report : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private YearMonthSelectorViewModel YearMonthDataContext { get; set; } = null!;
    [Inject] private MudThemeService MudThemeService { get; set; } = null!;
    [Inject] private AppSettingService AppSettingService { get; set; } = null!;
    
    private ApexChart<ApexRecord>? _monthBalanceChart;
    private ApexChart<ApexRecord>? _bankBalanceChart;
    private ApexChart<ApexRecord>? _monthIncomeExpensesChart;
    private ApexChart<ApexRecord>? _yearIncomeExpensesChart;
    private ApexChart<ApexRecord>? _balanceDistributionBucketGroupChart;
    private ApexChart<ApexRecord>? _balanceDistributionBucketChart;
    private ApexChart<ApexRecord>? _inDistributionBucketGroupChart;
    private ApexChart<ApexRecord>? _inDistributionBucketChart;
    private ApexChart<ApexRecord>? _activityDistributionBucketGroupChart;
    private ApexChart<ApexRecord>? _activityDistributionBucketChart;
    
    private List<ApexChart<ApexRecord>> _monthBucketExpensesCharts = new();
    private ApexChart<ApexRecord> InjectMonthBucketExpensesChart
    {
        set => _monthBucketExpensesCharts.Add(value);
    }
    private List<ApexChart<ApexRecord>> _bucketRemainingBudgetCharts = new();
    private ApexChart<ApexRecord> InjectBucketRemainingBudgetChart
    {
        set => _bucketRemainingBudgetCharts.Add(value);
    }

    private ApexChartOptions<ApexRecord> GaugeChartOptions => new()
    {
        Chart = GaugeChart,
        Theme = BaseTheme,
        Title = MudH5Title,
        Legend = NoLegend,
        PlotOptions = GaugePlot
    };
    
    private Theme BaseTheme => new()
    {
        Mode = MudThemeService.CurrentThemeSetting.IsDarkMode ? Mode.Dark : Mode.Light, 
        Palette = PaletteType.Palette1
    };

    private Chart GaugeChart => new()
    {
        Height = "350px"
    };

    private Title MudH5Title => new()
    {
        Align = Align.Center,
        Style = new()
        {
            Color = "var(--mud-palette-text-primary)",
            FontFamily = "var(--mud-typography-h5-family)",
            FontSize = "1.5rem",
            FontWeight = "normal"
        },
        OffsetY = 30
    };

    private PlotOptions DonutPlot => new()
    {
        Pie = new()
        {
            Donut = new()
            {
                Labels = new()
                {
                    Total = new()
                    {
                        FontSize = "18px",
                        Formatter =
                            """
                            function (w) {
                                const total = w.globals.seriesTotals.reduce((a, b) => a + b, 0);
                                return parseFloat(total.toFixed(2)); // return number with 2 decimals
                            }
                            """
                    }
                }
            }
        }
    };

    private PlotOptions GaugePlot => new()
    {
        RadialBar = new()
        {
            StartAngle = -90, EndAngle = 90, 
            DataLabels = new()
            {
                Total = new()
                {
                    FontSize = "18px"
                }
            }
        }
    };

    private Legend NoLegend => new()
    {
        Show = false
    };

    private ApexReportViewModel _dataContext = null!;
    private List<Tuple<string, List<ApexRecord>>> _monthBucketExpensesConfigsLeft = null!;
    private List<Tuple<string, List<ApexRecord>>> _monthBucketExpensesConfigsRight = null!;

    protected override async Task OnInitializedAsync()
    {
        _bucketRemainingBudgetCharts = new();
        _monthBucketExpensesCharts = new();
        _monthBucketExpensesConfigsLeft = new List<Tuple<string, List<ApexRecord>>>();
        _monthBucketExpensesConfigsRight = new List<Tuple<string, List<ApexRecord>>>();
    
        _dataContext = new ApexReportViewModel(ServiceManager, AppSettingService, YearMonthDataContext);
        await _dataContext.LoadDataAsync();
        
        // Monthly Bucket Expenses Tab
        var halfIndex = _dataContext.MonthBucketExpenses.Count / 2;
        _monthBucketExpensesConfigsLeft.AddRange(_dataContext.MonthBucketExpenses.GetRange(0,halfIndex));
        _monthBucketExpensesConfigsRight.AddRange(_dataContext.MonthBucketExpenses.GetRange(halfIndex,_dataContext.MonthBucketExpenses.Count - halfIndex));
        
        StateHasChanged();
        
        YearMonthDataContext.SelectedYearMonthChanged += async (sender, args) => 
        {
            await _dataContext.ReloadBucketReportsAsync();
            StateHasChanged();
            
            // Handle Chart reload as ViewModel data have been reloaded now
            await RefreshChartsAsync();
        };
        
        // Handle Chart reload as ViewModel data have been loaded now
        await RefreshChartsAsync();
    }

    private async Task RefreshChartsAsync()
    {
        var tasks = new List<Task>();
        
        // General Tab
        if (_monthBalanceChart is not null) tasks.Add(_monthBalanceChart.UpdateSeriesAsync());
        if (_bankBalanceChart is not null) tasks.Add(_bankBalanceChart.UpdateSeriesAsync());
        if (_monthIncomeExpensesChart is not null) tasks.Add(_monthIncomeExpensesChart.UpdateSeriesAsync());
        if (_yearIncomeExpensesChart is not null) tasks.Add(_yearIncomeExpensesChart.UpdateSeriesAsync());
        
        // Buckets Tab
        if (_balanceDistributionBucketGroupChart is not null) tasks.Add(UpdatePieChartAsync(_balanceDistributionBucketGroupChart));
        if (_balanceDistributionBucketChart is not null) tasks.Add(UpdatePieChartAsync(_balanceDistributionBucketChart));
        if (_inDistributionBucketGroupChart is not null) tasks.Add(UpdatePieChartAsync(_inDistributionBucketGroupChart));
        if (_inDistributionBucketChart is not null) tasks.Add(UpdatePieChartAsync(_inDistributionBucketChart));
        if (_activityDistributionBucketGroupChart is not null) tasks.Add(UpdatePieChartAsync(_activityDistributionBucketGroupChart));
        if (_activityDistributionBucketChart is not null) tasks.Add(UpdatePieChartAsync(_activityDistributionBucketChart));
        
        // Bucket Budgets Tab
        tasks.AddRange(_bucketRemainingBudgetCharts
            .Select(UpdatePieChartAsync));
        
        // Monthly Bucket Expenses Tab
        tasks.AddRange(_monthBucketExpensesCharts
            .Select(monthBucketExpensesChart => monthBucketExpensesChart.UpdateSeriesAsync()));
        
        await Task.WhenAll(tasks);
        
        return;
        
        async Task UpdatePieChartAsync(ApexChart<ApexRecord> chart)
        {
            // UpdateOptionsAsync required here so that labels are properly updated
            // See: https://github.com/apexcharts/Blazor-ApexCharts/issues/351
            await chart.UpdateOptionsAsync(true, true, false);
            await chart.RenderAsync(); // Required so that correct color palette is used
        }
    }
}