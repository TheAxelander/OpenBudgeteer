using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using OpenBudgeteer.Core.Common.EventClasses;

namespace OpenBudgeteer.Core.ViewModels
{
    public class YearMonthSelectorViewModel : ViewModelBase
    {
        private int _selectedMonth;
        /// <summary>
        /// Number of the current month
        /// </summary>
        /// <remarks>Change triggers <see cref="SelectedYearMonthChanged"/></remarks>
        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                var valueChanged = Set(ref _selectedMonth, value);
                if (!_yearMontIsChanging && valueChanged) SelectedYearMonthChanged?.Invoke(this, new ViewModelReloadEventArgs(this));
            }
        }

        private int _selectedYear;
        /// <summary>
        /// Number of the current year
        /// </summary>
        /// <remarks>Change triggers <see cref="SelectedYearMonthChanged"/></remarks>
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                var valueChanged = Set(ref _selectedYear, value);
                if (!_yearMontIsChanging && valueChanged) SelectedYearMonthChanged?.Invoke(this, new ViewModelReloadEventArgs(this));
            }
        }

        private ObservableCollection<int> _months;
        /// <summary>
        /// Helper collection which contains the number of all months
        /// </summary>
        public ObservableCollection<int> Months
        {
            get => _months;
            private set => Set(ref _months, value);
        }

        /// <summary>
        /// Returns the first day as <see cref="DateTime"/> based on <see cref="SelectedYear"/> and <see cref="SelectedMonth"/>
        /// </summary>
        public DateTime CurrentMonth => new DateTime(SelectedYear, SelectedMonth, 1);

        /// <summary>
        /// EventHandler which should be invoked once the a year and/or a month has been modified. To be used to trigger
        /// ViewModel reloads which are dependent on this ViewModel
        /// </summary>
        public event EventHandler<ViewModelReloadEventArgs> SelectedYearMonthChanged;

        private bool _yearMontIsChanging; // prevents double invoke of SelectedYearMonthChanged
        
        /// <summary>
        /// Basic constructor
        /// </summary>
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

        /// <summary>
        /// Moves to the previous month
        /// </summary>
        /// <remarks>Triggers <see cref="SelectedYearMonthChanged"/></remarks>
        public void PreviousMonth()
        {
            UpdateYearMonth(CurrentMonth.AddMonths(-1));
        }

        /// <summary>
        /// Moves to the next month
        /// </summary>
        /// <remarks>Triggers <see cref="SelectedYearMonthChanged"/></remarks>
        public void NextMonth()
        {
            UpdateYearMonth(CurrentMonth.AddMonths(1));
        }

        /// <summary>
        /// Sets the date to the passed <see cref="DateTime"/>
        /// </summary>
        /// <param name="newYearMonth">New date</param>
        /// <remarks>Triggers <see cref="SelectedYearMonthChanged"/> (only once)</remarks>
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
