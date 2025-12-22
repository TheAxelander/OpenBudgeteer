using ApexCharts;
using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Blazor.Common;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Blazor.Shared;
using OpenBudgeteer.Core.Common.AppSettings;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Photino.ViewModels;

namespace OpenBudgeteer.Photino.Pages;

public partial class Report : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private YearMonthSelectorViewModel YearMonthDataContext { get; set; } = null!;
    [Inject] private IMudThemeService MudThemeService { get; set; } = null!;
    [Inject] private IAppSettingService AppSettingService { get; set; } = null!;

    private readonly List<ApexChartWrapper<ApexRecord>> _generalCharts = new();
    private ApexChartWrapper<ApexRecord> AddGeneralChartRef
    {
        set => _generalCharts.Add(value);
    }

    private readonly List<ApexChartWrapper<ApexRecord>> _bucketsCharts = new();
    private ApexChartWrapper<ApexRecord> AddBucketChartRef
    {
        set => _bucketsCharts.Add(value);
    }

    private List<ApexChartWrapper<ApexRecord>> _bucketRemainingBudgetCharts = new();
    private ApexChartWrapper<ApexRecord> AddBucketBudgetChartRef
    {
        set => _bucketRemainingBudgetCharts.Add(value);
    }

    private List<ApexChartWrapper<ApexRecord>> _monthBucketExpensesCharts = new();
    private ApexChartWrapper<ApexRecord> AddMonthlyBucketExpensesChartRef
    {
        set => _monthBucketExpensesCharts.Add(value);
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
            Track = new() { StartAngle = -90, EndAngle = 90 },
            DataLabels = new()
            {
                Name = new() { Show = false },
                Value = new() { FontSize = "22px", Show = true }
            }
        }
    };

    private Legend NoLegend => new()
    {
        Show = false
    };

    private PhotinoApexReportViewModel _dataContext = null!;
    private List<Tuple<string, List<ApexRecord>>> _monthBucketExpensesConfigsLeft = null!;
    private List<Tuple<string, List<ApexRecord>>> _monthBucketExpensesConfigsRight = null!;

    protected override async Task OnInitializedAsync()
    {
        _bucketRemainingBudgetCharts = new();
        _monthBucketExpensesCharts = new();
        _monthBucketExpensesConfigsLeft = new List<Tuple<string, List<ApexRecord>>>();
        _monthBucketExpensesConfigsRight = new List<Tuple<string, List<ApexRecord>>>();

        _dataContext = new PhotinoApexReportViewModel(ServiceManager, AppSettingService, YearMonthDataContext);
        await _dataContext.LoadDataAsync();

        // Monthly Bucket Expenses Tab
        var halfIndex = _dataContext.MonthBucketExpenses.Count / 2;
        _monthBucketExpensesConfigsLeft.AddRange(_dataContext.MonthBucketExpenses.GetRange(0,halfIndex));
        _monthBucketExpensesConfigsRight.AddRange(_dataContext.MonthBucketExpenses.GetRange(halfIndex,_dataContext.MonthBucketExpenses.Count - halfIndex));

        StateHasChanged();

        YearMonthDataContext.SelectedYearMonthChanged += async (sender, args) =>
        {
            /*
             * Clear chart lists before reloading to prevent stale references
             *
             * Currently I'm not clearing these list as this lead to situations where "reused" charts were no longer
             * being updated. Rely on try/catch part in UpdatePieChartAsync() and
             */
            //_bucketRemainingBudgetCharts.Clear();
            //_monthBucketExpensesCharts.Clear();

            await _dataContext.ReloadBucketReportsAsync();
            StateHasChanged();

            /*
             * First wait for render to complete (100ms) then refresh charts. This was so far the most stable combination.
             *
             * Why InvokeAsync:
             * Chart operations like UpdateSeriesAsync() and RenderAsync() use JavaScript interop and modify UI state,
             * which must run on Blazor's rendering thread.
             */
            await Task.Delay(100);
            await InvokeAsync(async () => await RefreshChartsAsync());
        };

        // Handle Chart reload as ViewModel data have been loaded now
        await RefreshChartsAsync();
    }

    private async Task RefreshChartsAsync()
    {
        var tasks = new List<Task>();
        tasks.AddRange(_generalCharts.Select(wrapper => wrapper.RefreshAsync()));
        tasks.AddRange(_bucketsCharts.Select(wrapper => wrapper.RefreshAsync()));
        tasks.AddRange(_bucketRemainingBudgetCharts.Select(wrapper => wrapper.RefreshAsync()));
        tasks.AddRange(_monthBucketExpensesCharts.Select(wrapper => wrapper.RefreshAsync()));
        await Task.WhenAll(tasks);
    }
}
