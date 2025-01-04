using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenBudgeteer.Blazor.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.Helper;

namespace OpenBudgeteer.Blazor.ViewModels
{
    public class BlazorBucketStatisticsViewModel : BucketDetailsViewModel
    {
        public List<ReportRecord> MonthBalances { get; private set; } = new();
        public List<ReportRecord> MonthInput { get; private set; } = new();
        public List<ReportRecord> MonthOutput { get; private set; } = new();
        public List<ReportRecord> BucketProgression { get; private set; } = new();
        
        
        public BlazorBucketStatisticsViewModel(IServiceManager serviceManager, YearMonthSelectorViewModel yearMonthViewModel,
            Guid bucketId) : base(serviceManager, yearMonthViewModel, bucketId)
        {
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
            MonthBalances.Clear();
            foreach (var (month, balance) in await LoadBucketMonthBalancesAsync(12))
            {
                MonthBalances.Add(new ReportRecord(month.ToString("yyyy-MM"), balance));
            }
        }

        private async Task LoadMonthInputOutputReportAsync()
        {
            MonthInput.Clear();
            MonthOutput.Clear();
            foreach (var (month, input, output) in await LoadBucketMonthInOutAsync(12))
            {
                MonthInput.Add(new ReportRecord(month.ToString("yyyy-MM"), input));
                MonthOutput.Add(new ReportRecord(month.ToString("yyyy-MM"), output));
            }
        }

        private async Task LoadBucketBalanceProgressionReportAsync()
        {
            BucketProgression.Clear();
            foreach (var (month, balance) in await LoadBucketBalanceProgressionAsync())
            {
                BucketProgression.Add(new ReportRecord(month.ToString("yyyy-MM"), balance));
            }
        }
    }
}
