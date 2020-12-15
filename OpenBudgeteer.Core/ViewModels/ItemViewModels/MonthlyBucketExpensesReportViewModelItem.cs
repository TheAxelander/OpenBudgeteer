using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels
{
    /// <summary>
    /// Helper class for Reports showing monthly Bucket expenses
    /// </summary>
    public class MonthlyBucketExpensesReportViewModelItem : ViewModelBase
    {
        private string _bucketName;
        /// <summary>
        /// Name of the Bucket
        /// </summary>
        public string BucketName
        {
            get => _bucketName;
            set => Set(ref _bucketName, value);
        }

        private ObservableCollection<Tuple<DateTime, decimal>> _monthlyResults;
        /// <summary>
        /// Collection of the results for the report
        /// </summary>
        public ObservableCollection<Tuple<DateTime, decimal>> MonthlyResults
        {
            get => _monthlyResults;
            set => Set(ref _monthlyResults, value);
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        public MonthlyBucketExpensesReportViewModelItem()
        {
            MonthlyResults = new ObservableCollection<Tuple<DateTime, decimal>>();
        }
    }
}
