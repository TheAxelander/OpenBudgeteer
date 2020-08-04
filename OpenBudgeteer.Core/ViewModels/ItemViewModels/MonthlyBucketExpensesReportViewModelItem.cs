using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels
{
    public class MonthlyBucketExpensesReportViewModelItem : ViewModelBase
    {
        private string _bucketName;
        public string BucketName
        {
            get => _bucketName;
            set => Set(ref _bucketName, value);
        }

        private ObservableCollection<Tuple<DateTime, decimal>> _monthlyResults;
        public ObservableCollection<Tuple<DateTime, decimal>> MonthlyResults
        {
            get => _monthlyResults;
            set => Set(ref _monthlyResults, value);
        }

        public MonthlyBucketExpensesReportViewModelItem()
        {
            MonthlyResults = new ObservableCollection<Tuple<DateTime, decimal>>();
        }
    }
}
