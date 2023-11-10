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

public class TransactionViewModel : ViewModelBase
{
    private BankTransaction _transaction;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public BankTransaction Transaction
    {
        get => _transaction;
        private set => Set(ref _transaction, value);
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
    public decimal Difference => Transaction.Amount - Buckets.Sum(b => b.Amount);
    
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
    public readonly ObservableCollection<Account> AvailableAccounts;
    
    /// <summary>
    /// Helper collection to list all existing Buckets
    /// </summary>
    public readonly ObservableCollection<Bucket> AvailableBuckets;

    private TransactionViewModel? _oldTransactionViewModelItem;
    private DateTime CurrentMonth => new DateTime(
        Transaction.TransactionDate.Year, Transaction.TransactionDate.Month, 1);
    
    /// <summary>
    /// Initialize ViewModel with an existing <see cref="BankTransaction"/> object (with Buckets)
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableAccounts">List of all available <see cref="Account"/> from database. (Use a cached list here)</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    /// <param name="transaction">Transaction instance</param>
    protected TransactionViewModel(IServiceManager serviceManager, IEnumerable<Account>? availableAccounts, 
        IEnumerable<Bucket>? availableBuckets, BankTransaction? transaction) : base(serviceManager)
    {
        _buckets = new();
        AvailableBuckets = new();
        AvailableAccounts = new ObservableCollection<Account>();
        
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
                AvailableBuckets.Add(availableBucket);
            }
        }

        if (transaction == null)
        {
            var noAccount = new Account
            {
                Id = Guid.Empty,
                IsActive = 1,
                Name = "No Account"
            };
            _transaction = new BankTransaction()
            {
                TransactionDate = DateTime.Now,
                Account = noAccount
            };
            
            // Create an empty Bucket Assignment if requested (required for "Create new Transaction")
            if (availableBuckets != null)
            {
                var emptyBucket = PartialBucketViewModel.CreateNoSelection(serviceManager, AvailableBuckets);
                emptyBucket.AmountChanged += CheckBucketAssignments;
                emptyBucket.DeleteAssignmentRequest += DeleteRequestedBucketAssignment;
                Buckets.Add(emptyBucket);
            }
            
            // Add the "No Account" for pre-selection
            AvailableAccounts.Add(noAccount);
        }
        else
        {
            // Make a copy of the object to prevent any double Bindings
            _transaction = new BankTransaction
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                Account = new Account()
                {
                    Id = transaction.Account.Id,
                    Name = transaction.Account.Name,
                    IsActive = transaction.Account.IsActive
                },
                Amount = transaction.Amount,
                Memo = transaction.Memo,
                Payee = transaction.Payee,
                TransactionDate = transaction.TransactionDate
            };

            // Handle Buckets
            if (availableBuckets == null) return;
                
            // Get all assigned Buckets for this transaction
            var assignedBuckets = ServiceManager.BudgetedTransactionService
                .GetAllFromTransaction(transaction.Id)
                .ToList();
                
            if (assignedBuckets.Any())
            {
                // Create a PartialBucketViewModel for each assignment
                foreach (var assignedBucket in assignedBuckets)
                {
                    var newItem = PartialBucketViewModel.CreateFromBucketWithAmount(
                        serviceManager, 
                        AvailableBuckets,
                        ServiceManager.BucketService.Get(assignedBucket.BucketId),
                        assignedBucket.Amount);
                    newItem.SelectedBucketOutput = newItem.Amount != transaction.Amount 
                        ?  $"{newItem.SelectedBucket.Name} ({newItem.Amount})" 
                        :  newItem.SelectedBucket.Name;
                    Buckets.Add(newItem);
                }
            }
            else
            {
                // Most likely an imported Transaction where Bucket assignment still needs to be done
                Buckets.Add(PartialBucketViewModel.CreateNoSelection(
                    serviceManager, 
                    AvailableBuckets, 
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
    /// Initialize ViewModel used to create a new <see cref="BankTransaction"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <returns>New ViewModel instance</returns>
    public static TransactionViewModel CreateEmpty(IServiceManager serviceManager)
    {
        var availableAccounts = serviceManager.AccountService.GetActiveAccounts().ToList();
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
        IEnumerable<Account> availableAccounts, IEnumerable<Bucket> availableBuckets, BankTransaction transaction)
    {
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
        IEnumerable<Account> availableAccounts, IEnumerable<Bucket> availableBuckets, BankTransaction transaction)
    {
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
                Amount = bucketMovement.Amount,
                Memo = "Bucket Movement",
                Payee = string.Empty,
                TransactionDate = bucketMovement.MovementDate,
            };
            return new TransactionViewModel(serviceManager, null, null, transformedMovement);
        });
    }
    
    /// <summary>
    /// Adds a bucket item.
    /// </summary>
    /// <param name="amount">Amount that will be assigned to the Bucket</param>
    /// <param name="newBucketItem">Optional: New bucket to be added. If omitted a new, empty bucket will be added.</param>
    /// <remarks>Will add an empty bucket item if a bucket item is not provided.</remarks>
    public void AddBucketItem(decimal amount, PartialBucketViewModel? newBucketItem = null)
    {
        if (newBucketItem == null)
        {
            var availableBuckets = ServiceManager.BucketService.GetActiveBuckets(CurrentMonth).ToList();
            newBucketItem = PartialBucketViewModel.CreateNoSelection(ServiceManager, availableBuckets);
        }
            
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
        if (changedArgs.Source.SelectedBucket.Id == Guid.Empty) return;

        // Calculate total amount assigned to any Bucket
        var assignedAmount = Buckets
            // ignore "emptyItem" where existing Bucket is not yet assigned
            // this is the one where the amount has to be updated
            .Where(i => i.SelectedBucket.Id != Guid.Empty)
            .Sum(i => i.Amount);

        // Consistency check
        if ((Transaction.Amount < 0 && assignedAmount > 0) || 
            (Transaction.Amount > 0 && assignedAmount < 0) ||
            // Check over-provisioning of amount assignment
            (Transaction.Amount < 0 && Transaction.Amount - assignedAmount > 0) ||
            (Transaction.Amount > 0 && Transaction.Amount - assignedAmount < 0))
        {
            return; // Inconsistency, better to do nothing, Error handling while saving
        }

        // Check if remaining amount left to be assigned to any Bucket
        if (assignedAmount != Transaction.Amount)
        {
            if (Buckets.Last().SelectedBucket.Id != Guid.Empty)
            {
                // All items have a valid Bucket assignment, create a new "empty item"
                AddBucketItem(Transaction.Amount - assignedAmount);
            }
            else
            {
                // "emptyItem" exists, update remaining amount to be assigned
                Buckets.Last().Amount = Transaction.Amount - assignedAmount;
            }
        }
        else if (Buckets.Last().SelectedBucket.Id == Guid.Empty)
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
    /// Creates or updates a record in the database based on <see cref="Transaction"/> object
    /// </summary>
    /// <remarks>(Re)Creates also <see cref="BudgetedTransaction"/> records for each assigned Bucket</remarks>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult CreateOrUpdateTransaction()
    {
        var result = PerformConsistencyCheck(out var canSkipBucketAssignment);
        if (!result.IsSuccessful) return result;

        try
        {
            if (Transaction.Id == Guid.Empty)
            {
                if (canSkipBucketAssignment)
                    ServiceManager.BankTransactionService.Create(Transaction);
                else
                    ServiceManager.BankTransactionService.Create(Transaction, 
                        Buckets
                            .Select(i => new BudgetedTransaction
                            {
                                TransactionId = Transaction.Id, 
                                BucketId = i.SelectedBucket.Id, 
                                Amount = i.Amount
                            }));
            }
            else
            {
                if (canSkipBucketAssignment)
                    ServiceManager.BankTransactionService.Update(Transaction);
                else
                    ServiceManager.BankTransactionService.Update(
                        Transaction, 
                        Buckets
                            .Select(i => new BudgetedTransaction
                            {
                                TransactionId = Transaction.Id, 
                                BucketId = i.SelectedBucket.Id, 
                                Amount = i.Amount
                            }));
            }
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
        //if (Transaction == null) return new ViewModelOperationResult(false, "Errors in Transaction object.");
        if (Transaction.Account.Id == Guid.Empty) return new ViewModelOperationResult(false, "No Bank account selected.");
        if (Buckets.Count == 0) return new ViewModelOperationResult(false, "No Bucket assigned to this Transaction.");
        
        foreach (var assignedBucket in Buckets)
        {
            if (assignedBucket.SelectedBucket.Id == Guid.Empty)
            {
                if (assignedBucket.SelectedBucket.Name == "No Selection")
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

        return assignedAmount == Transaction.Amount
            ? new ViewModelOperationResult(true)
            : new ViewModelOperationResult(false, "Amount between Bucket assignment and Transaction not consistent.");

    }

    /// <summary>
    /// Removes a record in the database based on <see cref="Transaction"/> object
    /// </summary>
    /// <remarks>Removes also all its assigned Buckets</remarks>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult DeleteTransaction()
    {
        try
        {
            ServiceManager.BankTransactionService.Delete(Transaction);
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }

    public void StartModification()
    {
        _oldTransactionViewModelItem = CreateFromTransaction(ServiceManager, AvailableAccounts, AvailableBuckets, Transaction);
        InModification = true;
    }

    public void CancelModification()
    {
        Transaction = _oldTransactionViewModelItem!.Transaction;
        Buckets = _oldTransactionViewModelItem.Buckets;
        InModification = false;
        _oldTransactionViewModelItem = null;
    }

    public ViewModelOperationResult CreateItem()
    {
        Transaction.Id = Guid.Empty; // Triggers CREATE during CreateOrUpdateTransaction()
        return CreateOrUpdateTransaction();
    }

    public ViewModelOperationResult UpdateItem()
    {
        if (Transaction.Id == Guid.Empty) 
            return new ViewModelOperationResult(false, "Transaction needs to be created first in database");

        var result = CreateOrUpdateTransaction();
        if (!result.IsSuccessful)
        {
            return new ViewModelOperationResult(false, result.Message, true);
        }
        _oldTransactionViewModelItem = null;
        InModification = false;

        return new ViewModelOperationResult(true);
    }

    public ViewModelOperationResult DeleteItem()
    {
        var result = DeleteTransaction();
        return result.IsSuccessful 
            ? new ViewModelOperationResult(true, true) 
            : result;
    }

    public void ProposeBucket()
    {
        var proposal = CheckMappingRules();
        if (proposal == null) return;
        Buckets.Clear();
        var availableBuckets = ServiceManager.BucketService.GetActiveBuckets(CurrentMonth).ToList();
        Buckets.Add(PartialBucketViewModel.CreateFromBucketWithAmount(
            ServiceManager,
            availableBuckets, 
            proposal, 
            Transaction.Amount));
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
            var cleanedComparisionValue = mappingRule.ComparisionValue.ToLower();
            return mappingRule.ComparisionType switch
            {
                1 => cleanedComparisionValue == GetFieldValue(mappingRule.ComparisionField),
                2 => cleanedComparisionValue != GetFieldValue(mappingRule.ComparisionField),
                3 => GetFieldValue(mappingRule.ComparisionField).Contains(cleanedComparisionValue),
                4 => !GetFieldValue(mappingRule.ComparisionField).Contains(cleanedComparisionValue),
                _ => false
            };
        }

        string GetFieldValue(int comparisionField)
        {
            return (comparisionField switch
            {
                1 => Transaction.Account.Name,
                2 => Transaction.Payee ?? "",
                3 => Transaction.Memo ?? "",
                4 => Transaction.Amount.ToString(CultureInfo.CurrentCulture),
                _ => string.Empty
            }).ToLower();
        }
    }
}