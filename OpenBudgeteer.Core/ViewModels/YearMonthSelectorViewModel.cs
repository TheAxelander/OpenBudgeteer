using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using OpenBudgeteer.Core.Common.EventClasses;

namespace OpenBudgeteer.Core.ViewModels
{
    public class YearMonthSelectorViewModel : ViewModelBase
    {
        public YearMonthSelectorViewModel()
        {
            Months = new ObservableCollection<int>();
            for (var i = 1; i < 13; i++)
            {
                Months.Add(i);
            }
            SelectedMonth = DateTime.Now.Month;
            SelectedYear = DateTime.Now.Year;
        }

        private int _selectedMonth;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                Set(ref _selectedMonth, value);
                if (!_yearMontIsChanging) SelectedYearMonthChanged?.Invoke(this, new ViewModelReloadEventArgs(this));
            }
        }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                Set(ref _selectedYear, value);
                if (!_yearMontIsChanging) SelectedYearMonthChanged?.Invoke(this, new ViewModelReloadEventArgs(this));
            }
        }

        private ObservableCollection<int> _months;
        public ObservableCollection<int> Months
        {
            get => _months;
            set => Set(ref _months, value);
        }

        public DateTime CurrentMonth => new DateTime(SelectedYear, SelectedMonth, 1);

        public event EventHandler<ViewModelReloadEventArgs> SelectedYearMonthChanged;

        private bool _yearMontIsChanging;

        public void PreviousMonth()
        {
            UpdateYearMonth(CurrentMonth.AddMonths(-1));
        }

        public void NextMonth()
        {
            UpdateYearMonth(CurrentMonth.AddMonths(1));
        }

        private void UpdateYearMonth(DateTime newYearMonth)
        {
            _yearMontIsChanging = true;
            SelectedYear = newYearMonth.Year;
            SelectedMonth = newYearMonth.Month;
            _yearMontIsChanging = false;
            SelectedYearMonthChanged?.Invoke(this, new ViewModelReloadEventArgs(this));
        }
    }
}
