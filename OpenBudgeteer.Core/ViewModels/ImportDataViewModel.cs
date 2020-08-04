using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Tokenizer.RFC4180;
using TinyCsvParser.TypeConverter;

namespace OpenBudgeteer.Core.ViewModels
{
    public class ImportDataViewModel : ViewModelBase
    {
        private class CsvBankTransactionMapping : CsvMapping<BankTransaction>
        {
            public CsvBankTransactionMapping(ImportProfile importProfile, IEnumerable<string> identifiedColumns) : base()
            {
                // TODO Add User Input for CultureInfo for Amount & TransactionDate conversion

                MapProperty(identifiedColumns.ToList().IndexOf(importProfile.AmountColumnName), x => x.Amount, new DecimalConverter(new CultureInfo(importProfile.NumberFormat)));
                MapProperty(identifiedColumns.ToList().IndexOf(importProfile.MemoColumnName), x => x.Memo);
                if (!string.IsNullOrEmpty(importProfile.PayeeColumnName))
                {
                    MapProperty(identifiedColumns.ToList().IndexOf(importProfile.PayeeColumnName), x => x.Payee);
                }
                MapProperty(identifiedColumns.ToList().IndexOf(importProfile.TransactionDateColumnName), x => x.TransactionDate, new DateTimeConverter(importProfile.DateFormat));
            }
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set => Set(ref _filePath, value);
        }

        private string _fileText;
        public string FileText
        {
            get => _fileText;
            set => Set(ref _fileText, value);
        }

        private Account _selectedAccount;
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set => Set(ref _selectedAccount, value);
        }

        private ImportProfile _selectedImportProfile;
        public ImportProfile SelectedImportProfile
        {
            get => _selectedImportProfile;
            set => Set(ref _selectedImportProfile, value);
        }

        private string _profileName;
        public string ProfileName
        {
            get => _profileName;
            set => Set(ref _profileName, value);
        }

        private int _headerRow;
        public int HeaderRow
        {
            get => _headerRow;
            set => Set(ref _headerRow, value);
        }

        private char _delimiter;
        public char Delimiter
        {
            get => _delimiter;
            set => Set(ref _delimiter, value);
        }

        private char _textQualifier;
        public char TextQualifier
        {
            get => _textQualifier;
            set => Set(ref _textQualifier, value);
        }

        private string _dateFormat;
        public string DateFormat
        {
            get => _dateFormat;
            set => Set(ref _dateFormat, value);
        }

        private string _numberFormat;
        public string NumberFormat
        {
            get => _numberFormat;
            set => Set(ref _numberFormat, value);
        }

        private string _transactionDateColumn;
        public string TransactionDateColumn
        {
            get => _transactionDateColumn;
            set => Set(ref _transactionDateColumn, value);
        }

        private string _payeeColumn;
        public string PayeeColumn
        {
            get => _payeeColumn;
            set => Set(ref _payeeColumn, value);
        }

        private string _memoColumn;
        public string MemoColumn
        {
            get => _memoColumn;
            set => Set(ref _memoColumn, value);
        }

        private string _amountColumn;
        public string AmountColumn
        {
            get => _amountColumn;
            set => Set(ref _amountColumn, value);
        }

        private int _totalRecords;
        public int TotalRecords
        {
            get => _totalRecords;
            set => Set(ref _totalRecords, value);
        }

        private int _recordsWithErrors;
        public int RecordsWithErrors
        {
            get => _recordsWithErrors;
            set => Set(ref _recordsWithErrors, value);
        }

        private int _validRecords;
        public int ValidRecords
        {
            get => _validRecords;
            set => Set(ref _validRecords, value);
        }

        private bool _isModificationEnabled;
        public bool IsModificationEnabled
        {
            get => _isModificationEnabled;
            set => Set(ref _isModificationEnabled, value);
        }

        private bool _isColumnMappingSettingVisible;
        public bool IsColumnMappingSettingVisible
        {
            get => _isColumnMappingSettingVisible;
            set => Set(ref _isColumnMappingSettingVisible, value);
        }

        private ObservableCollection<ImportProfile> _availableImportProfiles;
        public ObservableCollection<ImportProfile> AvailableImportProfiles
        {
            get => _availableImportProfiles;
            set => Set(ref _availableImportProfiles, value);
        }

        private ObservableCollection<Account> _availableAccounts;
        public ObservableCollection<Account> AvailableAccounts
        {
            get => _availableAccounts;
            set => Set(ref _availableAccounts, value);
        }

        private ObservableCollection<string> _identifiedColumns;
        public ObservableCollection<string> IdentifiedColumns
        {
            get => _identifiedColumns;
            set => Set(ref _identifiedColumns, value);
        }

        private ObservableCollection<CsvMappingResult<BankTransaction>> _parsedRecords;
        public ObservableCollection<CsvMappingResult<BankTransaction>> ParsedRecords
        {
            get => _parsedRecords;
            set => Set(ref _parsedRecords, value);
        }
        
        private bool _isProfileValid;
        private string[] _fileLines;
        private readonly DbContextOptions<DatabaseContext> _dbOptions;

        public ImportDataViewModel(DbContextOptions<DatabaseContext> dbOptions)
        {
            AvailableImportProfiles = new ObservableCollection<ImportProfile>();
            AvailableAccounts = new ObservableCollection<Account>();
            IdentifiedColumns = new ObservableCollection<string>();
            ParsedRecords = new ObservableCollection<CsvMappingResult<BankTransaction>>();
            SelectedImportProfile = new ImportProfile();
            SelectedAccount = new Account();
            _dbOptions = dbOptions;
        }

        public Tuple<bool, string> LoadData()
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
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Error during loading: {e.Message}");
            }           
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool, string> HandleOpenFile(string[] dialogResults)
        {
            try
            {
                if (!dialogResults.Any()) return new Tuple<bool, string>(true, string.Empty);
                FilePath = dialogResults[0];
                FileText = File.ReadAllText(FilePath, Encoding.GetEncoding(1252));
                _fileLines = File.ReadAllLines(FilePath, Encoding.GetEncoding(1252));

                IsModificationEnabled = true;
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Error during loading: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
            
        }

        public async Task<Tuple<bool, string>> HandleOpenFileAsync(Stream stream)
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

                IsModificationEnabled = true;
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Unable to open file: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
            
        }

        public Tuple<bool, string> LoadProfile()
        {
            try
            {
                ResetLoadedProfileData();
                ProfileName = SelectedImportProfile.ProfileName;
                IsColumnMappingSettingVisible = true;

                // Set target Account
                if (AvailableAccounts.Any(i => i.AccountId == SelectedImportProfile.AccountId))
                {
                    SelectedAccount = AvailableAccounts.First(i => i.AccountId == SelectedImportProfile.AccountId);
                }

                HeaderRow = SelectedImportProfile.HeaderRow;
                Delimiter = SelectedImportProfile.Delimiter;
                TextQualifier = SelectedImportProfile.TextQualifier;
                DateFormat = SelectedImportProfile.DateFormat;
                NumberFormat = SelectedImportProfile.NumberFormat;
                var (success, errorMessage) = LoadHeaders();
                if (!success) throw new Exception(errorMessage);

                ValidateData();

                _isProfileValid = true;
            }
            catch (Exception e)
            {
                _isProfileValid = false;
                return new Tuple<bool, string>(false, $"Unable to load Profile: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool, string> LoadHeaders()
        {
            try
            {
                // Set ComboBox selection for Column Mapping
                IdentifiedColumns.Clear();
                var headerLine = _fileLines[HeaderRow - 1];
                var columns = headerLine.Split(Delimiter);
                foreach (var column in columns)
                {
                    if (column != string.Empty)
                        IdentifiedColumns.Add(column.Trim(TextQualifier)); // Exclude TextQualifier
                }
                AmountColumn = SelectedImportProfile.AmountColumnName;
                MemoColumn = SelectedImportProfile.MemoColumnName;
                PayeeColumn = SelectedImportProfile.PayeeColumnName;
                TransactionDateColumn = SelectedImportProfile.TransactionDateColumnName;
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Unable to load Headers: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        private void ResetLoadedProfileData()
        {
            ProfileName = string.Empty;
            SelectedAccount = new Account();
            HeaderRow = 0;
            Delimiter = char.MinValue;
            DateFormat = string.Empty;
            NumberFormat = string.Empty;
            TextQualifier = char.MinValue;
            AmountColumn = string.Empty;
            MemoColumn = string.Empty;
            PayeeColumn = string.Empty;
            TransactionDateColumn = string.Empty;
            IsColumnMappingSettingVisible = false;
            ParsedRecords.Clear();
            TotalRecords = 0;
            RecordsWithErrors = 0;
            ValidRecords = 0;
        }

        public string ValidateData()
        {
            try
            {
                // Create a new interim Import Profile considering unsaved changes (e.g. in column mapping)
                var interimProfile = new ImportProfile()
                {
                    AccountId = SelectedAccount.AccountId,
                    HeaderRow = HeaderRow,
                    Delimiter = Delimiter,
                    TextQualifier = TextQualifier,
                    DateFormat = DateFormat,
                    NumberFormat = NumberFormat,
                    TransactionDateColumnName = TransactionDateColumn,
                    PayeeColumnName = PayeeColumn,
                    MemoColumnName = MemoColumn,
                    AmountColumnName = AmountColumn
                };

                // Pre-Load Data for verification
                // Initialize CsvReader
                var options = new Options(TextQualifier, '\\', Delimiter);
                var tokenizer = new RFC4180Tokenizer(options);
                var csvParserOptions = new CsvParserOptions(true, tokenizer);
                var csvReaderOptions = new CsvReaderOptions(new[] { Environment.NewLine });
                var csvMapper = new CsvBankTransactionMapping(interimProfile, IdentifiedColumns);
                var csvParser = new CsvParser<BankTransaction>(csvParserOptions, csvMapper);

                // Read File and Skip rows based on HeaderRow
                var stringBuilder = new StringBuilder();
                for (int i = interimProfile.HeaderRow-1; i < _fileLines.Length; i++)
                {
                    stringBuilder.AppendLine(_fileLines[i]);
                }

                // Parse Csv File
                var parsedResults = csvParser.ReadFromString(csvReaderOptions, stringBuilder.ToString()).ToList();

                ParsedRecords.Clear();
                foreach (var parsedResult in parsedResults)
                {
                    ParsedRecords.Add(parsedResult);
                }

                TotalRecords = parsedResults.Count;
                RecordsWithErrors = parsedResults.Count(i => !i.IsValid);
                ValidRecords = parsedResults.Count(i => i.IsValid);

                if (ValidRecords > 0) _isProfileValid = true;
                return string.Empty;
            }
            catch (Exception e)
            {
                TotalRecords = 0;
                RecordsWithErrors = 0;
                ValidRecords = 0;
                ParsedRecords.Clear();
                return e.Message;
            }
        }

        public Tuple<bool, string> ImportData()
        {
            if (!_isProfileValid) 
                return new Tuple<bool, string>(false, "Unable to Import Data as current settings are invalid.");

            var importedCount = 0;
            ValidateData();
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var newRecords = new List<BankTransaction>();
                        foreach (var parsedRecord in ParsedRecords.Where(i => i.IsValid))
                        {
                            var newRecord = parsedRecord.Result;
                            newRecord.AccountId = SelectedAccount.AccountId;
                            newRecords.Add(newRecord);
                        }
                        importedCount = dbContext.CreateBankTransactions(newRecords);
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return new Tuple<bool, string>(false, $"Unable to Import Data. Error message: {e.Message}");
                    }
                }
            }
            
            return new Tuple<bool, string>(true, $"Successfully imported {importedCount} records.");
        }

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

        public Tuple<bool, string> CreateProfile()
        {
            try
            {
                if (string.IsNullOrEmpty(ProfileName)) throw new Exception("Profile Name must not be empty.");

                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    var newProfile = new ImportProfile()
                    {
                        ImportProfileId = 0,
                        ProfileName = ProfileName,
                        AccountId = SelectedAccount.AccountId,
                        HeaderRow = HeaderRow,
                        Delimiter = Delimiter,
                        TextQualifier = TextQualifier,
                        DateFormat = DateFormat,
                        NumberFormat = NumberFormat,
                        AmountColumnName = AmountColumn,
                        MemoColumnName = MemoColumn,
                        PayeeColumnName = PayeeColumn,
                        TransactionDateColumnName = TransactionDateColumn
                    };
                    if (dbContext.CreateImportProfile(newProfile) == 0)
                        throw new Exception("Profile could not be created in database.");
                    LoadAvailableProfiles();
                    SelectedImportProfile = AvailableImportProfiles.First(i => i.ImportProfileId == newProfile.ImportProfileId);
                }                
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Unable to create Import Profile: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool, string> SaveProfile()
        {
            try
            {
                if (string.IsNullOrEmpty(ProfileName)) throw new Exception("Profile Name must not be empty.");

                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    var currentProfile = new ImportProfile()
                    {
                        ImportProfileId = SelectedImportProfile.ImportProfileId,
                        ProfileName = ProfileName,
                        AccountId = SelectedAccount.AccountId,
                        HeaderRow = HeaderRow,
                        Delimiter = Delimiter,
                        TextQualifier = TextQualifier,
                        DateFormat = DateFormat,
                        NumberFormat = NumberFormat,
                        AmountColumnName = AmountColumn,
                        MemoColumnName = MemoColumn,
                        PayeeColumnName = PayeeColumn,
                        TransactionDateColumnName = TransactionDateColumn
                    };
                    dbContext.UpdateImportProfile(currentProfile);

                    LoadAvailableProfiles();
                    SelectedImportProfile = AvailableImportProfiles.First(i => i.ImportProfileId == currentProfile.ImportProfileId);
                }                
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Unable to save Import Profile: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool, string> DeleteProfile()
        {
            try
            {
                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    dbContext.DeleteImportProfile(SelectedImportProfile);
                }                
                ResetLoadedProfileData();
                LoadAvailableProfiles();
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Unable to delete Import Profile: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }
    }
}
