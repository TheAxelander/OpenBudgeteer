using ChartJs.Blazor.ChartJS.BarChart;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Enums;
using ChartJs.Blazor.ChartJS.Common.Handlers;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Time;
using ChartJs.Blazor.ChartJS.LineChart;
using ChartJs.Blazor.Util;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBudgeteer.Blazor.ViewModels
{
    public class BlazorBucketStatisticsViewModel : BucketDetailsViewModel
    {
        private BarConfig _monthBalancesConfig;
        public BarConfig MonthBalancesConfig
        {
            get => _monthBalancesConfig;
            set => Set(ref _monthBalancesConfig, value);
        }

        private BarConfig _monthInputOutputConfig;
        public BarConfig MonthInputOutputConfig
        {
            get => _monthInputOutputConfig;
            set => Set(ref _monthInputOutputConfig, value);
        }

        private LineConfig _bucketProgressionConfig;
        public LineConfig BucketProgressionConfig
        {
            get => _bucketProgressionConfig;
            set => Set(ref _bucketProgressionConfig, value);
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

        public BlazorBucketStatisticsViewModel(
            DbContextOptions<DatabaseContext> dbOptions, 
            YearMonthSelectorViewModel yearMonthViewModel,
            Bucket bucket) 
            : base(dbOptions, yearMonthViewModel, bucket)
        {
            MonthBalancesConfig = new BarConfig();
        }

        public async Task LoadDataAsync(bool withMovements)
        {
            var loadTasks = new List<Task>()
            {
                LoadBucketMovementsDataAsync(withMovements),
                LoadMonthBalancesReportAsync(),
                LoadMonthInputOutputReportAsync(),
                LoadBucketBalanceProgressionReportAsync()
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

            var monthBalanceData = await LoadBucketMonthBalancesAsync(12);
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

        private async Task LoadMonthInputOutputReportAsync()
        {
            MonthInputOutputConfig = DefaultBarConfig;
            MonthInputOutputConfig.Options.Title.Text = "Input & Output";

            var incomeResults = new List<object>();
            var expensesResults = new List<object>();

            foreach (var (month, input, output) in await LoadBucketMonthInOutAsync(12))
            {
                incomeResults.Add(input);
                expensesResults.Add(output);
                MonthInputOutputConfig.Data.Labels.Add(month.ToString("MM.yyyy"));
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

            MonthInputOutputConfig.Data.Datasets.Add(incomeBarDataSet);
            MonthInputOutputConfig.Data.Datasets.Add(expensesBarDataSet);
        }

        private async Task LoadBucketBalanceProgressionReportAsync()
        {
            BucketProgressionConfig = DefaultTimeLineConfig;
            BucketProgressionConfig.Options.Title.Text = "Balance Progression";

            var lineDataSet = new LineDataset<TimeTuple<double>>
            {
                BackgroundColor = ColorUtil.FromDrawingColor(Color.Green),
                BorderColor = ColorUtil.FromDrawingColor(Color.LightGreen),
                Label = "Bucket Balance",
                Fill = true,
                BorderWidth = 2,
                PointRadius = 3,
                PointBorderWidth = 1,
                SteppedLine = SteppedLine.False
            };

            foreach (var (month, balance) in await LoadBucketBalanceProgressionAsync())
            {
                lineDataSet.Add(new TimeTuple<double>(new Moment(month), Convert.ToDouble(balance)));
            }

            BucketProgressionConfig.Data.Datasets.Add(lineDataSet);
        }
    }
}
