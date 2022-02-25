using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Tokenizer.RFC4180;
using TinyCsvParser.TypeConverter;
using System.Linq.Expressions;

namespace OpenBudgeteer.Core.ViewModels;

public class ImportDataViewModel : ViewModelBase
{
    private class CsvBankTransactionMapping : CsvMapping<BankTransaction>
    {
        /// <summary>
        /// Definition on how CSV columns should be mapped to <see cref="ImportProfile"/>
        /// </summary>
        /// <remarks>
        /// <see cref="ImportProfile"/> instance and collection of columns will be used to identify columnIndex for
        /// CSV mapping
        /// </remarks>
        /// <param name="importProfile">Instance required for CSV column name</param>
        /// <param name="identifiedColumns">Collection of all CSV columns</param>
        public CsvBankTransactionMapping(ImportProfile importProfile, IEnumerable<string> identifiedColumns) : base()
        {
            // Mandatory
            MapProperty(
                identifiedColumns.ToList().IndexOf(importProfile.MemoColumnName),
                x => x.Memo);
            MapProperty(
                identifiedColumns.ToList().IndexOf(importProfile.TransactionDateColumnName),
                x => x.TransactionDate, 
                new DateTimeConverter(importProfile.DateFormat));
            
            // Optional
            if (!string.IsNullOrEmpty(importProfile.PayeeColumnName))
            {
                MapProperty(
                    identifiedColumns.ToList().IndexOf(importProfile.PayeeColumnName), 
                    x => x.Payee);
            }

            // Amount Mapping
            if (!string.IsNullOrEmpty(importProfile.CreditColumnName))
            {
                MapUsing((transaction, row) =>
                {
                    var debitValue = row.Tokens[identifiedColumns.ToList().IndexOf(importProfile.AmountColumnName)];
                    var creditValue = row.Tokens[identifiedColumns.ToList().IndexOf(importProfile.CreditColumnName)];

                    if (string.IsNullOrWhiteSpace(debitValue) && string.IsNullOrWhiteSpace(creditValue))
                    {
                        return false;
                    }
                    
                    if (string.IsNullOrWhiteSpace(debitValue))
                    {
                        decimal.TryParse(creditValue, NumberStyles.Currency, CultureInfo.GetCultureInfo(importProfile.NumberFormat), out var result);
                        transaction.Amount = result > 0 ? result * -1 : result;

                        return true;
                    }
                    else
                    {
                        decimal.TryParse(debitValue, NumberStyles.Currency, CultureInfo.GetCultureInfo(importProfile.NumberFormat), out var result);
                        transaction.Amount = result;
                        
                        return true;
                    }
                });
            }
            else
            {
                MapProperty(
                    identifiedColumns.ToList().IndexOf(importProfile.AmountColumnName),
                    x => x.Amount,
                    new DecimalConverter(new CultureInfo(importProfile.NumberFormat)));
            }
        }
    }

    private string _filePath;
    /// <summary>
    /// Path to the file which should be imported
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set => Set(ref _filePath, value);
    }

    private string _fileText;
    /// <summary>
    /// Readonly content of the file
    /// </summary>
    public string FileText
    {
        get => _fileText;
        set => Set(ref _fileText, value);
    }

    private Account _selectedAccount;
    /// <summary>
    /// Target <see cref="Account"/> for which all imported <see cref="BankTransaction"/> should be added
    /// </summary>
    public Account SelectedAccount
    {
        get => _selectedAccount;
        set => Set(ref _selectedAccount, value);
    }

    private ImportProfile _selectedImportProfile;
    /// <summary>
    /// Selected profile with import settings from the database
    /// </summary>
    public ImportProfile SelectedImportProfile
    {
        get => _selectedImportProfile;
        set => Set(ref _selectedImportProfile, value);
    }

    private int _totalRecords;
    /// <summary>
    /// Number of records identified in the file
    /// </summary>
    public int TotalRecords
    {
        get => _totalRecords;
        private set => Set(ref _totalRecords, value);
    }

    private int _recordsWithErrors;
    /// <summary>
    /// Number of records where import will fail or has failed
    /// </summary>
    public int RecordsWithErrors
    {
        get => _recordsWithErrors;
        private set => Set(ref _recordsWithErrors, value);
    }

    private int _validRecords;
    /// <summary>
    /// Number of records where import will be or was successful
    /// </summary>
    public int ValidRecords
    {
        get => _validRecords;
        private set => Set(ref _validRecords, value);
    }
    
    private int _potentialDuplicates;
    /// <summary>
    /// Number of records which have been identified as potential duplicate
    /// </summary>
    public int PotentialDuplicates
    {
        get => _potentialDuplicates;
        private set => Set(ref _potentialDuplicates, value);
    }

    private ObservableCollection<ImportProfile> _availableImportProfiles;
    /// <summary>
    /// Available <see cref="ImportProfile"/> in the database
    /// </summary>
    public ObservableCollection<ImportProfile> AvailableImportProfiles
    {
        get => _availableImportProfiles;
        private set => Set(ref _availableImportProfiles, value);
    }

    private ObservableCollection<Account> _availableAccounts;
    /// <summary>
    /// Helper collection to list all available <see cref="Account"/> in the database
    /// </summary>
    public ObservableCollection<Account> AvailableAccounts
    {
        get => _availableAccounts;
        private set => Set(ref _availableAccounts, value);
    }

    private ObservableCollection<string> _identifiedColumns;
    /// <summary>
    /// Collection of columns that have been identified in the CSV file
    /// </summary>
    public ObservableCollection<string> IdentifiedColumns
    {
        get => _identifiedColumns;
        private set => Set(ref _identifiedColumns, value);
    }

    private ObservableCollection<CsvMappingResult<BankTransaction>> _parsedRecords;
    /// <summary>
    /// Results of <see cref="TinyCsvParser"/>
    /// </summary>
    public ObservableCollection<CsvMappingResult<BankTransaction>> ParsedRecords
    {
        get => _parsedRecords;
        private set => Set(ref _parsedRecords, value);
    }

    private ObservableCollection<Tuple<CsvMappingResult<BankTransaction>, List<BankTransaction>>> _duplicates;
    /// <summary>
    /// Collection of all parsed CSV records which are a potential duplicate of existing <see cref="BankTransaction"/> 
    /// </summary>
    public ObservableCollection<Tuple<CsvMappingResult<BankTransaction>, List<BankTransaction>>> Duplicates
    {
        get => _duplicates;
        private set => Set(ref _duplicates, value);
    }

    private bool _isProfileValid;
    private string[] _fileLines;
    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public ImportDataViewModel(DbContextOptions<DatabaseContext> dbOptions)
    {
        AvailableImportProfiles = new ObservableCollection<ImportProfile>();
        AvailableAccounts = new ObservableCollection<Account>();
        IdentifiedColumns = new ObservableCollection<string>();
        ParsedRecords = new ObservableCollection<CsvMappingResult<BankTransaction>>();
        Duplicates = new ObservableCollection<Tuple<CsvMappingResult<BankTransaction>, List<BankTransaction>>>();
        SelectedImportProfile = new ImportProfile();
        SelectedAccount = new Account();
        _dbOptions = dbOptions;
    }

    /// <summary>
    /// Initialize ViewModel and load data from database
    /// </summary>
    /// <returns></returns>
    public ViewModelOperationResult LoadData()
    {
        try
        {
            LoadAvailableProfiles();
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                foreach (var account in dbContext.Account.Where(i => i.IsActive == 1))
                {
                    AvailableAccounts.Add(account);
                }
            }
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }           
    }

    /// <summary>
    /// Open a file based on <see cref="FilePath"/> and read its content
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult HandleOpenFile()
    {
        try
        {
            FileText = File.ReadAllText(FilePath, Encoding.GetEncoding(1252));
            _fileLines = File.ReadAllLines(FilePath, Encoding.GetEncoding(1252));

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
    }

    /// <summary>
    /// Open a file based on results of an OpenFileDialog and read its content
    /// </summary>
    /// <param name="dialogResults">OpenFileDialog results</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult HandleOpenFile(string[] dialogResults)
    {
        try
        {
            if (!dialogResults.Any()) return new ViewModelOperationResult(true);
            FilePath = dialogResults[0];
            return HandleOpenFile();
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
    }

    /// <summary>
    /// Open a file based on a <see cref="Stream"/> and read its content
    /// </summary>
    /// <param name="stream">Stream to the file</param>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> HandleOpenFileAsync(Stream stream)
    {
        try
        {
            string line;
            var newLines = new List<string>();
            var stringBuilder = new StringBuilder();

            FilePath = string.Empty;

            using var lineReader = new StreamReader(stream, Encoding.GetEncoding(1252));
            while((line = await lineReader.ReadLineAsync()) != null)
            {
                newLines.Add(line);
                stringBuilder.AppendLine(line);
            }

            FileText = stringBuilder.ToString();
            _fileLines = newLines.ToArray();

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to open file: {e.Message}");
        }
    }

    /// <summary>
    /// Loads all settings based on <see cref="SelectedImportProfile"/>
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> LoadProfileAsync()
    {
        try
        {
            ResetLoadedProfileData();

            // Set target Account
            if (AvailableAccounts.Any(i => i.AccountId == SelectedImportProfile.AccountId))
            {
                SelectedAccount = AvailableAccounts.First(i => i.AccountId == SelectedImportProfile.AccountId);
            }

            var result = LoadHeaders();
            if (!result.IsSuccessful) throw new Exception(result.Message);

            await ValidateDataAsync();
            _isProfileValid = true;

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            _isProfileValid = false;
            return new ViewModelOperationResult(false, $"Unable to load Profile: {e.Message}");
        }
    }

    /// <summary>
    /// Reads column headers from the loaded file
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult LoadHeaders()
    {
        try
        {
            // Set ComboBox selection for Column Mapping
            IdentifiedColumns.Clear();
            var headerLine = _fileLines[SelectedImportProfile.HeaderRow - 1];
            var columns = headerLine.Split(SelectedImportProfile.Delimiter);
            foreach (var column in columns)
            {
                if (column != string.Empty)
                    IdentifiedColumns.Add(column.Trim(SelectedImportProfile.TextQualifier)); // Exclude TextQualifier
            }
            
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to load Headers: {e.Message}");
        }
    }

    /// <summary>
    /// Reset all figures and parsed records
    /// </summary>
    private void ResetLoadedProfileData()
    {
        SelectedAccount = new Account();
        ParsedRecords.Clear();
        TotalRecords = 0;
        RecordsWithErrors = 0;
        ValidRecords = 0;
        PotentialDuplicates = 0;
        Duplicates.Clear();
    }

    /// <summary>
    /// Reads the file and parses the content to a set of <see cref="BankTransaction"/>.
    /// Results will be stored in <see cref="ParsedRecords"/>
    /// </summary>
    /// <remarks>
    /// Sets also figures of the ViewModel like <see cref="TotalRecords"/> or <see cref="ValidRecords"/>
    /// </remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> ValidateDataAsync()
    {
        try
        {
            // Pre-Load Data for verification
            // Initialize CsvReader
            var options = new Options(SelectedImportProfile.TextQualifier, '\\', SelectedImportProfile.Delimiter);
            var tokenizer = new RFC4180Tokenizer(options);
            var csvParserOptions = new CsvParserOptions(true, tokenizer);
            var csvReaderOptions = new CsvReaderOptions(new[] { Environment.NewLine });
            var csvMapper = new CsvBankTransactionMapping(SelectedImportProfile, IdentifiedColumns);
            var csvParser = new CsvParser<BankTransaction>(csvParserOptions, csvMapper);

            // Read File and Skip rows based on HeaderRow
            var stringBuilder = new StringBuilder();
            for (int i = SelectedImportProfile.HeaderRow-1; i < _fileLines.Length; i++)
            {
                stringBuilder.AppendLine(_fileLines[i]);
            }

            // Parse Csv File
            var parsedResults = csvParser.ReadFromString(csvReaderOptions, stringBuilder.ToString()).ToList();

            ParsedRecords.Clear();
            Duplicates.Clear();
            foreach (var parsedResult in parsedResults)
            {
                ParsedRecords.Add(parsedResult);
            }

            await DuplicateCheckOnParsedRecordsAsync();
            UpdateCountValues();

            if (ValidRecords > 0) _isProfileValid = true;
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            TotalRecords = 0;
            RecordsWithErrors = 0;
            ValidRecords = 0;
            PotentialDuplicates = 0;
            ParsedRecords.Clear();
            Duplicates.Clear();
            return new ViewModelOperationResult(false, e.Message);
        }
    }

    /// <summary>
    /// Update counts of several statistics
    /// </summary>
    private void UpdateCountValues()
    {
        TotalRecords = ParsedRecords.Count;
        RecordsWithErrors = ParsedRecords.Count(i => !i.IsValid);
        ValidRecords = ParsedRecords.Count(i => i.IsValid);
        PotentialDuplicates = Duplicates.Count;
    }

    /// <summary>
    /// Checks each parsed CSV records on potential existing <see cref="BankTransaction"/> 
    /// </summary>
    private async Task DuplicateCheckOnParsedRecordsAsync()
    {
        foreach (var parsedRecord in ParsedRecords.Where(i => i.IsValid).ToList())
        {
            var duplicateSearches = Task.Run(() =>
            {
                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    var duplicates = dbContext.BankTransaction.Where(i =>
                        i.TransactionDate.Date == parsedRecord.Result.TransactionDate.Date &&
                        i.Amount == parsedRecord.Result.Amount &&
                        (i.Memo == parsedRecord.Result.Memo || i.Payee == parsedRecord.Result.Payee));
                    if (duplicates.Any())
                        Duplicates.Add(new Tuple<CsvMappingResult<BankTransaction>, List<BankTransaction>>(parsedRecord, duplicates.ToList()));
                }
            });

            await Task.WhenAll(duplicateSearches);
        }
    }

    /// <summary>
    /// Removes the passed duplicate from the parsed records to exclude it from import
    /// </summary>
    /// <param name="duplicate">Duplicate that should be excluded</param>
    public void ExcludeDuplicateRecord(Tuple<CsvMappingResult<BankTransaction>, List<BankTransaction>> duplicate)
    {
        ParsedRecords.Remove(duplicate.Item1);
        Duplicates.Remove(duplicate);
        UpdateCountValues();
    }

    /// <summary>
    /// Uses data from <see cref="ParsedRecords"/> to store it in the database
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> ImportDataAsync()
    {
        if (!_isProfileValid) return new ViewModelOperationResult(false, "Unable to Import Data as current settings are invalid.");
        
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var importedCount = 0;
                    var newRecords = new List<BankTransaction>();
                    foreach (var parsedRecord in ParsedRecords.Where(i => i.IsValid))
                    {
                        var newRecord = parsedRecord.Result;
                        newRecord.AccountId = SelectedAccount.AccountId;
                        newRecords.Add(newRecord);
                    }
                    importedCount = dbContext.CreateBankTransactions(newRecords);
                    
                    transaction.Commit();
                    return new ViewModelOperationResult(true, $"Successfully imported {importedCount} records.");
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return new ViewModelOperationResult(false, $"Unable to Import Data. Error message: {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Helper method to load <see cref="ImportProfile"/> from the database
    /// </summary>
    private void LoadAvailableProfiles()
    {
        AvailableImportProfiles.Clear();
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            foreach (var profile in dbContext.ImportProfile)
            {
                AvailableImportProfiles.Add(profile);
            }
        }
    }

    /// <summary>
    /// Creates a new <see cref="ImportProfile"/> in the database based on <see cref="SelectedImportProfile"/> data
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateProfile()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedImportProfile.ProfileName)) throw new Exception("Profile Name must not be empty.");

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                SelectedImportProfile.ImportProfileId = 0;
                if (dbContext.CreateImportProfile(SelectedImportProfile) == 0)
                    throw new Exception("Profile could not be created in database.");
                LoadAvailableProfiles();
                SelectedImportProfile = AvailableImportProfiles.First(i => i.ImportProfileId == SelectedImportProfile.ImportProfileId);
            }    
            
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to create Import Profile: {e.Message}");
        }
    }

    /// <summary>
    /// Updates data of the current <see cref="SelectedImportProfile"/> in the database
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveProfile()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedImportProfile.ProfileName)) throw new Exception("Profile Name must not be empty.");

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                dbContext.UpdateImportProfile(SelectedImportProfile);

                LoadAvailableProfiles();
                SelectedImportProfile = AvailableImportProfiles.First(i => i.ImportProfileId == SelectedImportProfile.ImportProfileId);
            }        
            
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to save Import Profile: {e.Message}");
        }
    }

    /// <summary>
    /// Deletes the <see cref="ImportProfile"/> in the database based on <see cref="SelectedImportProfile"/>
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteProfile()
    {
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                dbContext.DeleteImportProfile(SelectedImportProfile);
            }                
            ResetLoadedProfileData();
            LoadAvailableProfiles();

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to delete Import Profile: {e.Message}");
        }
    }
}
