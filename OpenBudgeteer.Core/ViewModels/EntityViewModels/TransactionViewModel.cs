using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class TransactionViewModel : BaseEntityViewModel<BankTransaction>
{
    #region Properties & Fields
    
    /// <summary>
    /// Database Id of the BankTransaction
    /// </summary>
    public readonly Guid TransactionId;
    
    private AccountViewModel _selectedAccount;
    /// <summary>
    /// ViewModel instance of the selected Account
    /// </summary>
    public AccountViewModel SelectedAccount
    {
        get => _selectedAccount; 
        set => Set(ref _selectedAccount, value);
    }
    
    private DateTime _transactionDate;
    /// <summary>
    /// Booking Date of the BankTransaction 
    /// </summary>
    public DateTime TransactionDate 
    { 
        get => _transactionDate;
        set => Set(ref _transactionDate, value);
    }
    
    private string _payee;
    /// <summary>
    /// Payee of the BankTransaction
    /// </summary>
    public string Payee 
    { 
        get => _payee;
        set => Set(ref _payee, value);
    }
    
    private string _memo;
    /// <summary>
    /// Memo of the BankTransaction
    /// </summary>
    public string Memo 
    { 
        get => _memo;
        set => Set(ref _memo, value);
    }
    
    private decimal _amount;
    /// <summary>
    /// Amount of the BankTransaction
    /// </summary>
    public decimal Amount 
    { 
        get => _amount;
        set => Set(ref _amount, value);
    }

    private bool _inModification;
    /// <summary>
    /// Helper property to check if the Transaction is currently modified
    /// </summary>
    public bool InModification
    {
        get => _inModification;
        set => Set(ref _inModification, value);
    }

    private bool _isHovered;
    /// <summary>
    /// Helper property to check if the cursor hovers over the entry in the UI
    /// </summary>
    public bool IsHovered
    {
        get => _isHovered;
        set => Set(ref _isHovered, value);
    }
    
    /// <summary>
    /// Returns the difference between the transaction amount and the sum of all bucket amounts.
    /// </summary>
    public decimal Difference => Amount - Buckets.Sum(b => b.Amount);
    
    private ObservableCollection<PartialBucketViewModel> _buckets;
    /// <summary>
    /// Collection of Buckets which are assigned to this Transaction
    /// </summary>
    public ObservableCollection<PartialBucketViewModel> Buckets
    {
        get => _buckets;
        set => Set(ref _buckets, value);
    }

    /// <summary>
    /// Helper collection to list all existing Account
    /// </summary>
    public readonly ObservableCollection<AccountViewModel> AvailableAccounts;
    
    /// <summary>
    /// Helper collection to list all existing Buckets
    /// </summary>
    private readonly ObservableCollection<Bucket> _availableBuckets;

    private TransactionViewModel? _oldTransactionViewModelItem;
    
    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BankTransaction"/> object (with Buckets)
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableAccounts">List of all available <see cref="Account"/> from database. (Use a cached list here)</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    /// <param name="transaction">Transaction instance</param>
    protected TransactionViewModel(IServiceManager serviceManager, IEnumerable<AccountViewModel>? availableAccounts, 
        IEnumerable<Bucket>? availableBuckets, BankTransaction? transaction) : base(serviceManager)
    {
        _buckets = new();
        _availableBuckets = new();
        AvailableAccounts = new ObservableCollection<AccountViewModel>();
        
        // Handle Accounts
        if (availableAccounts != null)
        {
            foreach (var availableAccount in availableAccounts)
            {
                AvailableAccounts.Add(availableAccount);
            }
        }
        
        // Handle Buckets
        if (availableBuckets != null)
        {
            foreach (var availableBucket in availableBuckets)
            {
                _availableBuckets.Add(availableBucket);
            }
        }

        // Handle Transaction
        if (transaction == null)
        {
            // Add the "No Account" for pre-selection
            var noAccount = new Account
            {
                Id = Guid.Empty,
                IsActive = 1,
                Name = "No Account"
            };
            TransactionId = Guid.Empty;
            _transactionDate = DateTime.Now;
            _payee = string.Empty;
            _memo = string.Empty;
            _amount = 0;
            
            // Add the "No Account" for pre-selection
            _selectedAccount = AccountViewModel.CreateFromAccount(serviceManager, noAccount);
            AvailableAccounts.Add(_selectedAccount);
            
            // Create an empty Bucket Assignment if requested (required for "Create new Transaction")
            if (availableBuckets != null)
            {
                var emptyBucket = PartialBucketViewModel.CreateNoSelection(serviceManager);
                emptyBucket.AmountChanged += CheckBucketAssignments;
                emptyBucket.DeleteAssignmentRequest += DeleteRequestedBucketAssignment;
                Buckets.Add(emptyBucket);
            }
            
        }
        else
        {
            TransactionId = transaction.Id;
            _transactionDate = transaction.TransactionDate;
            _payee = transaction.Payee ?? string.Empty;
            _memo = transaction.Memo ?? string.Empty;
            _amount = transaction.Amount;
            
            // Handle Accounts
            _selectedAccount = AccountViewModel.CreateFromAccount(serviceManager, transaction.Account);
            if (!SelectedAccount.IsActive)
            {
                // Add inactive Account for selection
                AvailableAccounts.Add(SelectedAccount);
            }
            
            // Handle Buckets
            if (availableBuckets == null) return;
                
            // Get all assigned Buckets for this transaction
            var budgetedTransactions = serviceManager.BudgetedTransactionService
                .GetAllFromTransaction(transaction.Id)
                .ToList();
                
            if (budgetedTransactions.Any())
            {
                // Create a PartialBucketViewModel for each assignment
                foreach (var budgetedTransaction in budgetedTransactions)
                {
                    var newItem = PartialBucketViewModel.CreateFromBucket(
                        serviceManager,
                        _availableBuckets.First(i => i.Id == budgetedTransaction.BucketId),
                        budgetedTransaction.Amount);
                    newItem.SelectedBucketOutput = (newItem.Amount != transaction.Amount 
                        ?  $"{newItem.SelectedBucketName} ({newItem.Amount})" 
                        :  newItem.SelectedBucketName) ?? string.Empty;
                    Buckets.Add(newItem);
                }
            }
            else
            {
                // Most likely an imported Transaction where Bucket assignment still needs to be done
                Buckets.Add(PartialBucketViewModel.CreateNoSelection(
                    serviceManager, 
                    transaction.Amount));
            }
                    
            // Subscribe Event Handler for Amount Changes (must be always the last step) and assignment deletion requests
            foreach (var bucket in Buckets)
            {
                bucket.AmountChanged += CheckBucketAssignments;
                bucket.DeleteAssignmentRequest += DeleteRequestedBucketAssignment;
            }
        }
    }

    /// <summary>
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected TransactionViewModel(TransactionViewModel viewModel) : base(viewModel.ServiceManager)
    {
        TransactionId = viewModel.TransactionId;
        _selectedAccount = (AccountViewModel)viewModel.SelectedAccount.Clone();
        _transactionDate = viewModel.TransactionDate;
        _payee = viewModel.Payee;
        _memo = viewModel.Memo;
        _amount = viewModel.Amount;
        _inModification = viewModel.InModification;
        _isHovered = viewModel.IsHovered;
        
        // Handle Buckets
        _buckets = new();
        foreach (var bucket in viewModel.Buckets)
        {
            _buckets.Add((PartialBucketViewModel)bucket.Clone());
        }
        
        // Handle Available Accounts
        AvailableAccounts = new ObservableCollection<AccountViewModel>();
        foreach (var availableAccount in viewModel.AvailableAccounts)
        {
            AvailableAccounts.Add((AccountViewModel)availableAccount.Clone());
        }
        
        // Handle Available Buckets
        _availableBuckets = new();
        foreach (var availableBucket in viewModel._availableBuckets)
        {
            _availableBuckets.Add(availableBucket);
        }
        
        // Subscribe Event Handler for Amount Changes (must be always the last step) and assignment deletion requests
        foreach (var bucket in Buckets)
        {
            bucket.AmountChanged += CheckBucketAssignments;
            bucket.DeleteAssignmentRequest += DeleteRequestedBucketAssignment;
        }
    }
    
    /// <summary>
    /// Initialize ViewModel used to create a new <see cref="BankTransaction"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <returns>New ViewModel instance</returns>
    public static TransactionViewModel CreateEmpty(IServiceManager serviceManager)
    {
        var availableAccounts = serviceManager.AccountService
            .GetActiveAccounts()
            .Select(i => AccountViewModel.CreateFromAccount(serviceManager, i))
            .ToList();
        
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var availableBuckets = serviceManager.BucketService.GetActiveBuckets(currentMonth).ToList();
        var result = new TransactionViewModel(serviceManager, availableAccounts, availableBuckets, null);
        
       return result;
    }
    
    /// <summary>
    /// Initialize and return a new ViewModel based on an existing <see cref="BankTransaction"/> object including
    /// assigned Buckets
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableAccounts">List of all available <see cref="Account"/> from database. (Use a cached list here)</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    /// <param name="transaction">Transaction instance</param>
    /// <returns>New ViewModel instance</returns>
    public static TransactionViewModel CreateFromTransaction(IServiceManager serviceManager, 
        IEnumerable<AccountViewModel> availableAccounts, IEnumerable<Bucket> availableBuckets, BankTransaction transaction)
    {
        //TODO Refactor availableAccounts to Account instead of AccountViewModel
        return new TransactionViewModel(serviceManager, availableAccounts, availableBuckets, transaction);
    }
    
    /// <summary>
    /// Initialize ViewModel based on an existing  <see cref="BankTransaction"/> object including assigned Buckets.
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableAccounts">List of all available <see cref="Account"/> from database. (Use a cached list here)</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    /// <param name="transaction">Transaction instance</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<TransactionViewModel> CreateFromTransactionAsync(IServiceManager serviceManager, 
        IEnumerable<AccountViewModel> availableAccounts, IEnumerable<Bucket> availableBuckets, BankTransaction transaction)
    {
        //TODO Refactor availableAccounts to Account instead of AccountViewModel
        return await Task.Run(() => CreateFromTransaction(serviceManager, availableAccounts, availableBuckets, transaction));
    }

    /// <summary>
    /// Initialize ViewModel for displaying the <see cref="BankTransaction"/> object without assigned Buckets.
    /// Not to be used for any modification purposes
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="transaction">Transaction instance</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<TransactionViewModel> CreateFromTransactionWithoutBucketsAsync(IServiceManager serviceManager, 
        BankTransaction transaction)
    {
        return await Task.Run(() => new TransactionViewModel(serviceManager, null, null, transaction));
    }

    /// <summary>
    /// Initialize ViewModel for displaying a <see cref="BankTransaction"/> transforming the passed <see cref="BucketMovement"/>.
    /// Not to be used for any modification purposes
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucketMovement">BucketMovement which will be transformed</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<TransactionViewModel> CreateFromBucketMovementAsync(IServiceManager serviceManager, 
        BucketMovement bucketMovement)
    {
        return await Task.Run(() =>
        {
            // Simulate a BankTransaction based on BucketMovement
            var transformedMovement = new BankTransaction
            {
                Id = Guid.Empty,
                AccountId = Guid.Empty,
                Account = new Account(){ IsActive = 1},
                Amount = bucketMovement.Amount,
                Memo = "Bucket Movement",
                Payee = string.Empty,
                TransactionDate = bucketMovement.MovementDate,
            };
            return new TransactionViewModel(serviceManager, null, null, transformedMovement);
        });
    }

    /// <summary>
    /// Return a deep copy of the ViewModel
    /// </summary>
    public override object Clone()
    {
        return new TransactionViewModel(this);
    }

    #endregion
    
    #region Modification Handler
    
    internal override BankTransaction ConvertToDto()
    {
        return new BankTransaction()
        {
            Id = TransactionId,
            AccountId = SelectedAccount.AccountId,
            TransactionDate = TransactionDate,
            Payee = Payee,
            Memo = Memo,
            Amount = Amount
        };
    }
    
    internal BankTransaction ConvertToDtoWithBuckets()
    {
        var result = ConvertToDto();
        result.BudgetedTransactions = new List<BudgetedTransaction>();
        foreach (var bucket in Buckets)
        {
            result.BudgetedTransactions.Add(new BudgetedTransaction()
            {
                TransactionId = TransactionId, 
                BucketId = bucket.SelectedBucketId, 
                Amount = bucket.Amount
            });
        }

        return result;
    }
    
    /// <summary>
    /// Adds a bucket item.
    /// </summary>
    /// <param name="amount">Amount that will be assigned to the Bucket</param>
    /// <param name="newBucketItem">Optional: New bucket to be added. If omitted a new, empty bucket will be added.</param>
    /// <remarks>Will add an empty bucket item if a bucket item is not provided.</remarks>
    public void AddBucketItem(decimal amount, PartialBucketViewModel? newBucketItem = null)
    {
        newBucketItem ??= PartialBucketViewModel.CreateNoSelection(ServiceManager);
            
        newBucketItem.AmountChanged += CheckBucketAssignments;
        newBucketItem.DeleteAssignmentRequest += DeleteRequestedBucketAssignment;
        Buckets.Add(newBucketItem);
    }
    
    /// <summary>
    /// Event that checks amount for all assigned Buckets and creates or removes an "empty item"
    /// </summary>
    /// <param name="sender">Object that has triggered the event</param>
    /// <param name="changedArgs">Event Arguments about changed amount</param>
    private void CheckBucketAssignments(object? sender, AmountChangedArgs changedArgs)
    {
        // Check if this current event was triggered while updating the amount for the "emptyItem"
        // Prevents Deadlock and StackOverflowException 
        if (changedArgs.Source.SelectedBucketId == Guid.Empty) return;

        // Calculate total amount assigned to any Bucket
        var assignedAmount = Buckets
            // ignore "emptyItem" where existing Bucket is not yet assigned
            // this is the one where the amount has to be updated
            .Where(i => i.SelectedBucketId != Guid.Empty)
            .Sum(i => i.Amount);

        // Consistency check
        if ((Amount < 0 && assignedAmount > 0) || 
            (Amount > 0 && assignedAmount < 0) ||
            // Check over-provisioning of amount assignment
            (Amount < 0 && Amount - assignedAmount > 0) ||
            (Amount > 0 && Amount - assignedAmount < 0))
        {
            return; // Inconsistency, better to do nothing, Error handling while saving
        }

        // Check if remaining amount left to be assigned to any Bucket
        if (assignedAmount != Amount)
        {
            if (Buckets.Last().SelectedBucketId != Guid.Empty)
            {
                // All items have a valid Bucket assignment, create a new "empty item"
                AddBucketItem(Amount - assignedAmount);
            }
            else
            {
                // "emptyItem" exists, update remaining amount to be assigned
                Buckets.Last().Amount = Amount - assignedAmount;
            }
        }
        else if (Buckets.Last().SelectedBucketId == Guid.Empty)
        {
            // Remove unnecessary "empty item" as amount is already assigned properly
            Buckets.Remove(Buckets.Last());
        }
    }

    /// <summary>
    /// Event that handles the deletion of teh requested Bucket
    /// </summary>
    /// <param name="sender">Object that has triggered the event</param>
    /// <param name="args">Event Arguments about deletion request</param>
    private void DeleteRequestedBucketAssignment(object? sender, DeleteAssignmentRequestArgs args)
    {
        // Prevent deletion all last remaining BucketAssignment
        if (Buckets.Count > 1)
        {
            Buckets.Remove(args.Source);
        }
    }

    /// <summary>
    /// Creates or updates a record in the database based ViewModel data
    /// </summary>
    /// <remarks>(Re)Creates also <see cref="BudgetedTransaction"/> records for each assigned Bucket</remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateOrUpdateTransaction()
    {
        var result = PerformConsistencyCheck(out var canSkipBucketAssignment);
        if (!result.IsSuccessful) return result;

        try
        {
            if (TransactionId == Guid.Empty)
            {
                ServiceManager.BankTransactionService.Create(canSkipBucketAssignment
                    ? ConvertToDto()
                    : ConvertToDtoWithBuckets());
            }
            else
            {
                ServiceManager.BankTransactionService.Update(canSkipBucketAssignment
                    ? ConvertToDto()
                    : ConvertToDtoWithBuckets());
            }
            
            _oldTransactionViewModelItem = null;
            InModification = false;
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }

    /// <summary>
    /// Executes several data consistency checks (e.g. Bucket assignment, pending amount etc.) to see if changes
    /// can be stored in the database 
    /// </summary>
    /// <param name="skipBucketAssignment">Exclude checks on Bucket assignment</param>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult PerformConsistencyCheck(out bool skipBucketAssignment)
    {
        decimal assignedAmount = 0;
        skipBucketAssignment = false;

        // Consistency and Validity Checks
        if (SelectedAccount.AccountId == Guid.Empty) return new ViewModelOperationResult(false, "No Bank account selected.");
        if (!SelectedAccount.IsActive) return new ViewModelOperationResult(false, "The selected Bank account is inactive.");
        if (Buckets.Count == 0) return new ViewModelOperationResult(false, "No Bucket assigned to this Transaction.");
        
        foreach (var assignedBucket in Buckets)
        {
            if (assignedBucket.SelectedBucketId == Guid.Empty)
            {
                if (assignedBucket.SelectedBucketName == "No Selection")
                {
                    // Imported Transaction where Bucket assignment is pending
                    // Allow Transaction Update but Skip DB Updates for Bucket assignment
                    skipBucketAssignment = true;
                }
                else
                {
                    return new ViewModelOperationResult(false, "Pending Bucket assignment for this Transaction.");
                }
            }

            assignedAmount += assignedBucket.Amount;
        }

        return assignedAmount == Amount
            ? new ViewModelOperationResult(true)
            : new ViewModelOperationResult(false, "Amount between Bucket assignment and Transaction not consistent.");

    }

    /// <summary>
    /// Removes a record in the database based on ViewModel data
    /// </summary>
    /// <remarks>Removes also all its assigned Buckets</remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteTransaction()
    {
        try
        {
            ServiceManager.BankTransactionService.Delete(TransactionId);
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }

    public void StartModification()
    {
        _oldTransactionViewModelItem = new TransactionViewModel(this);
        InModification = true;
    }

    public void CancelModification()
    {
        if (_oldTransactionViewModelItem == null) return;
        
        SelectedAccount = _oldTransactionViewModelItem.SelectedAccount;
        TransactionDate = _oldTransactionViewModelItem.TransactionDate;
        Payee = _oldTransactionViewModelItem.Payee;
        Memo = _oldTransactionViewModelItem.Memo;
        Amount = _oldTransactionViewModelItem.Amount;
        Buckets.Clear();
        foreach (var bucket in _oldTransactionViewModelItem.Buckets)
        {
            Buckets.Add(bucket);
        }
        InModification = false;
        _oldTransactionViewModelItem = null;
    }

    #endregion
    
    #region Misc

    public void ProposeBucket()
    {
        var proposal = CheckMappingRules();
        if (proposal == null) return;
        Buckets.Clear();
        Buckets.Add(PartialBucketViewModel.CreateFromBucket(
            ServiceManager, 
            proposal, 
            Amount));
    }

    private Bucket? CheckMappingRules()
    {
        var targetBucketId = Guid.Empty;
        foreach (var ruleSet in ServiceManager.BucketRuleSetService.GetAll())
        {
            if (!ServiceManager.BucketRuleSetService.GetMappingRules(ruleSet.Id).All(DoesRuleApply)) continue;
            targetBucketId = ruleSet.TargetBucketId;
            break;
        }

        return targetBucketId != Guid.Empty ? ServiceManager.BucketService.Get(targetBucketId) : null;

        bool DoesRuleApply(MappingRule mappingRule)
        {
            var cleanedComparisionValue = mappingRule.ComparisonValue.ToLower();
            return mappingRule.ComparisonType switch
            {
                1 => cleanedComparisionValue == GetFieldValue(mappingRule.ComparisonField),
                2 => cleanedComparisionValue != GetFieldValue(mappingRule.ComparisonField),
                3 => GetFieldValue(mappingRule.ComparisonField).Contains(cleanedComparisionValue),
                4 => !GetFieldValue(mappingRule.ComparisonField).Contains(cleanedComparisionValue),
                _ => false
            };
        }

        string GetFieldValue(int comparisionField)
        {
            var fieldValue = comparisionField switch
            {
                1 => SelectedAccount.Name,
                2 => Payee,
                3 => Memo,
                4 => Amount.ToString(CultureInfo.CurrentCulture),
                _ => string.Empty
            };
            return fieldValue!.ToLower();
        }
    }
    
    #endregion
}