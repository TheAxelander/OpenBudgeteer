using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Core.ViewModels.PageViewModels;
using Xunit;

namespace OpenBudgeteer.Core.Test.ViewModelTest;

public class TransactionPageViewModelTest : BaseTest
{
    public TransactionPageViewModelTest() : base(nameof(TransactionPageViewModelTest))
    {
    }

    public static IEnumerable<object[]> TestData_AddRecurringTransactionsAsync_CheckRecurrance => new[]
    {
        // Every week, starting 1.1.2010
        new object[]
        {
            new DateTime(2010,1,1), 1, 1, "Every week",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[]
                {
                    new DateTime(2010,1,1),
                    new DateTime(2010,1,8),
                    new DateTime(2010,1,15),
                    new DateTime(2010,1,22),
                    new DateTime(2010,1,29)
                }),
                new Tuple<int, int, DateTime[]>(2010, 2, new[]
                {
                    new DateTime(2010,2,5),
                    new DateTime(2010,2,12),
                    new DateTime(2010,2,19),
                    new DateTime(2010,2,26)
                }),
                new Tuple<int, int, DateTime[]>(2010, 3, new[]
                {
                    new DateTime(2010,3,5),
                    new DateTime(2010,3,12),
                    new DateTime(2010,3,19),
                    new DateTime(2010,3,26)
                })
            }
        },
        // Every 2 week, starting 1.1.2010
        new object[]
        {
            new DateTime(2010,1,1), 1, 2, "Every 2 weeks",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[]
                {
                    new DateTime(2010,1,1),
                    new DateTime(2010,1,15),
                    new DateTime(2010,1,29)
                }),
                new Tuple<int, int, DateTime[]>(2010, 2, new[]
                {
                    new DateTime(2010,2,12),
                    new DateTime(2010,2,26)
                }),
                new Tuple<int, int, DateTime[]>(2010, 3, new[]
                {
                    new DateTime(2010,3,12),
                    new DateTime(2010,3,26)
                })
            }
        },
        // Every 5 week, starting 1.1.2010
        new object[]
        {
            new DateTime(2010,1,1), 1, 5, "Every 5 weeks",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[]
                {
                    new DateTime(2010,1,1),
                }),
                new Tuple<int, int, DateTime[]>(2010, 2, new[]
                {
                    new DateTime(2010,2,5)
                }),
                new Tuple<int, int, DateTime[]>(2010, 3, new[]
                {
                    new DateTime(2010,3,12)
                })
            }
        },
        // Every month, starting 15.1.2010
        new object[]
        {
            new DateTime(2010,1,15), 2, 1, "Every month",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[] { new DateTime(2010,1,15) }),
                new Tuple<int, int, DateTime[]>(2010, 2, new[] { new DateTime(2010,2,15) }),
                new Tuple<int, int, DateTime[]>(2010, 3, new[] { new DateTime(2010,3,15) }),
                new Tuple<int, int, DateTime[]>(2010, 4, new[] { new DateTime(2010,4,15) }),
                new Tuple<int, int, DateTime[]>(2010, 5, new[] { new DateTime(2010,5,15) }),
                new Tuple<int, int, DateTime[]>(2010, 6, new[] { new DateTime(2010,6,15) })
            }
        },
        // Every 2 month, starting 15.1.2010
        new object[]
        {
            new DateTime(2010,1,15), 2, 2, "Every 2 month",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[] { new DateTime(2010,1,15) }),
                new Tuple<int, int, DateTime[]>(2010, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 3, new[] { new DateTime(2010,3,15) }),
                new Tuple<int, int, DateTime[]>(2010, 4, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 5, new[] { new DateTime(2010,5,15) }),
                new Tuple<int, int, DateTime[]>(2010, 6, Array.Empty<DateTime>())
            }
        },
        // Every 5 month, starting 15.1.2010
        new object[]
        {
            new DateTime(2010,1,15), 2, 5, "Every 5 month",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[] { new DateTime(2010,1,15) }),
                new Tuple<int, int, DateTime[]>(2010, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 4, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 6, new[] { new DateTime(2010,6,15) })
            }
        },
        // Every quarter, starting 15.1.2010
        new object[]
        {
            new DateTime(2010,1,15), 3, 1, "Every quarter",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[] { new DateTime(2010,1,15) }),
                new Tuple<int, int, DateTime[]>(2010, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 4, new[] { new DateTime(2010,4,15) }),
                new Tuple<int, int, DateTime[]>(2010, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 6, Array.Empty<DateTime>())
            }
        },
        // Every 2 quarter, starting 15.1.2010
        new object[]
        {
            new DateTime(2010,1,15), 3, 2, "Every 2 quarter",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[] { new DateTime(2010,1,15) }),
                new Tuple<int, int, DateTime[]>(2010, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 4, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 6, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 7, new[] { new DateTime(2010,7,15) }),
                new Tuple<int, int, DateTime[]>(2010, 8, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 9, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 10, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 11, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 12, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 1, new[] { new DateTime(2011,1,15) }),
                new Tuple<int, int, DateTime[]>(2011, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 4, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 6, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 7, new[] { new DateTime(2011,7,15) }),
                new Tuple<int, int, DateTime[]>(2011, 8, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 9, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 10, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 11, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 12, Array.Empty<DateTime>())
            }
        },
        // Every 5 quarter, starting 15.1.2010
        new object[]
        {
            new DateTime(2010,1,15), 3, 5, "Every 5 quarter",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[] { new DateTime(2010,1,15) }),
                new Tuple<int, int, DateTime[]>(2010, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 4, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 6, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 7, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 8, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 9, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 10, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 11, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 12, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 4, new[] { new DateTime(2011,4,15) }),
                new Tuple<int, int, DateTime[]>(2011, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 6, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 7, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 8, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 9, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 10, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 11, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 12, Array.Empty<DateTime>())
            }
        },
        // Every year, starting 15.1.2010
        new object[]
        {
            new DateTime(2010,1,15), 4, 1, "Every year",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[] { new DateTime(2010,1,15) }),
                new Tuple<int, int, DateTime[]>(2010, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 4, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 6, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 7, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 8, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 9, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 10, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 11, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 12, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 1, new[] { new DateTime(2011,1,15) }),
                new Tuple<int, int, DateTime[]>(2011, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 4, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 6, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 7, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 8, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 9, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 10, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 11, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 12, Array.Empty<DateTime>())
            }
        },
        // Every 2 year, starting 15.1.2010
        new object[]
        {
            new DateTime(2010,1,15), 4, 2, "Every 2 year",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[] { new DateTime(2010,1,15) }),
                new Tuple<int, int, DateTime[]>(2010, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 4, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 6, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 7, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 8, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 9, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 10, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 11, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 12, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2012, 1, new[] { new DateTime(2012,1,15) }),
                new Tuple<int, int, DateTime[]>(2013, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2014, 1, new[] { new DateTime(2014,1,15) })
            }
        },
        // Every 5 year, starting 15.1.2010
        new object[]
        {
            new DateTime(2010,1,15), 4, 5, "Every 5 year",
            new[]
            {
                new Tuple<int, int, DateTime[]>(2010, 1, new[] { new DateTime(2010,1,15) }),
                new Tuple<int, int, DateTime[]>(2010, 2, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 3, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 4, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 5, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 6, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 7, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 8, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 9, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 10, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 11, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2010, 12, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2011, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2012, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2013, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2014, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2015, 1, new[] { new DateTime(2015,1,15) }),
                new Tuple<int, int, DateTime[]>(2016, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2017, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2018, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2019, 1, Array.Empty<DateTime>()),
                new Tuple<int, int, DateTime[]>(2020, 1, new[] { new DateTime(2020,1,15) })
            }
        }
    };

    [Theory]
    [MemberData(nameof(TestData_AddRecurringTransactionsAsync_CheckRecurrance))]
    public async Task AddRecurringTransactionsAsync_CheckRecurrence(
        DateTime firstOccurrence,
        int recurrenceType,
        int recurrenceAmount,
        string memo,
        Tuple<int, int, DateTime[]>[] expectedCreationDatesPerMonth
        )
    {
        try
        {
            var testAccount = new Account() { IsActive = 1, Name = "Account" };

            ServiceManager.AccountService.Create(testAccount);
            ServiceManager.RecurringBankTransactionService.Create(new RecurringBankTransaction
            {
                FirstOccurrenceDate = firstOccurrence,
                AccountId = testAccount.Id,
                RecurrenceType = recurrenceType,
                RecurrenceAmount = recurrenceAmount,
                Memo = memo
            });

            foreach (var monthData in expectedCreationDatesPerMonth.Select(i => new 
                         { Year = i.Item1, Month = i.Item2, Dates = i.Item3 }))
            {
                var monthSelectorViewModel = new YearMonthSelectorViewModel(ServiceManager)
                {
                    SelectedYear = monthData.Year,
                    SelectedMonth = monthData.Month
                };
                var viewModel = new TransactionPageViewModel(ServiceManager, monthSelectorViewModel);
                await viewModel.LoadDataAsync();
                await viewModel.AddRecurringTransactionsAsync();
                await viewModel.LoadDataAsync();

                Assert.Equal(monthData.Dates.Count(), viewModel.Transactions.Count);
                foreach (var item in monthData.Dates)
                {
                    Assert.Contains(item, viewModel.Transactions.Select(i => i.TransactionDate));
                }
            }
        }
        finally
        {
            Cleanup();
        }
    }
}