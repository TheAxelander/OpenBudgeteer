using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Contracts.Models;
using OpenBudgeteer.Core.ViewModels;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using OpenBudgeteer.Data;
using Xunit;

namespace OpenBudgeteer.Core.Test.ViewModelTest;

public class BucketViewModelTest
{
    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    public BucketViewModelTest()
    {
        _dbOptions = DbConnector.GetDbContextOptions(nameof(BucketViewModelTest));
        DbConnector.CleanupDatabase(nameof(BucketViewModelTest));
    }

    public static IEnumerable<object[]> TestData_LoadDataAsync_CheckBucketGroupAssignedBuckets
    {
        get
        {
            return new[]
            {
                new object[] {new List<string> {"Bucket 1"}},
                new object[] {new List<string> {"Bucket 1", "Bucket 2", "Bucket 3"}},
                new object[] {new List<string>()},
            };
        }
    }
    
    [Theory]
    [MemberData(nameof(TestData_LoadDataAsync_CheckBucketGroupAssignedBuckets))]
    public async Task LoadDataAsync_CheckBucketGroupAssignedBuckets(List<string> bucketNames)
    {
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            var testBucketGroup = new BucketGroup() { Name = "Bucket Group", Position = 1 };
            dbContext.CreateBucketGroup(testBucketGroup);

            foreach (var bucketName in bucketNames)
            {
                dbContext.CreateBucket(new Bucket()
                {
                    BucketGroupId = testBucketGroup.BucketGroupId, 
                    Name = bucketName, 
                    ColorCode = "Red", 
                    ValidFrom = new DateTime(2010, 1, 1)
                });
            }
                
            var monthSelectorViewModel = new YearMonthSelectorViewModel();
            var viewModel = new BucketViewModel(_dbOptions, monthSelectorViewModel);
            await viewModel.LoadDataAsync();
                
            var testObject = viewModel.BucketGroups
                .FirstOrDefault(i => i.BucketGroup.BucketGroupId == testBucketGroup.BucketGroupId);
                
            Assert.NotNull(testObject);
            Assert.Equal(bucketNames.Count, testObject.Buckets.Count);
            foreach (var bucketName in bucketNames)
            {
                Assert.Contains(testObject.Buckets, i => i.Bucket.Name == bucketName);
            }
        }
    }

    public static IEnumerable<object[]> TestData_LoadDataAsync_CheckBucketSorting
    {
        get
        {
            return new[]
            {
                new object[] {new List<string> {"A_Bucket 1", "C_Bucket 2", "B_Bucket 3"}, new List<string> {"A_Bucket 1", "B_Bucket 3", "C_Bucket 2"} }
            };
        }
    }
    
    [Theory]
    [MemberData(nameof(TestData_LoadDataAsync_CheckBucketSorting))]
    public async Task LoadDataAsync_CheckBucketSorting(List<string> bucketNamesUnsorted, List<string> expectedBucketNamesSorted)
    {
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            var testBucketGroup = new BucketGroup() { Name = "Bucket Group", Position = 1 };
            dbContext.CreateBucketGroup(testBucketGroup);
                
            foreach (var bucketName in bucketNamesUnsorted)
            {
                dbContext.CreateBucket(new Bucket()
                {
                    BucketGroupId = testBucketGroup.BucketGroupId, 
                    Name = bucketName, 
                    ColorCode = "Red", 
                    ValidFrom = new DateTime(2010, 1, 1)
                });
            }
                
            var monthSelectorViewModel = new YearMonthSelectorViewModel();
            var viewModel = new BucketViewModel(_dbOptions, monthSelectorViewModel);
            await viewModel.LoadDataAsync();

            var bucketGroup = viewModel.BucketGroups.FirstOrDefault(i => i.BucketGroup.BucketGroupId == testBucketGroup.BucketGroupId);
            Assert.NotNull(bucketGroup);
            Assert.Equal(bucketNamesUnsorted.Count, bucketGroup.Buckets.Count);

            for (int i = 0; i < bucketGroup.Buckets.Count; i++)
            {
                Assert.Equal(
                    expectedBucketNamesSorted.ElementAt(i),
                    bucketGroup.Buckets.ElementAt(i).Bucket.Name);
            }
        }
    }

    public static IEnumerable<object[]> TestData_LoadDataAsync_LoadOnlyActiveBuckets
    {
        get
        {
            return new[]
            {
                // Active in current month
                new object[] { new DateTime(2010,1,1), new DateTime(2010,1,1), false, null, true},
                // Active starting next month
                new object[] { new DateTime(2010,1,1), new DateTime(2010,2,1), false, null, false},
                // Active starting next year
                new object[] { new DateTime(2010,1,1), new DateTime(2011,1,1), false, null, false},
                // Inactive since current month
                new object[] { new DateTime(2010,1,1), new DateTime(2009,1,1), true, new DateTime(2010,1,1), false},
                // Inactive since last year
                new object[] { new DateTime(2010,1,1), new DateTime(2009,1,1), true, new DateTime(2009,1,1), false},
                // Inactive since last month
                new object[] { new DateTime(2010,2,1), new DateTime(2009,1,1), true, new DateTime(2010,1,1), false},
                // Inactive starting next month                  
                new object[] { new DateTime(2010,1,1), new DateTime(2010,1,1), true, new DateTime(2010,2,1), true},
                // Active starting next month but already inactive in the future
                new object[] { new DateTime(2010,1,1), new DateTime(2010,2,1), true, new DateTime(2010,3,1), false}
            };
        }
    }
    
    [Theory]
    [MemberData(nameof(TestData_LoadDataAsync_LoadOnlyActiveBuckets))]
    public async Task LoadDataAsync_LoadOnlyActiveBuckets(
        DateTime testMonth,
        DateTime bucketActiveSince,
        bool bucketIsInactive,
        DateTime bucketIsInActiveFrom,
        bool expectedBucketAvailable
        )
    {
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            var testAccount = new Account() {IsActive = 1, Name = "Account"};
            var testBucketGroup = new BucketGroup() {Name = "Bucket Group", Position = 1};

            dbContext.CreateAccount(testAccount);
            dbContext.CreateBucketGroup(testBucketGroup);

            var testBucket = new Bucket()
            {
                BucketGroupId = testBucketGroup.BucketGroupId,
                Name = "Bucket",
                ValidFrom = bucketActiveSince,
                IsInactive = bucketIsInactive,
                IsInactiveFrom = bucketIsInActiveFrom,
            };

            dbContext.CreateBucket(testBucket);
            dbContext.CreateBucketVersion(new BucketVersion()
            {
                BucketId = testBucket.BucketId, 
                Version = 1, 
                BucketType = 1, 
                ValidFrom = bucketActiveSince
            });
                
            var monthSelectorViewModel = new YearMonthSelectorViewModel()
            {
                SelectedYear = testMonth.Year,
                SelectedMonth = testMonth.Month
            };
            var viewModel = new BucketViewModel(_dbOptions, monthSelectorViewModel);
            await viewModel.LoadDataAsync();
            
            var bucketGroup = viewModel.BucketGroups.FirstOrDefault(i => i.BucketGroup.BucketGroupId == testBucketGroup.BucketGroupId);
            Assert.NotNull(bucketGroup);
            
            Assert.Equal(expectedBucketAvailable, bucketGroup.Buckets.Any());
        }
    }

    [Fact]
    public async Task LoadDataAsync_CheckValidFromHandling()
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var testAccount = new Account() { IsActive = 1, Name = "Account" };
                var testBucketGroup = new BucketGroup() { Name = "Bucket Group", Position = 1 };

                dbContext.CreateAccount(testAccount);
                dbContext.CreateBucketGroup(testBucketGroup);

                var testBucket1 = new Bucket()
                {
                    BucketGroupId = testBucketGroup.BucketGroupId, 
                    Name = "Bucket Active Current Month",
                    ValidFrom = new DateTime(2010, 1, 1)
                };
                var testBucket2 = new Bucket()
                {
                    BucketGroupId = testBucketGroup.BucketGroupId, 
                    Name = "Bucket Active Past",
                    ValidFrom = new DateTime(2009, 1, 1)
                };
                var testBucket3 = new Bucket()
                {
                    BucketGroupId = testBucketGroup.BucketGroupId, 
                    Name = "Bucket Active Future",
                    ValidFrom = new DateTime(2010, 2, 1)
                };

                dbContext.CreateBuckets(new[]
                {
                    testBucket1, testBucket2, testBucket3
                });
                dbContext.CreateBucketVersions(new[]
                {
                    new BucketVersion() { BucketId = testBucket1.BucketId, Version = 1, BucketType = 1, ValidFrom = testBucket1.ValidFrom },
                    new BucketVersion() { BucketId = testBucket2.BucketId, Version = 1, BucketType = 1, ValidFrom = testBucket2.ValidFrom },
                    new BucketVersion() { BucketId = testBucket3.BucketId, Version = 1, BucketType = 1, ValidFrom = testBucket3.ValidFrom },
                });


                var monthSelectorViewModel = new YearMonthSelectorViewModel()
                {
                    SelectedYear = 2010,
                    SelectedMonth = 1
                };
                var viewModel = new BucketViewModel(_dbOptions, monthSelectorViewModel);
                await viewModel.LoadDataAsync();
            
                var bucketGroup = viewModel.BucketGroups.FirstOrDefault(i => i.BucketGroup.BucketGroupId == testBucketGroup.BucketGroupId);
                Assert.NotNull(bucketGroup);
                
                Assert.Equal(2, bucketGroup.Buckets.Count);
                Assert.Contains(bucketGroup.Buckets, i => i.Bucket.BucketId == testBucket1.BucketId);
                Assert.Contains(bucketGroup.Buckets, i => i.Bucket.BucketId == testBucket2.BucketId);
                Assert.DoesNotContain(bucketGroup.Buckets, i => i.Bucket.BucketId == testBucket3.BucketId);
            }
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(BucketViewModelTest));
        }
    }

    [Fact]
    public async Task LoadDataAsync_CheckCalculatedValues()
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var testAccount = new Account() { IsActive = 1, Name = "Account" };
                var testBucketGroup = new BucketGroup() { Name = "Bucket Group", Position = 1 };

                dbContext.CreateAccount(testAccount);
                dbContext.CreateBucketGroup(testBucketGroup);

                var testBucket1 = new Bucket()
                {
                    BucketGroupId = testBucketGroup.BucketGroupId, 
                    Name = "Bucket 1", 
                    ValidFrom = new DateTime(2010, 1, 1)
                };
                var testBucket2 = new Bucket()
                {
                    BucketGroupId = testBucketGroup.BucketGroupId, 
                    Name = "Bucket 2", 
                    ValidFrom = new DateTime(2010, 1, 1)
                };

                dbContext.CreateBuckets(new[]
                {
                    testBucket1, testBucket2
                });
                dbContext.CreateBucketVersions(new[]
                {
                    new BucketVersion() { BucketId = testBucket1.BucketId, Version = 1, BucketType = 1, ValidFrom = testBucket1.ValidFrom },
                    new BucketVersion() { BucketId = testBucket2.BucketId, Version = 1, BucketType = 1, ValidFrom = testBucket2.ValidFrom },
                });

                var testTransactions = new[]
                {
                    new BankTransaction() { AccountId = testAccount.AccountId, TransactionDate = new DateTime(2010,1,1), Amount = 1 },
                    new BankTransaction() { AccountId = testAccount.AccountId, TransactionDate = new DateTime(2010,1,1), Amount = -10 },
                    new BankTransaction() { AccountId = testAccount.AccountId, TransactionDate = new DateTime(2010,1,1), Amount = 100 },
                    new BankTransaction() { AccountId = testAccount.AccountId, TransactionDate = new DateTime(2010,1,1), Amount = -1000 },
                    new BankTransaction() { AccountId = testAccount.AccountId, TransactionDate = new DateTime(2010,1,1), Amount = 10000 },
                    new BankTransaction() { AccountId = testAccount.AccountId, TransactionDate = new DateTime(2009,1,1), Amount = 100000 },
                    new BankTransaction() { AccountId = testAccount.AccountId, TransactionDate = new DateTime(2010,2,1), Amount = 1000000 },
                };
                dbContext.CreateBankTransactions(testTransactions);

                dbContext.CreateBudgetedTransactions(new[]
                {
                    new BudgetedTransaction() { TransactionId = testTransactions[0].TransactionId, BucketId = testBucket1.BucketId, Amount = 1 },
                    new BudgetedTransaction() { TransactionId = testTransactions[1].TransactionId, BucketId = testBucket1.BucketId, Amount = -5 },
                    new BudgetedTransaction() { TransactionId = testTransactions[1].TransactionId, BucketId = testBucket2.BucketId, Amount = -5 },
                    new BudgetedTransaction() { TransactionId = testTransactions[2].TransactionId, BucketId = testBucket1.BucketId, Amount = 100 },
                    new BudgetedTransaction() { TransactionId = testTransactions[3].TransactionId, BucketId = testBucket2.BucketId, Amount = -1000 },
                    new BudgetedTransaction() { TransactionId = testTransactions[4].TransactionId, BucketId = testBucket2.BucketId, Amount = 10000 },
                    new BudgetedTransaction() { TransactionId = testTransactions[5].TransactionId, BucketId = testBucket2.BucketId, Amount = 100000 },
                    new BudgetedTransaction() { TransactionId = testTransactions[6].TransactionId, BucketId = testBucket2.BucketId, Amount = 1000000 },
                });

                var monthSelectorViewModel = new YearMonthSelectorViewModel()
                {
                    SelectedYear = 2010,
                    SelectedMonth = 1
                };
                var viewModel = new BucketViewModel(_dbOptions, monthSelectorViewModel);
                await viewModel.LoadDataAsync();

                var bucketGroup = viewModel.BucketGroups.FirstOrDefault(i => i.BucketGroup.BucketGroupId == testBucketGroup.BucketGroupId);
                Assert.NotNull(bucketGroup);

                // This test includes:
                // - Bucket Split
                var testObject = bucketGroup.Buckets.FirstOrDefault(i => i.Bucket.BucketId == testBucket1.BucketId);
                Assert.NotNull(testObject);
                Assert.Equal(-5, testObject.Activity);
                Assert.Equal(96, testObject.Balance);
                Assert.Equal(101, testObject.In);

                // This test includes:
                // - Bucket Split
                // - Include Transactions in previous months for Balance
                // - Exclude Transactions in the future
                testObject = bucketGroup.Buckets.FirstOrDefault(i => i.Bucket.BucketId == testBucket2.BucketId);
                Assert.NotNull(testObject);
                Assert.Equal(-1005, testObject.Activity);
                Assert.Equal(108995, testObject.Balance);
                Assert.Equal(10000, testObject.In);
            }
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(BucketViewModelTest));
        }
    }

    public static IEnumerable<object[]> TestData_CheckWantAndDetailCalculation_MonthlyExpenses
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new Bucket { Name = "Bucket with pending Want", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 2, BucketTypeYParam = 10 },
                    new List<BankTransaction>(),
                    new List<BucketMovement>(),
                    10, 0, 0
                },
                new object[]
                {
                    new Bucket { Name = "Bucket with fulfilled Want", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 2, BucketTypeYParam = 10 },
                    new List<BankTransaction>(),
                    new List<BucketMovement>()
                    {
                        new BucketMovement { MovementDate = new DateTime(2010, 1, 1), Amount = 10 }
                    },
                    0, 10 ,0
                },
                new object[]
                {
                    new Bucket { Name = "Bucket pending Want including expense", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 2, BucketTypeYParam = 10 },
                    new List<BankTransaction>
                    {
                        new BankTransaction { TransactionDate = new DateTime(2010,1,1), Amount = -10 }
                    },
                    new List<BucketMovement>(),
                    10, 0, -10
                },
                new object[]
                {
                    new Bucket { Name = "Bucket fulfilled Want including expense", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 2, BucketTypeYParam = 10 },
                    new List<BankTransaction>
                    {
                        new BankTransaction { TransactionDate = new DateTime(2010,1,1), Amount = -10 }
                    },
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2010, 1, 1), Amount = 10 }
                    },
                    0, 10, -10
                },
                new object[]
                {
                    new Bucket { Name = "Bucket with partial fulfilled Want", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 2, BucketTypeYParam = 10 },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2010, 1, 1), Amount = 5 }
                    },
                    5, 5, 0
                },
                new object[]
                {
                    new Bucket { Name = "Bucket with over fulfilled Want", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 2, BucketTypeYParam = 10 },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2010, 1, 1), Amount = 15 }
                    },
                    0, 15 ,0
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(TestData_CheckWantAndDetailCalculation_MonthlyExpenses))]
    public async Task LoadDataAsync_CheckWantAndDetailCalculation_MonthlyExpenses(
        Bucket testBucket,
        BucketVersion testBucketVersion,
        List<BankTransaction> testTransactions,
        List<BucketMovement> testBucketMovements,
        decimal expectedWant,
        decimal expectedIn,
        decimal expectedActivity
        )
    {
        try
        {
            var testObject = await ExecuteBucketCreationAndTransactionMovementsAsync(
                testBucket, testBucketVersion, testTransactions, testBucketMovements, new DateTime(2010,1,1));

            Assert.Equal(expectedWant, testObject.Want);
            Assert.Equal(expectedIn, testObject.In);
            Assert.Equal(expectedActivity, testObject.Activity);

        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(BucketViewModelTest));
        }
    }

    public static IEnumerable<object[]> TestData_CheckWantAndDetailCalculation_ExpenseEveryXMonths
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, with Want", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,12,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>(),
                    10, 0, 0, 0, "120 until 2010-12", 0
                },
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, without Want", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,12,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2010,1,1), Amount = 10 }
                    },
                    0, 10, 0, 10, "120 until 2010-12", 8
                },
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, last 6 months, with Want", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 10 }
                    },
                    10, 0, 0, 60, "120 until 2010-06", 50
                },
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, last 6 months, without Want", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2010,1,1), Amount = 10 }
                    },
                    0, 10, 0, 70, "120 until 2010-06", 58
                },
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, last 6 months, fulfilled target", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 20 }
                    },
                    0, 0, 0, 120, "120 until 2010-06", 100
                },
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, last 6 months, over-fulfilled target", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 30 }
                    },
                    0, 0, 0, 130, "120 until 2010-06", 100
                },
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, last 6 months, no input", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>(),
                    20, 0, 0, 0, "120 until 2010-06", 0
                },
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, last 6 months, input not in sync", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 10 }
                    },
                    15, 0, 0, 30, "120 until 2010-06", 25
                },
                new object[]
                {
                    new Bucket { Name = "100 every 3 months, with Want", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 3, BucketTypeYParam = 100, BucketTypeZParam = new DateTime(2010,3,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>(),
                    33.33m, 0, 0, 0, "100 until 2010-03", 0
                },
                new object[]
                {
                    new Bucket { Name = "100 every 3 months, last month, with Want", ValidFrom = new DateTime(2009,11,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 3, BucketTypeYParam = 100, BucketTypeZParam = new DateTime(2010,1,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 33.33m },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 33.33m }
                    },
                    33.34m, 0, 0, 66.66m, "100 until 2010-01", 67
                },
                new object[]
                {
                    new Bucket { Name = "100 every 3 months, last month, input not in sync", ValidFrom = new DateTime(2009,11,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 3, BucketTypeYParam = 100, BucketTypeZParam = new DateTime(2010,1,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 12.34m },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 56.78m }
                    },
                    30.88m, 0, 0, 69.12m, "100 until 2010-01", 69
                },
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, last 6 months, with expenses", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>
                    {
                        new BankTransaction { TransactionDate = new DateTime(2009,9,2), Amount = -30 },
                        new BankTransaction { TransactionDate = new DateTime(2010,1,2), Amount = -10 }
                    },
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 10 }
                    },
                    16.67m, 0, -10, 20, "120 until 2010-06", 17
                },
                new object[]
                {
                    new Bucket { Name = "120 every 12 months, 2nd year, last 6 months, with Want", ValidFrom = new DateTime(2008,7,1) },
                    new BucketVersion { Version = 1, BucketType = 3, BucketTypeXParam = 12, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2009,6,1) },
                    new List<BankTransaction>
                    {
                        new BankTransaction { TransactionDate = new DateTime(2009,6,1), Amount = -120 }
                    },
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2008,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2008,8,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2008,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2008,10,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2008,11,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2008,12,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,1,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,2,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,3,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,4,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,5,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,6,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 10 }
                    },
                    10, 0, 0, 60, "120 until 2010-06", 50
                }
            };
        }
    }

    public static IEnumerable<object[]> TestData_CheckWantAndDetailCalculation_SaveXUntilY
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new Bucket { Name = "120 until 2010-12, no input", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,12,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>(),
                    10, 0, 0, 0, "120 until 2010-12", 0
                },
                new object[]
                {
                    new Bucket { Name = "120 until 2010-12, input in current Month", ValidFrom = new DateTime(2010,1,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,12,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2010,1,1), Amount = 10 }
                    },
                    0, 10, 0, 10, "120 until 2010-12", 8
                },
                new object[]
                {
                    new Bucket { Name = "120 until 2010-06, input in sync", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 10 }
                    },
                    10, 0, 0, 60, "120 until 2010-06", 50
                },
                new object[]
                {
                    new Bucket { Name = "120 until 2010-06, fulfilled target", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 20 }
                    },
                    0, 0, 0, 120, "120 until 2010-06", 100
                },
                new object[]
                {
                    new Bucket { Name = "120 until 2010-06, over-fulfilled target", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,8,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,11,1), Amount = 20 },
                        new BucketMovement { MovementDate = new DateTime(2009,12,1), Amount = 30 }
                    },
                    0, 0, 0, 130, "120 until 2010-06", 100
                },
                new object[]
                {
                    new Bucket { Name = "120 until 2010-06, input not in sync", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2010,6,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 }
                    },
                    15, 0, 0, 30, "120 until 2010-06", 25
                },
                new object[]
                {
                    new Bucket { Name = "120 until 2009-12, target not reached", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 120, BucketTypeZParam = new DateTime(2009,12,1) },
                    new List<BankTransaction>(),
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 }
                    },
                    0, 0, 0, 30, "120 until 2009-12", 25
                },
                new object[]
                {
                    new Bucket { Name = "30 until 2010-01, target reached, with expense in target month", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 30, BucketTypeZParam = new DateTime(2010,1,1) },
                    new List<BankTransaction>
                    {
                        new BankTransaction { TransactionDate = new DateTime(2010,1,5), Amount = -30 }
                    },
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 }
                    },
                    0, 0, -30, 0, "30 until 2010-01", 100
                },
                new object[]
                {
                    new Bucket { Name = "30 until 2010-01, target reached, with lower expense in target month", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 30, BucketTypeZParam = new DateTime(2010,1,1) },
                    new List<BankTransaction>
                    {
                        new BankTransaction { TransactionDate = new DateTime(2010,1,5), Amount = -20 }
                    },
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 }
                    },
                    0, 0, -20, 10, "30 until 2010-01", 100
                },
                new object[]
                {
                    new Bucket { Name = "30 until 2010-01, target reached, with higher expense in target month", ValidFrom = new DateTime(2009,7,1) },
                    new BucketVersion { Version = 1, BucketType = 4, BucketTypeYParam = 30, BucketTypeZParam = new DateTime(2010,1,1) },
                    new List<BankTransaction>
                    {
                        new BankTransaction { TransactionDate = new DateTime(2010,1,5), Amount = -40 }
                    },
                    new List<BucketMovement>
                    {
                        new BucketMovement { MovementDate = new DateTime(2009,7,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,9,1), Amount = 10 },
                        new BucketMovement { MovementDate = new DateTime(2009,10,1), Amount = 10 }
                    },
                    10, 0, -40, -10, "30 until 2010-01", 75
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(TestData_CheckWantAndDetailCalculation_ExpenseEveryXMonths))]
    [MemberData(nameof(TestData_CheckWantAndDetailCalculation_SaveXUntilY))]
    public async Task LoadDataAsync_CheckWantAndDetailCalculation_ExpenseEveryXMonths_SaveXUntilY(
        Bucket testBucket,
        BucketVersion testBucketVersion,
        List<BankTransaction> testTransactions,
        List<BucketMovement> testBucketMovements,
        decimal expectedWant,
        decimal expectedIn,
        decimal expectedActivity,
        decimal expectedBalance,
        string expectedDetails,
        int expectedProgress
        )
    {
        try
        {
            var testObject = await ExecuteBucketCreationAndTransactionMovementsAsync(
                testBucket, testBucketVersion, testTransactions, testBucketMovements, new DateTime(2010,1,1));

            Assert.Equal(expectedWant, testObject.Want);
            Assert.Equal(expectedIn, testObject.In);
            Assert.Equal(expectedActivity, testObject.Activity);
            Assert.Equal(expectedBalance, testObject.Balance);
            Assert.Equal(expectedDetails, testObject.Details);
            Assert.Equal(expectedProgress, testObject.Progress);
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(BucketViewModelTest));
        }
    }

    private async Task<BucketViewModelItem> ExecuteBucketCreationAndTransactionMovementsAsync(
        Bucket testBucket,
        BucketVersion testBucketVersion,
        IEnumerable<BankTransaction> testTransactions,
        IEnumerable<BucketMovement> testBucketMovements,
        DateTime testMonth)
    {
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            var testAccount = new Account() { IsActive = 1, Name = "Account" };
            var testBucketGroup = new BucketGroup() { Name = "Bucket Group", Position = 1 };

            dbContext.CreateAccount(testAccount);
            dbContext.CreateBucketGroup(testBucketGroup);

            testBucket.BucketGroupId = testBucketGroup.BucketGroupId;
            dbContext.CreateBucket(testBucket);

            testBucketVersion.BucketId = testBucket.BucketId;
            testBucketVersion.ValidFrom = testBucket.ValidFrom;
            dbContext.CreateBucketVersion(testBucketVersion);

            foreach (var testTransaction in testTransactions)
            {
                testTransaction.AccountId = testAccount.AccountId;
                dbContext.CreateBankTransaction(testTransaction);
                dbContext.CreateBudgetedTransaction(new BudgetedTransaction()
                {
                    TransactionId = testTransaction.TransactionId,
                    BucketId = testBucket.BucketId,
                    Amount = testTransaction.Amount
                });
            }
            
            foreach (var testBucketMovement in testBucketMovements)
            {
                testBucketMovement.BucketId = testBucket.BucketId;
                dbContext.CreateBucketMovement(testBucketMovement);
            }
            
            var monthSelectorViewModel = new YearMonthSelectorViewModel()
            {
                SelectedYear = testMonth.Year,
                SelectedMonth = testMonth.Month
            };
            var viewModel = new BucketViewModel(_dbOptions, monthSelectorViewModel);
            await viewModel.LoadDataAsync();

            var bucketGroup = viewModel.BucketGroups.FirstOrDefault(i => i.BucketGroup.BucketGroupId == testBucketGroup.BucketGroupId);
            Assert.NotNull(bucketGroup);

            var testObject = bucketGroup.Buckets.FirstOrDefault(i => i.Bucket.BucketId == testBucket.BucketId);
            Assert.NotNull(testObject);

            return testObject;
        }
    }

    public static IEnumerable<object[]> TestData_DistributeBudget_CheckDistributedMoney
    {
        get
        {
            return new[]
            {
                new object[]
                {

                },
            };
        }
    }

    //TODO Finalize Test Case DistributeBudget_CheckDistributedMoney
    [Theory (Skip = "Work in progress")]
    [MemberData(nameof(TestData_DistributeBudget_CheckDistributedMoney))]
    public void DistributeBudget_CheckDistributedMoney(
        IEnumerable<Tuple<Bucket, BucketVersion>> testBuckets)
    {
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            var testAccount = new Account() { IsActive = 1, Name = "Account" };
            var testBucketGroup = new BucketGroup() { Name = "Bucket Group", Position = 1 };

            dbContext.CreateAccount(testAccount);
            dbContext.CreateBucketGroup(testBucketGroup);

            foreach (var (bucket, bucketVersion) in testBuckets)
            {
                bucket.BucketGroupId = testBucketGroup.BucketGroupId;
                dbContext.CreateBucket(bucket);
                bucketVersion.BucketId = bucket.BucketId;
                dbContext.CreateBucketVersion(bucketVersion);
            }
        }
    }
}
