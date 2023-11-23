using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Tokenizer.RFC4180;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

public class ImportPageViewModel : ViewModelBase
{
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
        set
        {
            if (value == _fileText) return;
            Set(ref _fileText, value);
            _fileLines = value.Split(Environment.NewLine);
        }
    }

    private ImportProfileViewModel _selectedImportProfile;
    /// <summary>
    /// Selected profile with import settings from the database
    /// </summary>
    public ImportProfileViewModel SelectedImportProfile
    {
        get => _selectedImportProfile;
        set
        {
            // Load Headers already to prevent hiccups with other SelectedItem properties from Column Mappings
            // as they depend on SelectedImportProfile   
            if (value is { HeaderRow: > 0 }) LoadHeaders(value);
            Set(ref _selectedImportProfile, value);    
        }
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

    /// <summary>
    /// Available <see cref="ImportProfile"/> in the database
    /// </summary>
    public readonly ObservableCollection<ImportProfileViewModel> AvailableImportProfiles;

    /// <summary>
    /// Helper collection to list all available <see cref="Account"/> in the database
    /// </summary>
    public readonly ObservableCollection<AccountViewModel> AvailableAccounts;

    /// <summary>
    /// Collection of columns that have been identified in the CSV file
    /// </summary>
    public readonly ObservableCollection<string> IdentifiedColumns;

    /// <summary>
    /// Results of <see cref="TinyCsvParser"/>
    /// </summary>
    public readonly ObservableCollection<CsvMappingResult<ParsedBankTransaction>> ParsedRecords;

    /// <summary>
    /// Collection of all parsed CSV records which are a potential duplicate of existing <see cref="BankTransaction"/> 
    /// </summary>
    public readonly ObservableCollection<Tuple<CsvMappingResult<ParsedBankTransaction>, List<BankTransaction>>> Duplicates;

    private bool _isProfileValid;
    private string[]? _fileLines;

    public const string DummyColumn = "---Select Column---";

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    public ImportPageViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
        AvailableImportProfiles = new ObservableCollection<ImportProfileViewModel>();
        AvailableAccounts = new ObservableCollection<AccountViewModel>();
        IdentifiedColumns = new ObservableCollection<string>();
        ParsedRecords = new ObservableCollection<CsvMappingResult<ParsedBankTransaction>>();
        Duplicates = new ObservableCollection<Tuple<CsvMappingResult<ParsedBankTransaction>, List<BankTransaction>>>();
        _filePath = string.Empty;
        _fileText = string.Empty;
        _selectedImportProfile = ImportProfileViewModel.CreateDummy(serviceManager);
    }

    /// <summary>
    /// Initialize ViewModel and load data from database
    /// </summary>
    /// <returns></returns>
    public ViewModelOperationResult LoadData()
    {
        try
        {
            // Handle available ImportProfiles
            LoadAvailableProfiles();
            SelectedImportProfile = ImportProfileViewModel.CreateDummy(ServiceManager); // Pre-select Dummy Import Profile
            
            // Handle available Accounts
            AvailableAccounts.Clear();
            foreach (var account in ServiceManager.AccountService.GetActiveAccounts())
            {
                AvailableAccounts.Add(AccountViewModel.CreateFromAccount(ServiceManager, account));
            }
            var dummyAccount = AccountViewModel.CreateEmpty(ServiceManager);
            dummyAccount.Name = "---Select Target Account---";
            AvailableAccounts.Insert(0, dummyAccount);
            
            // Handle Dummy Column
            IdentifiedColumns.Insert(0, DummyColumn);
            
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
            FileText = File.ReadAllText(FilePath, Encoding.GetEncoding("utf-8"));

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
            var stringBuilder = new StringBuilder();
            FilePath = string.Empty;

            using var lineReader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
            var line = await lineReader.ReadLineAsync();
            while(line != null)
            {
                stringBuilder.AppendLine(line);
                line = await lineReader.ReadLineAsync();
            }

            FileText = stringBuilder.ToString();

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to open file: {e.Message}");
        }
    }

    /// <summary>
    /// Reset all figures and parsed records
    /// </summary>
    public void ResetLoadFigures()
    {
        ParsedRecords.Clear();
        TotalRecords = 0;
        RecordsWithErrors = 0;
        ValidRecords = 0;
        PotentialDuplicates = 0;
        Duplicates.Clear();
    }

    /// <summary>
    /// Reads column headers from the loaded file
    /// </summary>
    /// <remarks>
    /// Uses settings from current <see cref="SelectedImportProfile"/>
    /// </remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult LoadHeaders()
    {
        var result = LoadHeaders(SelectedImportProfile);
        
        // If possible and necessary make initial selections after loading headers
        //if (IdentifiedColumns.Count == 0) return result;
        var firstSelection = IdentifiedColumns.First();
        SelectedImportProfile.TransactionDateColumnName = firstSelection;
        SelectedImportProfile.PayeeColumnName = firstSelection;
        SelectedImportProfile.MemoColumnName = firstSelection;
        SelectedImportProfile.AmountColumnName = firstSelection;
        SelectedImportProfile.CreditColumnName = firstSelection;
        SelectedImportProfile.CreditColumnIdentifierColumnName = firstSelection;
        
        return result;
    }
    
    /// <summary>
    /// Reads column headers from the loaded file
    /// </summary>
    /// <param name="importProfile"><see cref="ImportProfileViewModel"/> containing the settings how to parse the headers</param>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult LoadHeaders(ImportProfileViewModel importProfile)
    {
        try
        {
            if (_fileLines == null) throw new Exception("File content not loaded.");
            if (importProfile.HeaderRow < 1 || importProfile.HeaderRow > _fileLines.Length)
                throw new Exception("Cannot read headers with given header row.");

            // Collect Columns for Column Mapping selection
            IdentifiedColumns.Clear();
            IdentifiedColumns.Add(DummyColumn);
            var headerLine = _fileLines[importProfile.HeaderRow - 1];
            var columns = headerLine.Split(importProfile.Delimiter);
            foreach (var column in columns)
            {
                if (!string.IsNullOrEmpty(column))
                    IdentifiedColumns.Add(column.Trim(importProfile.TextQualifier)); // Exclude TextQualifier
            }
            
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to load Headers: {e.Message}");
        }
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
            // Run pre-checks
            if (string.IsNullOrEmpty(SelectedImportProfile.NumberFormat)) throw new Exception("Missing Number Format");
            if (string.IsNullOrEmpty(SelectedImportProfile.DateFormat)) throw new Exception("Missing Date Format");
            if (string.IsNullOrEmpty(SelectedImportProfile.MemoColumnName)) throw new Exception("Missing Mapping for Memo");
            if (string.IsNullOrEmpty(SelectedImportProfile.TransactionDateColumnName)) throw new Exception("Missing Mapping for Transaction Date");
            if (string.IsNullOrEmpty(SelectedImportProfile.AmountColumnName)) throw new Exception("Missing Mapping for Amount");
            if (SelectedImportProfile.Account.AccountId == Guid.Empty) throw new Exception("No target account selected");
            if (_fileLines == null) throw new Exception("File content not loaded.");

            // Remove DummyColumn to prevent wrong column index
            IdentifiedColumns.Remove(DummyColumn);

            // Pre-Load Data for verification
            // Initialize CsvReader
            var options = new Options(SelectedImportProfile.TextQualifier, '\\', SelectedImportProfile.Delimiter);
            var tokenizer = new RFC4180Tokenizer(options);
            var csvParserOptions = new CsvParserOptions(true, tokenizer);
            var csvReaderOptions = new CsvReaderOptions(new[] { Environment.NewLine });
            var csvMapper = new CsvBankTransactionMapping(SelectedImportProfile, IdentifiedColumns);
            var csvParser = new CsvParser<ParsedBankTransaction>(csvParserOptions, csvMapper);

            // Read File and Skip rows based on HeaderRow
            var stringBuilder = new StringBuilder();
            for (int i = SelectedImportProfile.HeaderRow - 1; i < _fileLines.Length; i++)
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
        finally
        {
            // Ensure that DummyColumn still exists
            if (!IdentifiedColumns.Any() || 
                IdentifiedColumns.First() != DummyColumn) 
                IdentifiedColumns.Insert(0, DummyColumn);
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
        await Task.Run(() =>
        {
            var transactions = ServiceManager.BankTransactionService.GetAll().ToList();
            var parsedRecords = ParsedRecords
                .Where(i => i.IsValid)
                .ToList();

            // GroupJoin transactions and parsedRecords with the match logic implemented in DuplicateMatchComparer
            var matchQuery = parsedRecords
                .GroupJoin(
                    transactions,
                    i => i.Result.AsBankTransaction(),
                    j => j,
                    (parsedRecord, matches) => 
                        new { ParsedRecord = parsedRecord, MatchList = matches.ToList() },
                    new DuplicateMatchComparer())
                .ToList();

            foreach (var matchQueryResults in matchQuery.Where(i => i.MatchList.Count > 0))
            {
                Duplicates.Add(
                    new Tuple<CsvMappingResult<ParsedBankTransaction>, 
                        List<BankTransaction>>(matchQueryResults.ParsedRecord, 
                        matchQueryResults.MatchList));
            }
        });
    }

    /// <summary>
    /// Removes the passed duplicate from the parsed records to exclude it from import
    /// </summary>
    /// <param name="duplicate">Duplicate that should be excluded</param>
    public void ExcludeDuplicateRecord(Tuple<CsvMappingResult<ParsedBankTransaction>, List<BankTransaction>> duplicate)
    {
        ParsedRecords.Remove(duplicate.Item1);
        Duplicates.Remove(duplicate);
        UpdateCountValues();
    }

    /// <summary>
    /// Uses data from <see cref="ParsedRecords"/> to store it in the database
    /// </summary>
    /// <param name="withoutDuplicates">Ignore records identified as potential duplicate</param>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> ImportDataAsync(bool withoutDuplicates = true)
    {
        if (!_isProfileValid) return new ViewModelOperationResult(false, "Unable to Import Data as current settings are invalid.");
        return await Task.Run(() =>
        {
            try
            {
                var newRecords = new List<BankTransaction>();
                var recordsToImport = ParsedRecords
                    .Where(i => i.IsValid)
                    .ToList();

                if (withoutDuplicates && Duplicates.Any())
                {
                    recordsToImport.RemoveAll(i => Duplicates
                        .Select(j => j.Item1)
                        .Contains(i));
                }

                foreach (var newRecord in recordsToImport.Select(i => i.Result.AsBankTransaction()))
                {
                    newRecord.AccountId = SelectedImportProfile.Account.AccountId;
                    newRecords.Add(newRecord);
                }
                var result = ServiceManager.BankTransactionService
                    .ImportTransactions(newRecords)
                    .ToList();

                return new ViewModelOperationResult(true, $"Successfully imported {result.Count} records.");
            }
            catch (Exception e)
            {
                return new ViewModelOperationResult(false, $"Unable to Import Data. Error message: {e.Message}");
            }
        });
    }

    /// <summary>
    /// Helper method to load <see cref="ImportProfile"/> from the database
    /// </summary>
    private void LoadAvailableProfiles()
    {
        AvailableImportProfiles.Clear();
        foreach (var profile in ServiceManager.ImportProfileService.GetAll())
        {
            AvailableImportProfiles.Add(ImportProfileViewModel.CreateFromImportProfile(ServiceManager, profile));
        }
        var dummyImportProfile = ImportProfileViewModel.CreateDummy(ServiceManager);
        AvailableImportProfiles.Insert(0, dummyImportProfile);
    }

    /// <summary>
    /// Creates a new <see cref="ImportProfile"/> in the database based on <see cref="SelectedImportProfile"/> data
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateProfile()
    {
        try
        {
            var result = SelectedImportProfile.CreateProfile();
            if (!result.IsSuccessful) throw new Exception(result.Message);
            LoadAvailableProfiles();
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }

    /// <summary>
    /// Updates data of the current <see cref="SelectedImportProfile"/> in the database
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveProfile()
    {
        var result = SelectedImportProfile.SaveProfile();
        LoadAvailableProfiles();
        
        return result;
    }

    /// <summary>
    /// Deletes the <see cref="ImportProfile"/> from the database based on <see cref="SelectedImportProfile"/>
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteProfile()
    {
        try
        {
            var result = SelectedImportProfile.DeleteProfile();
            if (!result.IsSuccessful) throw new Exception(result.Message);
            LoadAvailableProfiles();
            SelectedImportProfile = ImportProfileViewModel.CreateDummy(ServiceManager);
            AvailableImportProfiles.Insert(0, SelectedImportProfile);

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }
}