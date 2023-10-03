using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ChartJs.Blazor.ChartJS.BarChart;
using ChartJs.Blazor.ChartJS.BarChart.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Enums;
using ChartJs.Blazor.ChartJS.Common.Handlers;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Time;
using ChartJs.Blazor.ChartJS.LineChart;
using ChartJs.Blazor.Util;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.ViewModels;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Blazor.ViewModels;

public class BlazorReportViewModel : ReportViewModel
{
    private BarConfig _monthBalancesConfig;
    public BarConfig MonthBalancesConfig
    {
        get => _monthBalancesConfig;
        set => Set(ref _monthBalancesConfig, value);
    }

    private LineConfig _bankBalancesConfig;
    public LineConfig BankBalancesConfig
    {
        get => _bankBalancesConfig;
        set => Set(ref _bankBalancesConfig, value);
    }

    private BarConfig _monthIncomeExpensesConfig;
    public BarConfig MonthIncomeExpensesConfig
    {
        get => _monthIncomeExpensesConfig;
        set => Set(ref _monthIncomeExpensesConfig, value);
    }

    private BarConfig _yearIncomeExpensesConfig;
    public BarConfig YearIncomeExpensesConfig
    {
        get => _yearIncomeExpensesConfig;
        set => Set(ref _yearIncomeExpensesConfig, value);
    }

    private ObservableCollection<Tuple<string, BarConfig>> _monthBucketExpensesConfigs;
    public ObservableCollection<Tuple<string, BarConfig>> MonthBucketExpensesConfigs
    {
        get => _monthBucketExpensesConfigs;
        set => Set(ref _monthBucketExpensesConfigs, value);
    }

    protected BarConfig DefaultBarConfig =>
        new BarConfig()
        {
            Options = new BarOptions
            {
                Title = new OptionsTitle
                {
                    Display = false
                },
                Animation = new ArcAnimation
                {
                    AnimateRotate = true,
                    AnimateScale = true
                },
                Legend = new Legend
                {
                    Display = false
                }
            }
        };
    
    protected BarConfig DefaultZeroBasedBarConfig
    {
        get
        {
            var result = this.DefaultBarConfig;
            result.Options.Scales = new BarScales
            {
                YAxes = new List<CartesianAxis>
                {
                    new BarLinearCartesianAxis
                    {
                        Ticks = new LinearCartesianTicks
                        {
                            Min = 0
                        }
                    }
                }
            };
            return result;
        }
    }
    
    protected LineConfig DefaultTimeLineConfig =>
        new LineConfig
        {
            Options = new LineOptions
            {
                Title = new OptionsTitle
                {
                    Display = false
                },
                Legend = new Legend
                {
                    Display = false
                },
                Tooltips = new Tooltips
                {
                    Mode = InteractionMode.Nearest,
                    Intersect = false
                },
                Scales = new Scales
                {
                    xAxes = new List<CartesianAxis>
                    {
                        new TimeAxis
                        {
                            Distribution = TimeDistribution.Linear,
                            Ticks = new TimeTicks
                            {
                                Source = TickSource.Data
                            },
                            Time = new TimeOptions
                            {
                                Unit = TimeMeasurement.Month,
                                Round = TimeMeasurement.Month,
                                TooltipFormat = "MM.YYYY",
                                DisplayFormats = TimeDisplayFormats.DE_CH
                            }
                        }
                    }
                },
                Hover = new LineOptionsHover
                {
                    Intersect = true,
                    Mode = InteractionMode.Y
                }
            }
        };

    public BlazorReportViewModel(DbContextOptions<DatabaseContext> dbOptions) : base(dbOptions)
    {
        MonthBalancesConfig = new BarConfig();
        BankBalancesConfig = new LineConfig();
        MonthIncomeExpensesConfig = new BarConfig();
        YearIncomeExpensesConfig = new BarConfig();
        MonthBucketExpensesConfigs = new ObservableCollection<Tuple<string, BarConfig>>();
    }

    public async Task LoadDataAsync()
    {
        var loadTasks = new List<Task>()
        {
            LoadMonthBalancesReportAsync(),
            LoadMonthIncomeExpensesReportAsync(),
            LoadYearIncomeExpensesReportAsync(),
            LoadBankBalancesReportAsync(),
            LoadMonthExpensesBucketReportAsync()
        };
        await Task.WhenAll(loadTasks);
    }

    private async Task LoadMonthBalancesReportAsync()
    {
        MonthBalancesConfig = DefaultBarConfig;
        MonthBalancesConfig.Options.Title.Text = "Month Balances";

        var backgroundColors = new List<string>();
        var hoverColors = new List<string>();
        var data = new List<object>();

        var monthBalanceData = await LoadMonthBalancesAsync();
        foreach (var balanceData in monthBalanceData)
        {
            data.Add(balanceData.Item2);
            MonthBalancesConfig.Data.Labels.Add(balanceData.Item1.ToString("MM.yyyy"));
            if (balanceData.Item2 < 0)
            {
                backgroundColors.Add(ColorUtil.FromDrawingColor(Color.DarkRed));
                hoverColors.Add(ColorUtil.FromDrawingColor(Color.LightCoral));
            }
            else
            {
                backgroundColors.Add(ColorUtil.FromDrawingColor(Color.Green));
                hoverColors.Add(ColorUtil.FromDrawingColor(Color.LightGreen));
            }
        }

        var barDataSet = new BarDataset<object>
        {
            BackgroundColor = backgroundColors.ToArray(),
            BorderWidth = 0,
            HoverBackgroundColor = hoverColors.ToArray(),
            HoverBorderWidth = 0
        };

        barDataSet.AddRange(data);

        MonthBalancesConfig.Data.Datasets.Add(barDataSet);
    }

    private async Task LoadMonthIncomeExpensesReportAsync()
    {
        MonthIncomeExpensesConfig = DefaultZeroBasedBarConfig;
        MonthIncomeExpensesConfig.Options.Title.Text = "Income & Expenses per Month";

        var incomeResults = new List<object>();
        var expensesResults = new List<object>();
        
        foreach (var (month, income, expenses) in await LoadMonthIncomeExpensesAsync())
        {
            incomeResults.Add(income);
            expensesResults.Add(expenses);
            MonthIncomeExpensesConfig.Data.Labels.Add(month.ToString("MM.yyyy"));
        }

        var incomeBarDataSet = new BarDataset<object>
        {
            BackgroundColor = ColorUtil.FromDrawingColor(Color.Green),
            BorderWidth = 0,
            HoverBackgroundColor = ColorUtil.FromDrawingColor(Color.LightGreen),
            HoverBorderWidth = 0
        };

        var expensesBarDataSet = new BarDataset<object>
        {
            BackgroundColor = ColorUtil.FromDrawingColor(Color.DarkRed),
            BorderWidth = 0,
            HoverBackgroundColor = ColorUtil.FromDrawingColor(Color.LightCoral),
            HoverBorderWidth = 0
        };

        incomeBarDataSet.AddRange(incomeResults);
        expensesBarDataSet.AddRange(expensesResults);

        MonthIncomeExpensesConfig.Data.Datasets.Add(incomeBarDataSet);
        MonthIncomeExpensesConfig.Data.Datasets.Add(expensesBarDataSet);
    }

    private async Task LoadYearIncomeExpensesReportAsync()
    {
        YearIncomeExpensesConfig = DefaultZeroBasedBarConfig;
        YearIncomeExpensesConfig.Options.Title.Text = "Income & Expenses per Year";

        var incomeResults = new List<object>();
        var expensesResults = new List<object>();

        foreach (var (month, income, expenses) in await LoadYearIncomeExpensesAsync())
        {
            incomeResults.Add(income);
            expensesResults.Add(expenses);
            YearIncomeExpensesConfig.Data.Labels.Add(month.ToString("yyyy"));
        }

        var incomeBarDataSet = new BarDataset<object>
        {
            BackgroundColor = ColorUtil.FromDrawingColor(Color.Green),
            BorderWidth = 0,
            HoverBackgroundColor = ColorUtil.FromDrawingColor(Color.LightGreen),
            HoverBorderWidth = 0
        };

        var expensesBarDataSet = new BarDataset<object>
        {
            BackgroundColor = ColorUtil.FromDrawingColor(Color.DarkRed),
            BorderWidth = 0,
            HoverBackgroundColor = ColorUtil.FromDrawingColor(Color.LightCoral),
            HoverBorderWidth = 0
        };

        incomeBarDataSet.AddRange(incomeResults);
        expensesBarDataSet.AddRange(expensesResults);

        YearIncomeExpensesConfig.Data.Datasets.Add(incomeBarDataSet);
        YearIncomeExpensesConfig.Data.Datasets.Add(expensesBarDataSet);
    }

    private async Task LoadBankBalancesReportAsync()
    {
        BankBalancesConfig = DefaultTimeLineConfig;
        BankBalancesConfig.Options.Title.Text = "Bank Balances";

        var lineDataSet = new LineDataset<TimeTuple<double>>
        {
            BackgroundColor = ColorUtil.FromDrawingColor(Color.Green),
            BorderColor = ColorUtil.FromDrawingColor(Color.LightGreen),
            Label = "Bank Balance",
            Fill = true,
            BorderWidth = 2,
            PointRadius = 3,
            PointBorderWidth = 1,
            SteppedLine = SteppedLine.False
        };

        foreach (var (month, balance) in await LoadBankBalancesAsync())
        {
            lineDataSet.Add(new TimeTuple<double>(new Moment(month), Convert.ToDouble(balance)));
        }

        // Set yAxes min value to 0 in case there is no negative Bank Balance existing
        if (!lineDataSet.Data.Any(i => i.YValue < 0))
        {
            BankBalancesConfig.Options.Scales.yAxes = new List<CartesianAxis>
            {
                new LinearCartesianAxis
                {
                    Ticks = new LinearCartesianTicks
                    {
                        Min = 0
                    }
                }
            };
        }

        BankBalancesConfig.Data.Datasets.Add(lineDataSet);
    }

    private async Task LoadMonthExpensesBucketReportAsync()
    {
        MonthBucketExpensesConfigs.Clear();
        foreach (var result in await LoadMonthExpensesBucketAsync())
        {
            var newConfig = DefaultZeroBasedBarConfig;
            newConfig.Options.Title.Display = false;

            var expensesResults = new List<object>();
            foreach (var monthlyResult in result.MonthlyResults)
            {
                expensesResults.Add(monthlyResult.Item2);
                newConfig.Data.Labels.Add(monthlyResult.Item1.ToString("MM.yyyy"));
            }

            var newDataSet = new BarDataset<object>()
            {
                BackgroundColor = ColorUtil.FromDrawingColor(Color.DarkRed),
                BorderWidth = 0,
                HoverBackgroundColor = ColorUtil.FromDrawingColor(Color.LightCoral),
                HoverBorderWidth = 0
            };
            newDataSet.AddRange(expensesResults);
            newConfig.Data.Datasets.Add(newDataSet);
            
            MonthBucketExpensesConfigs.Add(new Tuple<string, BarConfig>(
                result.BucketName,
                newConfig));
        }
    }
}
