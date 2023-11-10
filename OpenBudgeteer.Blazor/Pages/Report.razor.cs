using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJs.Blazor.ChartJS.BarChart;
using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Blazor.ViewModels;
using OpenBudgeteer.Core.Data.Contracts.Services;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Report : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;

    private BlazorReportPageViewModel _dataContext = null!;
    private List<Tuple<string, BarConfig>> _monthBucketExpensesConfigsLeft = null!;
    private List<Tuple<string, BarConfig>> _monthBucketExpensesConfigsRight = null!;

    protected override async Task OnInitializedAsync()
    {
        _monthBucketExpensesConfigsLeft = new List<Tuple<string, BarConfig>>();
        _monthBucketExpensesConfigsRight = new List<Tuple<string, BarConfig>>();

        _dataContext = new BlazorReportPageViewModel(ServiceManager);
        await _dataContext.LoadDataAsync();

        var halfIndex = _dataContext.MonthBucketExpensesConfigs.Count / 2;
        _monthBucketExpensesConfigsLeft.AddRange(_dataContext.MonthBucketExpensesConfigs.ToList().GetRange(0,halfIndex));
        _monthBucketExpensesConfigsRight.AddRange(_dataContext.MonthBucketExpensesConfigs.ToList().GetRange(halfIndex,_dataContext.MonthBucketExpensesConfigs.Count - halfIndex));
    }
}