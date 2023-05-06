using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.EventClasses;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels;

public class TransactionViewModelItem : ViewModelBase
{
    private BankTransaction _transaction;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public BankTransaction Transaction
    {
        get => _transaction;
        internal set => Set(ref _transaction, value);
    }

    private Account _selectedAccount;
    /// <summary>
    /// Account where the Transaction is assigned to
    /// </summary>
    public Account SelectedAccount
    {
        get => _selectedAccount;
        set => Set(ref _selectedAccount, value);
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

    private ObservableCollection<PartialBucketViewModelItem> _buckets;
    /// <summary>
    /// Collection of Buckets which are assigned to this Transaction
    /// </summary>
    public ObservableCollection<PartialBucketViewModelItem> Buckets
    {
        get => _buckets;
        set => Set(ref _buckets, value);
    }

    private ObservableCollection<Account> _availableAccounts;
    /// <summary>
    /// Helper collection to list all existing Account
    /// </summary>
    public ObservableCollection<Account> AvailableAccounts
    {
        get => _availableAccounts;
        set => Set(ref _availableAccounts, value);
    }

    private readonly DbContextOptions<DatabaseContext> _dbOptions;
    private readonly YearMonthSelectorViewModel _yearMonthViewModel;
    private TransactionViewModelItem _oldTransactionViewModelItem;

    /// <summary>
    /// Basic constructor
    /// </summary>
    public TransactionViewModelItem()
    {
        Transaction = new BankTransaction();
        Buckets = new ObservableCollection<PartialBucketViewModelItem>();
        AvailableAccounts = new ObservableCollection<Account>();
    }

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="yearMonthViewModel">YearMonth ViewModel instance</param>
    /// <param name="withEmptyBucket">Optionally creates an empty Bucket Assignment</param>
    public TransactionViewModelItem(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel, 
        bool withEmptyBucket = false) : this()
    {
        _dbOptions = dbOptions;
        _yearMonthViewModel = yearMonthViewModel;

        // Set initial TransactionDate in case of "Create new Transaction"
        Transaction.TransactionDate = _yearMonthViewModel.CurrentMonth;
        // Get all available Accounts for ComboBox selections
        // Add empty Account for empty pre-selection
        AvailableAccounts.Add(new Account
        {
            AccountId = Guid.Empty,
            IsActive = 1,
            Name = "No Account"
        });
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            foreach (var account in dbContext.Account.Where(i => i.IsActive == 1))
            {
                AvailableAccounts.Add(account);
            }
        }            
        SelectedAccount = AvailableAccounts.First();
        // Create an empty Bucket Assignment if requested (required for "Create new Transaction")
        if (withEmptyBucket)
        {
            var emptyBucket = new PartialBucketViewModelItem(dbOptions, yearMonthViewModel.CurrentMonth);
            emptyBucket.AmountChanged += CheckBucketAssignments;
            emptyBucket.DeleteAssignmentRequest += DeleteRequestedBucketAssignment;
            Buckets.Add(emptyBucket);
        }
    }

    /// <summary>
    /// Initialize ViewModel with an existing <see cref="BankTransaction"/> object
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="yearMonthViewModel">YearMonth ViewModel instance</param>
    /// <param name="transaction">Transaction instance</param>
    /// <param name="withBuckets">Include assigned Buckets</param>
    public TransactionViewModelItem(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel, 
        BankTransaction transaction, bool withBuckets = true) : this(dbOptions, yearMonthViewModel)
    {
        if (withBuckets)
        {
            // Get all assigned Buckets for this transaction
            using var dbContext = new DatabaseContext(_dbOptions);
            var assignedBuckets = dbContext.BudgetedTransaction
                .Where(i => i.TransactionId == transaction.TransactionId);

            if (assignedBuckets.Any())
            {
                // Create a PartialBucketViewModelItem for each assignment
                foreach (var assignedBucket in assignedBuckets)
                {
                    using var bucketDbContext = new DatabaseContext(_dbOptions);
                    var newItem = new PartialBucketViewModelItem(_dbOptions,
                        _yearMonthViewModel.CurrentMonth,
                        bucketDbContext.Bucket.FirstOrDefault(i => i.BucketId == assignedBucket.BucketId),
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
                Buckets.Add(new PartialBucketViewModelItem(
                    _dbOptions, 
                    _yearMonthViewModel.CurrentMonth, 
                    new Bucket() { BucketId = Guid.Empty }, 
                    transaction.Amount));
            }
                
            // Subscribe Event Handler for Amount Changes (must be always the last step) and assignment deletion requests
            foreach (var bucket in Buckets)
            {
                bucket.AmountChanged += CheckBucketAssignments;
                bucket.DeleteAssignmentRequest += DeleteRequestedBucketAssignment;
            }
        }
             
        // Make a copy of the object to prevent any double Bindings
        Transaction = new BankTransaction
        {
            TransactionId = transaction.TransactionId,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            Memo = transaction.Memo,
            Payee = transaction.Payee,
            TransactionDate = transaction.TransactionDate
        };
        
        // Pre-selection the right account
        using var accountDbContext = new DatabaseContext(_dbOptions);
        var account = accountDbContext.Account.First(i => i.AccountId == transaction.AccountId);
        if (account is { IsActive: 0 })
        {
            account.Name += " (Inactive)";
            AvailableAccounts.Add(account);
        }
        SelectedAccount = account;
            
        // Pre-select empty Account if no Account was found (for new BankTransaction())
        SelectedAccount ??= AvailableAccounts.First();
    }

    /// <summary>
    /// Initialize ViewModel and transform passed <see cref="BucketMovement"/> into a <see cref="BankTransaction"/>
    /// </summary>
    /// <param name="bucketMovement">BucketMovement which will be transformed</param>
    public TransactionViewModelItem(BucketMovement bucketMovement) : this()
    {
        // Simulate a BankTransaction based on BucketMovement
        Transaction = new BankTransaction
        {
            TransactionId = Guid.Empty,
            AccountId = Guid.Empty,
            Amount = bucketMovement.Amount,
            Memo = "Bucket Movement",
            Payee = string.Empty,
            TransactionDate = bucketMovement.MovementDate,
        };

        // Simulate Account
        SelectedAccount = new Account
        {
            AccountId = Guid.Empty,
            IsActive = 1,
            Name = string.Empty
        };
    }

    /// <summary>
    /// Initialize and return a new ViewModel based on an existing <see cref="BankTransaction"/> object including
    /// assigned Buckets
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="yearMonthViewModel">YearMonth ViewModel instance</param>
    /// <param name="transaction">Transaction instance</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<TransactionViewModelItem> CreateAsync(DbContextOptions<DatabaseContext> dbOptions, 
        YearMonthSelectorViewModel yearMonthViewModel, BankTransaction transaction)
    {
        return await Task.Run(() => new TransactionViewModelItem(dbOptions, yearMonthViewModel, transaction));
    }

    /// <summary>
    /// Initialize and return a new ViewModel based on an existing <see cref="BankTransaction"/> object without
    /// assigned Buckets
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="yearMonthViewModel">YearMonth ViewModel instance</param>
    /// <param name="transaction">Transaction instance</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<TransactionViewModelItem> CreateWithoutBucketsAsync(DbContextOptions<DatabaseContext> dbOptions, 
        YearMonthSelectorViewModel yearMonthViewModel, BankTransaction transaction)
    {
        return await Task.Run(() => new TransactionViewModelItem(dbOptions, yearMonthViewModel, transaction, false));
    }

    /// <summary>
    /// Create and return a new ViewModel and transform passed <see cref="BucketMovement"/> into a <see cref="BankTransaction"/>
    /// </summary>
    /// <param name="bucketMovement">BucketMovement which will be transformed</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<TransactionViewModelItem> CreateFromBucketMovementAsync(BucketMovement bucketMovement)
    {
        return await Task.Run(() => new TransactionViewModelItem(bucketMovement));
    }

    /// <summary>
    /// Adds a bucket item.
    /// </summary>
    /// <param name="amount">Amount that will be assigned to the Bucket</param>
    /// <param name="newBucketItem">Optional: New bucket to be added. If omitted a new, empty bucket will be added.</param>
    /// <remarks>Will add an empty bucket item if a bucket item is not provided.</remarks>
    public void AddBucketItem(decimal amount, PartialBucketViewModelItem newBucketItem = null)
    {
        newBucketItem ??=
            new PartialBucketViewModelItem(_dbOptions, _yearMonthViewModel.CurrentMonth, new Bucket(), amount);
        newBucketItem.AmountChanged += CheckBucketAssignments;
        newBucketItem.DeleteAssignmentRequest += DeleteRequestedBucketAssignment;
        Buckets.Add(newBucketItem);
    }
    
    /// <summary>
    /// Event that checks amount for all assigned Buckets and creates or removes an "empty item"
    /// </summary>
    /// <param name="sender">Object that has triggered the event</param>
    /// <param name="changedArgs">Event Arguments about changed amount</param>
    private void CheckBucketAssignments(object sender, AmountChangedArgs changedArgs)
    {
        // Check if this current event was triggered while updating the amount for the "emptyItem"
        // Prevents Deadlock and StackOverflowException 
        if (changedArgs.Source.SelectedBucket == null ||
            changedArgs.Source.SelectedBucket.BucketId == Guid.Empty)
        {
            return;
        }

        // Calculate total amount assigned to any Bucket
        decimal assignedAmount = 0;
        foreach (var bucket in Buckets)
        {
            // ignore "emptyItem" where existing Bucket is not yet assigned
            // this is the one where the amount has to be updated
            if (bucket.SelectedBucket != null && bucket.SelectedBucket.BucketId != Guid.Empty)
            {
                assignedAmount += bucket.Amount;
            }
        }

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
            if (Buckets.Last().SelectedBucket != null && Buckets.Last().SelectedBucket.BucketId != Guid.Empty)
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
        else if (Buckets.Last().SelectedBucket == null || 
                 Buckets.Last().SelectedBucket.BucketId == Guid.Empty)
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
    private void DeleteRequestedBucketAssignment(object sender, DeleteAssignmentRequestArgs args)
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
        var result = PerformConsistencyCheck(out var skipBucketAssignment);
        if (!result.IsSuccessful) return result;

        using var dbContext = new DatabaseContext(_dbOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var transactionId = Transaction.TransactionId;
            Transaction.AccountId = SelectedAccount.AccountId;

            if (transactionId != Guid.Empty)
            {
                // Update BankTransaction in DB
                dbContext.UpdateBankTransaction(Transaction);

                // Delete all previous bucket assignments for transaction
                var budgetedTransactions = dbContext.BudgetedTransaction
                    .Where(i => i.TransactionId == transactionId);
                dbContext.DeleteBudgetedTransactions(budgetedTransactions);
            }
            else
            {
                // Create BankTransaction in DB
                if (dbContext.CreateBankTransaction(Transaction) == 0)
                    throw new Exception("Transaction could not be created in database.");
            }

            if (!skipBucketAssignment)
            {
                // Create new bucket assignments
                foreach (var bucket in Buckets)
                {
                    var newBudgetedTransaction = new BudgetedTransaction
                    {
                        TransactionId = Transaction.TransactionId,
                        BucketId = bucket.SelectedBucket.BucketId,
                        Amount = bucket.Amount
                    };
                    // Execute DB Update
                    dbContext.CreateBudgetedTransaction(newBudgetedTransaction);
                }
            }

            transaction.Commit();
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            transaction.Rollback();
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
        if (Transaction == null) return new ViewModelOperationResult(false, "Errors in Transaction object.");
        if (SelectedAccount == null || SelectedAccount.AccountId == Guid.Empty) return new ViewModelOperationResult(false, "No Bank account selected.");
        if (Buckets.Count == 0) return new ViewModelOperationResult(false, "No Bucket assigned to this Transaction.");
        
        foreach (var assignedBucket in Buckets)
        {
            if (assignedBucket.SelectedBucket == null)
                return new ViewModelOperationResult(false, "Pending Bucket assignment for this Transaction.");

            if (assignedBucket.SelectedBucket.BucketId == Guid.Empty)
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
        using var dbContext = new DatabaseContext(_dbOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            // Delete BankTransaction in DB
            dbContext.DeleteBankTransaction(Transaction);

            // Delete all previous bucket assignments for transaction
            var budgetedTransactions = dbContext.BudgetedTransaction
                .Where(i => i.TransactionId == Transaction.TransactionId);
            dbContext.DeleteBudgetedTransactions(budgetedTransactions);

            transaction.Commit();
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }

    public void StartModification()
    {
        _oldTransactionViewModelItem = new TransactionViewModelItem(_dbOptions, _yearMonthViewModel, Transaction);
        InModification = true;
    }

    public void CancelModification()
    {
        Transaction = _oldTransactionViewModelItem.Transaction;
        SelectedAccount = _oldTransactionViewModelItem.SelectedAccount;
        Buckets = _oldTransactionViewModelItem.Buckets;
        InModification = false;
        _oldTransactionViewModelItem = null;
    }

    public ViewModelOperationResult CreateItem()
    {
        Transaction.TransactionId = Guid.Empty; // Triggers CREATE during CreateOrUpdateTransaction()
        return CreateOrUpdateTransaction();
    }

    public ViewModelOperationResult UpdateItem()
    {
        if (Transaction.TransactionId == Guid.Empty) 
            return new ViewModelOperationResult(false, "Transaction needs to be created first in database");

        var result = CreateOrUpdateTransaction();
        if (!result.IsSuccessful)
        {
            return new ViewModelOperationResult(false, result.Message, true);
        }
        _oldTransactionViewModelItem = null;
        InModification = false;

        return new ViewModelOperationResult(true, false);
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
        Buckets.Add(new PartialBucketViewModelItem(_dbOptions, _yearMonthViewModel.CurrentMonth, proposal, Transaction.Amount));
    }

    private Bucket CheckMappingRules()
    {
        var targetBucketId = Guid.Empty;
        using var dbContext = new DatabaseContext(_dbOptions);
        foreach (var ruleSet in dbContext.BucketRuleSet.OrderBy(i => i.Priority))
        {
            using var mappingRuleDbContext = new DatabaseContext(_dbOptions);
            if (!mappingRuleDbContext.MappingRule
                    .Where(i => i.BucketRuleSetId == ruleSet.BucketRuleSetId)
                    .All(DoesRuleApply)) continue;
            targetBucketId = ruleSet.TargetBucketId;
            break;
        }

        return targetBucketId != Guid.Empty ? dbContext.Bucket.First(i => i.BucketId == targetBucketId) : null;

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
                1 => Transaction.AccountId.ToString(),
                2 => Transaction.Payee,
                3 => Transaction.Memo,
                4 => Transaction.Amount.ToString(CultureInfo.CurrentCulture),
                _ => string.Empty
            }).ToLower();
        }
    }
}
