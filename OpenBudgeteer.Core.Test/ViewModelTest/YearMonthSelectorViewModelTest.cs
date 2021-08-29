using System;
using System.Globalization;
using System.Linq;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace OpenBudgeteer.Core.Test.ViewModelTest
{
    public class YearMonthSelectorViewModelTest
    {
        private readonly ITestOutputHelper _output;

        public YearMonthSelectorViewModelTest(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public void Constructor_CheckDefaults()
        {
            var viewModel = new YearMonthSelectorViewModel();

            Assert.Equal(DateTime.Now.Year, viewModel.SelectedYear);
            Assert.Equal(DateTime.Now.Month, viewModel.SelectedMonth);
            Assert.Equal(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), viewModel.CurrentMonth);

            // Test Months
            Assert.Equal(12, viewModel.Months.Count);
            for (int i = 1; i < 13; i++)
            {
                Assert.Equal(i, viewModel.Months.ElementAt(i-1));
            }
            
            var cultureInfo = new CultureInfo("de-DE");
            
            _output.WriteLine($"Current Culture: {CultureInfo.CurrentCulture.Name}");
            _output.WriteLine($"Current UI Culture: {CultureInfo.CurrentUICulture.Name}");
            _output.WriteLine($"Test culture: {cultureInfo.Name}");
            
            var converter = new MonthOutputConverter();
            
            Assert.Equal("Jan", converter.ConvertMonth(1, cultureInfo));
            Assert.Equal("Feb", converter.ConvertMonth(2, cultureInfo));
            Assert.Equal("Mär", converter.ConvertMonth(3, cultureInfo));
            Assert.Equal("Apr", converter.ConvertMonth(4, cultureInfo));
            Assert.Equal("Mai", converter.ConvertMonth(5, cultureInfo));
            Assert.Equal("Jun", converter.ConvertMonth(6, cultureInfo));
            Assert.Equal("Jul", converter.ConvertMonth(7, cultureInfo));
            Assert.Equal("Aug", converter.ConvertMonth(8, cultureInfo));
            Assert.Equal("Sep", converter.ConvertMonth(9, cultureInfo));
            Assert.Equal("Okt", converter.ConvertMonth(10, cultureInfo));
            Assert.Equal("Nov", converter.ConvertMonth(11, cultureInfo));
            Assert.Equal("Dez", converter.ConvertMonth(12, cultureInfo));
        }

        [Fact]
        public void PreviousMonth_CheckMonth()
        {
            var viewModel = new YearMonthSelectorViewModel()
            {
                SelectedYear = 2010,
                SelectedMonth = 2
            };

            viewModel.PreviousMonth();

            Assert.Equal(2010, viewModel.SelectedYear);
            Assert.Equal(1, viewModel.SelectedMonth);

            viewModel.PreviousMonth();

            Assert.Equal(2009, viewModel.SelectedYear);
            Assert.Equal(12, viewModel.SelectedMonth);
        }

        [Fact]
        public void NextMonth_CheckMonth()
        {
            var viewModel = new YearMonthSelectorViewModel()
            {
                SelectedYear = 2009,
                SelectedMonth = 11
            };

            viewModel.NextMonth();

            Assert.Equal(2009, viewModel.SelectedYear);
            Assert.Equal(12, viewModel.SelectedMonth);

            viewModel.NextMonth();

            Assert.Equal(2010, viewModel.SelectedYear);
            Assert.Equal(1, viewModel.SelectedMonth);
        }

        [Fact]
        public void SelectedYearMonthChanged_CheckEventHasBeenInvoked()
        {
            var viewModel = new YearMonthSelectorViewModel()
            {
                SelectedYear = 2010,
                SelectedMonth = 1
            };
            var eventHasBeenInvoked = false;
            viewModel.SelectedYearMonthChanged += (sender, args) => eventHasBeenInvoked = true;

            viewModel.SelectedYear = 2010;
            Assert.False(eventHasBeenInvoked);

            eventHasBeenInvoked = false;
            viewModel.SelectedMonth = 1;
            Assert.False(eventHasBeenInvoked);

            eventHasBeenInvoked = false;
            viewModel.SelectedYear = 2009;
            Assert.True(eventHasBeenInvoked);

            eventHasBeenInvoked = false;
            viewModel.SelectedMonth = 2;
            Assert.True(eventHasBeenInvoked);

            eventHasBeenInvoked = false;
            viewModel.NextMonth();
            Assert.True(eventHasBeenInvoked);

            eventHasBeenInvoked = false;
            viewModel.PreviousMonth();
            Assert.True(eventHasBeenInvoked);

            
        }
    }
}
