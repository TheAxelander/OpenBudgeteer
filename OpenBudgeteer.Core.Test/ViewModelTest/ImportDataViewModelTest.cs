using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenBudgeteer.Core.Test.ViewModelTest;
public class ImportDataViewModelTest
{
    private readonly DbContextOptions<DatabaseContext> _dbOptions;
    private readonly Account _testAccount;

    public ImportDataViewModelTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required to read ANSI Text files
        _dbOptions = DbConnector.GetDbContextOptions(nameof(ImportDataViewModelTest));
        DbConnector.CleanupDatabase(nameof(ImportDataViewModelTest));
        _testAccount = new Account() { Name = "Test Account", IsActive = 1 };
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            dbContext.CreateAccount(_testAccount);
        }
    }

    public static IEnumerable<object[]> TestData_LoadData_CheckAvailableProfiles
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new ImportProfile()
                    {
                        ProfileName = "Test Profile",

                        TransactionDateColumnName = "Date",
                        PayeeColumnName = "Payee",
                        MemoColumnName = "Memo",
                        AmountColumnName = "Amount",
                        CreditColumnName = "Credit",

                        Delimiter = ';',
                        TextQualifier = '"',
                        DateFormat = "dd.MM.yyyy",
                        NumberFormat = "de-DE",
                        HeaderRow = 2
                    }
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(TestData_LoadData_CheckAvailableProfiles))]
    public void LoadData_CheckAvailableProfiles(
        ImportProfile importProfile)
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var inactiveTestAccount = new Account() { Name = "Inactive Test Account", IsActive = 0 };
                dbContext.CreateAccount(inactiveTestAccount);

                importProfile.AccountId = _testAccount.AccountId;
                dbContext.CreateImportProfile(importProfile);

                var viewModel = new ImportDataViewModel(_dbOptions);
                viewModel.LoadData();
                var loadedImportProfile = viewModel.AvailableImportProfiles.Single(
                    i => i.ImportProfileId == importProfile.ImportProfileId);

                var foo = viewModel.AvailableAccounts;
                var bar = dbContext.Account;

                Assert.Single(viewModel.AvailableAccounts);
                Assert.Equal(_testAccount.Name, viewModel.AvailableAccounts.First().Name);
                Assert.Equal(_testAccount.IsActive, viewModel.AvailableAccounts.First().IsActive);

                Assert.Single(viewModel.AvailableImportProfiles);
                Assert.Equal(importProfile.ProfileName, loadedImportProfile.ProfileName);
                Assert.Equal(importProfile.AccountId, loadedImportProfile.AccountId);
                Assert.Equal(importProfile.TransactionDateColumnName, loadedImportProfile.TransactionDateColumnName);
                Assert.Equal(importProfile.PayeeColumnName, loadedImportProfile.PayeeColumnName);
                Assert.Equal(importProfile.MemoColumnName, loadedImportProfile.MemoColumnName);
                Assert.Equal(importProfile.AmountColumnName, loadedImportProfile.AmountColumnName);
                Assert.Equal(importProfile.CreditColumnName, loadedImportProfile.CreditColumnName);
                Assert.Equal(importProfile.Delimiter, loadedImportProfile.Delimiter);
                Assert.Equal(importProfile.TextQualifier, loadedImportProfile.TextQualifier);
                Assert.Equal(importProfile.DateFormat, loadedImportProfile.DateFormat);
                Assert.Equal(importProfile.NumberFormat, loadedImportProfile.NumberFormat);
                Assert.Equal(importProfile.HeaderRow, loadedImportProfile.HeaderRow);
            }
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(ImportDataViewModelTest));
        }
    }

    public static IEnumerable<object[]> TestData_LoadProfileAsync_CheckSelectedImportProfileHeaders
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new ImportProfile()
                    {
                        ProfileName = "Test Profile",

                        TransactionDateColumnName = "Date",
                        PayeeColumnName = "Payee",
                        MemoColumnName = "Memo",
                        AmountColumnName = "Amount (EUR)",
                        CreditColumnName = "Credit",

                        Delimiter = ';',
                        TextQualifier = '"',
                        DateFormat = "dd.MM.yyyy",
                        NumberFormat = "de-DE",
                        HeaderRow = 11
                    },
                    "./Resources/TestImportFile1.txt",
                    new List<string> {"Accounting Date", "Date", "Type", "Payee", "Memo", "IBAN", "Amount (EUR)"}
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(TestData_LoadProfileAsync_CheckSelectedImportProfileHeaders))]
    public async Task LoadProfileAsync_CheckSelectedImportProfileHeaders(
        ImportProfile importProfile,
        string testFilePath,
        List<string> fileHeaders)
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                importProfile.AccountId = _testAccount.AccountId;
                dbContext.CreateImportProfile(importProfile);

                var viewModel = new ImportDataViewModel(_dbOptions);
                viewModel.LoadData();
                
                await viewModel.HandleOpenFileAsync(File.OpenRead(testFilePath));
                viewModel.SelectedImportProfile = viewModel.AvailableImportProfiles.Single(
                    i => i.ImportProfileId == importProfile.ImportProfileId);
                await viewModel.LoadProfileAsync();

                Assert.Equal(_testAccount.AccountId, viewModel.SelectedAccount.AccountId);

                Assert.Equal(7, viewModel.IdentifiedColumns.Count);
                for (int i = 0; i < viewModel.IdentifiedColumns.Count; i++)
                {
                    Assert.Equal(fileHeaders[i], viewModel.IdentifiedColumns[i]);
                }
            }
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(ImportDataViewModelTest));
        }
    }

    public static IEnumerable<object[]> TestData_LoadProfileAsync_CheckValidateData
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new ImportProfile()
                    {
                        ProfileName = "Test Profile",

                        TransactionDateColumnName = "Date",
                        PayeeColumnName = "Payee",
                        MemoColumnName = "Memo",
                        AmountColumnName = "Amount (EUR)",
                        CreditColumnName = string.Empty,

                        Delimiter = ';',
                        TextQualifier = '"',
                        DateFormat = "dd.MM.yyyy",
                        NumberFormat = "de-DE",
                        HeaderRow = 11
                    },
                    "./Resources/TestImportFile1.txt"
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(TestData_LoadProfileAsync_CheckValidateData))]
    public async Task LoadProfileAsync_CheckValidateData(
        ImportProfile importProfile,
        string testFilePath)
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                importProfile.AccountId = _testAccount.AccountId;
                dbContext.CreateImportProfile(importProfile);

                var viewModel = new ImportDataViewModel(_dbOptions);
                viewModel.LoadData();

                await viewModel.HandleOpenFileAsync(File.OpenRead(testFilePath));
                viewModel.SelectedImportProfile = viewModel.AvailableImportProfiles.Single(
                    i => i.ImportProfileId == importProfile.ImportProfileId);
                await viewModel.LoadProfileAsync();
                await viewModel.ValidateDataAsync();

                Assert.Equal(4, viewModel.TotalRecords);
                Assert.Equal(4, viewModel.ValidRecords);
                Assert.Equal(0, viewModel.RecordsWithErrors);
                Assert.Equal(0, viewModel.PotentialDuplicates);
                Assert.Equal(4, viewModel.ParsedRecords.Count);
                Assert.Empty(viewModel.Duplicates);

                // Check Record 1
                var checkRecord = viewModel.ParsedRecords[0].Result;
                Assert.Equal(new DateTime(2022,02,14), checkRecord.TransactionDate);
                Assert.Equal("Lorem ipsum", checkRecord.Payee);
                Assert.Equal("dolor sit amet", checkRecord.Memo);
                Assert.Equal(new decimal(-2.95), checkRecord.Amount);

                // Check Record 2
                checkRecord = viewModel.ParsedRecords[1].Result;
                Assert.Equal(new DateTime(2022, 02, 15), checkRecord.TransactionDate);
                Assert.Equal("Foobar Company", checkRecord.Payee);
                Assert.Equal(string.Empty, checkRecord.Memo);
                Assert.Equal(new decimal(-27.5), checkRecord.Amount);

                // Check Record 3
                checkRecord = viewModel.ParsedRecords[2].Result;
                Assert.Equal(new DateTime(2022, 02, 16), checkRecord.TransactionDate);
                Assert.Equal("EMPLOYER", checkRecord.Payee);
                Assert.Equal("Income Feb/2022", checkRecord.Memo);
                Assert.Equal(new decimal(43), checkRecord.Amount);

                // Check Record 4
                checkRecord = viewModel.ParsedRecords[3].Result;
                Assert.Equal(new DateTime(2022, 02, 17), checkRecord.TransactionDate);
                Assert.Equal("The Webshop.com", checkRecord.Payee);
                Assert.Equal("Billing", checkRecord.Memo);
                Assert.Equal(new decimal(-6.34), checkRecord.Amount);
            }
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(ImportDataViewModelTest));
        }
    }

    public static IEnumerable<object[]> TestData_LoadProfileAsync_CheckValidateDataWithCreditColumn
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new ImportProfile()
                    {
                        ProfileName = "Test Profile",

                        TransactionDateColumnName = "Date",
                        PayeeColumnName = "Payee",
                        MemoColumnName = "Memo",
                        AmountColumnName = "Debit (EUR)",
                        CreditColumnName = "Credit (EUR)",

                        AdditionalSettingCreditValue = 1,
                        
                        Delimiter = ';',
                        TextQualifier = '"',
                        DateFormat = "dd.MM.yyyy",
                        NumberFormat = "de-DE",
                        HeaderRow = 11
                    },
                    "./Resources/TestImportFile2.txt"
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(TestData_LoadProfileAsync_CheckValidateDataWithCreditColumn))]
    public async Task LoadProfileAsync_CheckValidateDataWithCreditColumn(
        ImportProfile importProfile,
        string testFilePath)
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                importProfile.AccountId = _testAccount.AccountId;
                dbContext.CreateImportProfile(importProfile);

                var viewModel = new ImportDataViewModel(_dbOptions);
                viewModel.LoadData();

                await viewModel.HandleOpenFileAsync(File.OpenRead(testFilePath));
                viewModel.SelectedImportProfile = viewModel.AvailableImportProfiles.Single(
                    i => i.ImportProfileId == importProfile.ImportProfileId);
                await viewModel.LoadProfileAsync();
                await viewModel.ValidateDataAsync();

                Assert.Equal(4, viewModel.TotalRecords);
                Assert.Equal(4, viewModel.ValidRecords);
                Assert.Equal(0, viewModel.RecordsWithErrors);
                Assert.Equal(0, viewModel.PotentialDuplicates);
                Assert.Equal(4, viewModel.ParsedRecords.Count);
                Assert.Empty(viewModel.Duplicates);

                // Check Record 1
                var checkRecord = viewModel.ParsedRecords[0].Result;
                Assert.Equal(new DateTime(2022, 02, 14), checkRecord.TransactionDate);
                Assert.Equal("Lorem ipsum", checkRecord.Payee);
                Assert.Equal("dolor sit amet", checkRecord.Memo);
                Assert.Equal(new decimal(-2.95), checkRecord.Amount);

                // Check Record 2
                checkRecord = viewModel.ParsedRecords[1].Result;
                Assert.Equal(new DateTime(2022, 02, 15), checkRecord.TransactionDate);
                Assert.Equal("Foobar Company", checkRecord.Payee);
                Assert.Equal(string.Empty, checkRecord.Memo);
                Assert.Equal(new decimal(-27.5), checkRecord.Amount);

                // Check Record 3
                checkRecord = viewModel.ParsedRecords[2].Result;
                Assert.Equal(new DateTime(2022, 02, 16), checkRecord.TransactionDate);
                Assert.Equal("EMPLOYER", checkRecord.Payee);
                Assert.Equal("Income Feb/2022", checkRecord.Memo);
                Assert.Equal(new decimal(43), checkRecord.Amount);

                // Check Record 4
                // Credit Column value is positive in file and should be negative after import
                // Debit Column value is 0,00 and should be skipped
                checkRecord = viewModel.ParsedRecords[3].Result;
                Assert.Equal(new DateTime(2022, 02, 17), checkRecord.TransactionDate);
                Assert.Equal("The Webshop.com", checkRecord.Payee);
                Assert.Equal("Billing", checkRecord.Memo);
                Assert.Equal(new decimal(-6.34), checkRecord.Amount);
            }
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(ImportDataViewModelTest));
        }
    }

    public static IEnumerable<object[]> TestData_LoadProfileAsync_CheckValidateDataWithInvalidRecords
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new ImportProfile()
                    {
                        ProfileName = "Test Profile",

                        TransactionDateColumnName = "Date",
                        PayeeColumnName = "Payee",
                        MemoColumnName = "Memo",
                        AmountColumnName = "Amount (EUR)",
                        CreditColumnName = string.Empty,

                        Delimiter = ';',
                        TextQualifier = '"',
                        DateFormat = "dd.MM.yyyy",
                        NumberFormat = "de-DE",
                        HeaderRow = 11
                    },
                    "./Resources/TestImportFile3.txt"
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(TestData_LoadProfileAsync_CheckValidateDataWithInvalidRecords))]
    public async Task LoadProfileAsync_CheckValidateDataWithInvalidRecords(
        ImportProfile importProfile,
        string testFilePath)
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                importProfile.AccountId = _testAccount.AccountId;
                dbContext.CreateImportProfile(importProfile);

                var viewModel = new ImportDataViewModel(_dbOptions);
                viewModel.LoadData();

                await viewModel.HandleOpenFileAsync(File.OpenRead(testFilePath));
                viewModel.SelectedImportProfile = viewModel.AvailableImportProfiles.Single(
                    i => i.ImportProfileId == importProfile.ImportProfileId);
                await viewModel.LoadProfileAsync();
                await viewModel.ValidateDataAsync();

                Assert.Equal(4, viewModel.TotalRecords);
                Assert.Equal(2, viewModel.ValidRecords);
                Assert.Equal(2, viewModel.RecordsWithErrors);
                Assert.Equal(0, viewModel.PotentialDuplicates);
                Assert.Equal(4, viewModel.ParsedRecords.Count);
                Assert.Empty(viewModel.Duplicates);

                // Check Valid Record 1
                var checkRecord = viewModel.ParsedRecords[0].Result;
                Assert.Equal(new DateTime(2022, 02, 14), checkRecord.TransactionDate);
                Assert.Equal("Lorem ipsum", checkRecord.Payee);
                Assert.Equal("dolor sit amet", checkRecord.Memo);
                Assert.Equal(new decimal(-2.95), checkRecord.Amount);

                // Check Valid Record 2
                checkRecord = viewModel.ParsedRecords[2].Result;
                Assert.Equal(new DateTime(2022, 02, 16), checkRecord.TransactionDate);
                Assert.Equal("EMPLOYER", checkRecord.Payee);
                Assert.Equal("Income Feb/2022", checkRecord.Memo);
                Assert.Equal(new decimal(43), checkRecord.Amount);
            }
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(ImportDataViewModelTest));
        }
    }

    public static IEnumerable<object[]> TestData_LoadProfileAsync_CheckValidateDataWithDuplicates
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new ImportProfile()
                    {
                        ProfileName = "Test Profile",

                        TransactionDateColumnName = "Date",
                        PayeeColumnName = "Payee",
                        MemoColumnName = "Memo",
                        AmountColumnName = "Amount (EUR)",
                        CreditColumnName = string.Empty,

                        Delimiter = ';',
                        TextQualifier = '"',
                        DateFormat = "dd.MM.yyyy",
                        NumberFormat = "de-DE",
                        HeaderRow = 11
                    },
                    "./Resources/TestImportFile3.txt",
                    "./Resources/TestImportFile1.txt"
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(TestData_LoadProfileAsync_CheckValidateDataWithDuplicates))]
    public async Task LoadProfileAsync_CheckValidateDataWithDuplicates(
        ImportProfile importProfile,
        string testFilePath1,
        string testFilePath2)
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                importProfile.AccountId = _testAccount.AccountId;
                dbContext.CreateImportProfile(importProfile);

                var viewModel = new ImportDataViewModel(_dbOptions);
                viewModel.LoadData();

                await viewModel.HandleOpenFileAsync(File.OpenRead(testFilePath1));
                viewModel.SelectedImportProfile = viewModel.AvailableImportProfiles.Single(
                    i => i.ImportProfileId == importProfile.ImportProfileId);
                await viewModel.LoadProfileAsync();
                await viewModel.ValidateDataAsync();
                
                Assert.Equal(4, viewModel.TotalRecords);
                Assert.Equal(2, viewModel.ValidRecords);
                Assert.Equal(2, viewModel.RecordsWithErrors);
                Assert.Equal(0, viewModel.PotentialDuplicates);
                Assert.Equal(4, viewModel.ParsedRecords.Count);
                Assert.Empty(viewModel.Duplicates);

                await viewModel.ImportDataAsync();

                Assert.Equal(2, dbContext.BankTransaction.Count());

                // Load next file including two duplicates on existing BankTransaction
                await viewModel.HandleOpenFileAsync(File.OpenRead(testFilePath2));
                await viewModel.ValidateDataAsync();

                Assert.Equal(4, viewModel.TotalRecords);
                Assert.Equal(4, viewModel.ValidRecords);
                Assert.Equal(0, viewModel.RecordsWithErrors);
                Assert.Equal(2, viewModel.PotentialDuplicates);
                Assert.Equal(4, viewModel.ParsedRecords.Count);
                Assert.Equal(2, viewModel.Duplicates.Count);

                // Exclude just one duplicate and import the rest
                viewModel.ExcludeDuplicateRecord(viewModel.Duplicates.First());

                Assert.Equal(3, viewModel.TotalRecords);
                Assert.Equal(3, viewModel.ValidRecords);
                Assert.Equal(0, viewModel.RecordsWithErrors);
                Assert.Equal(1, viewModel.PotentialDuplicates);
                Assert.Equal(3, viewModel.ParsedRecords.Count);
                Assert.Single(viewModel.Duplicates);

                await viewModel.ImportDataAsync(false);

                Assert.Equal(5, dbContext.BankTransaction.Count());
            }
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(ImportDataViewModelTest));
        }
    }

    public static IEnumerable<object[]> TestData_LoadProfileAsync_CheckValidateDataWithDifferentSettings
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new ImportProfile()
                    {
                        ProfileName = "Test Profile",

                        TransactionDateColumnName = "Date",
                        PayeeColumnName = "Payee",
                        MemoColumnName = "Memo",
                        AmountColumnName = "Amount (USD)",
                        CreditColumnName = string.Empty,

                        Delimiter = ',',
                        TextQualifier = '\'',
                        DateFormat = "yyyy-MM-dd",
                        NumberFormat = "en-US",
                        HeaderRow = 11
                    },
                    "./Resources/TestImportFile4.txt"
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(TestData_LoadProfileAsync_CheckValidateDataWithDifferentSettings))]
    public async Task LoadProfileAsync_CheckValidateDataWithDifferentSettings(
        ImportProfile importProfile,
        string testFilePath)
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                importProfile.AccountId = _testAccount.AccountId;
                dbContext.CreateImportProfile(importProfile);

                var viewModel = new ImportDataViewModel(_dbOptions);
                viewModel.LoadData();

                await viewModel.HandleOpenFileAsync(File.OpenRead(testFilePath));
                viewModel.SelectedImportProfile = viewModel.AvailableImportProfiles.Single(
                    i => i.ImportProfileId == importProfile.ImportProfileId);
                await viewModel.LoadProfileAsync();
                await viewModel.ValidateDataAsync();

                Assert.Equal(4, viewModel.TotalRecords);
                Assert.Equal(4, viewModel.ValidRecords);
                Assert.Equal(0, viewModel.RecordsWithErrors);
                Assert.Equal(0, viewModel.PotentialDuplicates);
                Assert.Equal(4, viewModel.ParsedRecords.Count);
                Assert.Empty(viewModel.Duplicates);

                // Check Record 1
                var checkRecord = viewModel.ParsedRecords[0].Result;
                Assert.Equal(new DateTime(2022, 02, 14), checkRecord.TransactionDate);
                Assert.Equal("Lorem ipsum", checkRecord.Payee);
                Assert.Equal("dolor sit amet", checkRecord.Memo);
                Assert.Equal(new decimal(-2.95), checkRecord.Amount);

                // Check Record 2
                checkRecord = viewModel.ParsedRecords[1].Result;
                Assert.Equal(new DateTime(2022, 02, 15), checkRecord.TransactionDate);
                Assert.Equal("Foobar Company", checkRecord.Payee);
                Assert.Equal(string.Empty, checkRecord.Memo);
                Assert.Equal(new decimal(-27.5), checkRecord.Amount);

                // Check Record 3
                checkRecord = viewModel.ParsedRecords[2].Result;
                Assert.Equal(new DateTime(2022, 02, 16), checkRecord.TransactionDate);
                Assert.Equal("EMPLOYER", checkRecord.Payee);
                Assert.Equal("Income Feb/2022", checkRecord.Memo);
                Assert.Equal(new decimal(43), checkRecord.Amount);

                // Check Record 4
                checkRecord = viewModel.ParsedRecords[3].Result;
                Assert.Equal(new DateTime(2022, 02, 17), checkRecord.TransactionDate);
                Assert.Equal("The Webshop.com", checkRecord.Payee);
                Assert.Equal("Billing", checkRecord.Memo);
                Assert.Equal(new decimal(-6.34), checkRecord.Amount);
            }
        }
        finally
        {
            DbConnector.CleanupDatabase(nameof(ImportDataViewModelTest));
        }
    }

}
