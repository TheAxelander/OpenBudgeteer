using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels
{
    /// <summary>
    /// ViewModel for each Transaction Item
    /// </summary>
    public class TransactionViewModelItem : ViewModelBase
    {
        private BankTransaction _transaction;
        public BankTransaction Transaction
        {
            get => _transaction;
            internal set => Set(ref _transaction, value);
        }

        private Account _selectedAccount;
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set => Set(ref _selectedAccount, value);
        }

        private bool _inModification;
        public bool InModification
        {
            get => _inModification;
            set => Set(ref _inModification, value);
        }

        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set => Set(ref _isHovered, value);
        }

        private ObservableCollection<PartialBucketViewModelItem> _buckets;
        public ObservableCollection<PartialBucketViewModelItem> Buckets
        {
            get => _buckets;
            set => Set(ref _buckets, value);
        }

        private ObservableCollection<Account> _availableAccounts;
        public ObservableCollection<Account> AvailableAccounts
        {
            get => _availableAccounts;
            set => Set(ref _availableAccounts, value);
        }

        public event ViewModelReloadRequiredHandler ViewModelReloadRequired;
        public delegate void ViewModelReloadRequiredHandler(ViewModelBase sender);

        private readonly DbContextOptions<DatabaseContext> _dbOptions;
        private readonly YearMonthSelectorViewModel _yearMonthViewModel;
        private TransactionViewModelItem _oldTransactionViewModelItem;

        public TransactionViewModelItem()
        {
            Transaction = new BankTransaction();
            Buckets = new ObservableCollection<PartialBucketViewModelItem>();
            AvailableAccounts = new ObservableCollection<Account>();
        }

        public TransactionViewModelItem(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel) : this()
        {
            _dbOptions = dbOptions;
            _yearMonthViewModel = yearMonthViewModel;

            // Set initial TransactionDate in case of "Create new Transaction"
            Transaction.TransactionDate = _yearMonthViewModel.CurrentMonth;
            // Get all available Accounts for ComboBox selections
            // Add empty Account for empty pre-selection
            AvailableAccounts.Add(new Account
            {
                AccountId = 0,
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
        }

        public TransactionViewModelItem(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel, BankTransaction transaction, bool withBuckets = true) : this(dbOptions, yearMonthViewModel)
        {
            if (withBuckets)
            {
                // Get all assigned Buckets for this transaction
                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    var assignedBuckets = dbContext.BudgetedTransaction
                    .Where(i => i.TransactionId == transaction.TransactionId);

                    if (assignedBuckets.Any())
                    {
                        // Create a PartialBucketViewModelItem for each assignment
                        foreach (var assignedBucket in assignedBuckets)
                        {
                            using (var bucketDbContext = new DatabaseContext(_dbOptions))
                            {
                                var newItem = new PartialBucketViewModelItem(_dbOptions,
                                    _yearMonthViewModel,
                                    bucketDbContext.Bucket.FirstOrDefault(i => i.BucketId == assignedBucket.BucketId),
                                    assignedBucket.Amount);
                                newItem.SelectedBucketOutput =
                                    newItem.Amount != transaction.Amount ? $"{newItem.SelectedBucket.Name} ({newItem.Amount})" : newItem.SelectedBucket.Name;
                                Buckets.Add(newItem);
                            }
                        }
                    }
                    else
                    {
                        // Most likely an imported Transaction where Bucket assignment still needs to be done
                        var newItem = new PartialBucketViewModelItem(_dbOptions, _yearMonthViewModel, new Bucket() { BucketId = 0 }, transaction.Amount);
                        Buckets.Add(newItem);
                    }
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
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var account = dbContext.Account.First(i => i.AccountId == transaction.AccountId);
                if (account != null && account.IsActive == 0)
                {
                    account.Name += " (Inactive)";
                    AvailableAccounts.Add(account);
                }
                SelectedAccount = account;
            }
            // Pre-select empty Account if no Account was found (for new BankTransaction())
            SelectedAccount ??= AvailableAccounts.First();
            
        }

        public TransactionViewModelItem(BucketMovement bucketMovement) : this()
        {
            // Simulate a BankTransaction based on BucketMovement
            Transaction = new BankTransaction
            {
                TransactionId = 0,
                AccountId = 0,
                Amount = bucketMovement.Amount,
                Memo = "Bucket Movement",
                Payee = string.Empty,
                TransactionDate = bucketMovement.MovementDate,
            };

            // Simulate Account
            SelectedAccount = new Account
            {
                AccountId = 0,
                IsActive = 1,
                Name = string.Empty
            };
        }

        public static async Task<TransactionViewModelItem> CreateAsync(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel, BankTransaction transaction)
        {
            return await Task.Run(() => new TransactionViewModelItem(dbOptions, yearMonthViewModel, transaction));
        }

        public static async Task<TransactionViewModelItem> CreateWithoutBucketsAsync(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel, BankTransaction transaction)
        {
            return await Task.Run(() => new TransactionViewModelItem(dbOptions, yearMonthViewModel, transaction, false));
        }

        public static async Task<TransactionViewModelItem> CreateFromBucketMovementAsync(BucketMovement bucketMovement)
        {
            return await Task.Run(() => new TransactionViewModelItem(bucketMovement));
        }

        private void CheckBucketAssignments(object sender, AmountChangedArgs changedArgs)
        {
            // Check if this current event was triggered while updating the amount for the "emptyItem"
            // Prevents Deadlock and StackOverflowException 
            if (changedArgs.Source.SelectedBucket == null ||
                changedArgs.Source.SelectedBucket.BucketId == 0)
            {
                return;
            }

            // Calculate total amount assigned to any Bucket
            decimal assignedAmount = 0;
            foreach (var bucket in Buckets)
            {
                // ignore "emptyItem" where existing Bucket is not yet assigned
                // this is the one where the amount has to be updated

                if (bucket.SelectedBucket != null && bucket.SelectedBucket.BucketId != 0)
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
                if (Buckets.Last().SelectedBucket != null && Buckets.Last().SelectedBucket.BucketId != 0)
                {
                    // All items have a valid Bucket assignment, create a new "empty item"
                    AddEmptyBucketItem(Transaction.Amount - assignedAmount);
                }
                else
                {
                    // "emptyItem" exists, update remaining amount to be assigned
                    Buckets.Last().Amount = Transaction.Amount - assignedAmount;
                }
            }
            else if (Buckets.Last().SelectedBucket == null || 
                     Buckets.Last().SelectedBucket.BucketId == 0)
            {
                // Remove unnecessary "empty item" as amount is already assigned properly
                Buckets.Remove(Buckets.Last());
            }
        }

        private void DeleteRequestedBucketAssignment(object sender, DeleteAssignmentRequestArgs args)
        {
            // Prevent deletion all last remaining BucketAssignment
            if (Buckets.Count > 1)
            {
                Buckets.Remove(args.Source);
            }
        }

        private void AddEmptyBucketItem(decimal amount)
        {
            // All items have a valid Bucket assignment, create a new "empty item"
            var emptyItem = new PartialBucketViewModelItem(_dbOptions, _yearMonthViewModel, new Bucket(), amount);
            emptyItem.AmountChanged += CheckBucketAssignments;
            emptyItem.DeleteAssignmentRequest += DeleteRequestedBucketAssignment;
            Buckets.Add(emptyItem);
        }

        private Tuple<bool, string> CreateUpdateTransaction()
        {
            // Consistency and Validity Checks
            if (Transaction == null) return new Tuple<bool, string>(false, "Errors in Transaction object.");
            if (SelectedAccount == null || SelectedAccount.AccountId == 0) return new Tuple<bool, string>(false, "No Bank account selected.");
            //if (string.IsNullOrEmpty(Transaction.TransactionDate))
            //    return new Tuple<bool, string>(false, "Transaction date is missing.");
            //if (string.IsNullOrEmpty(transaction.Transaction.Memo))
            //    return new Tuple<bool, string>(false, "Transaction Memo is missing.");
            if (Buckets.Count == 0)
                return new Tuple<bool, string>(false, "No Bucket assigned to this Transaction.");

            decimal assignedAmount = 0;
            var skipBucketAssignment = false;
            foreach (var assignedBucket in Buckets)
            {
                if (assignedBucket.SelectedBucket == null)
                {
                    return new Tuple<bool, string>(false, "Pending Bucket assignment for this Transaction.");
                }

                if (assignedBucket.SelectedBucket.BucketId == 0)
                {
                    if (assignedBucket.SelectedBucket.Name == "No Selection")
                    {
                        // Imported Transaction where Bucket assignment is pending
                        // Allow Transaction Update but Skip DB Updates for Bucket assignment
                        skipBucketAssignment = true;
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, "Pending Bucket assignment for this Transaction.");
                    }
                }
                assignedAmount += assignedBucket.Amount;
            }

            if (assignedAmount != Transaction.Amount)
                return new Tuple<bool, string>(false,
                    "Amount between Bucket assignment and Transaction not consistent.");

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var transactionId = Transaction.TransactionId;
                        Transaction.AccountId = SelectedAccount.AccountId;

                        if (transactionId != 0)
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
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return new Tuple<bool, string>(false, $"Errors during database update: {e.Message}");
                    }
                }
            }            

            return new Tuple<bool, string>(true, string.Empty);
        }

        private Tuple<bool, string> DeleteTransaction()
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Delete BankTransaction in DB
                        dbContext.DeleteBankTransaction(Transaction);

                        // Delete all previous bucket assignments for transaction
                        var budgetedTransactions = dbContext.BudgetedTransaction

                            .Where(i => i.TransactionId == Transaction.TransactionId);
                        dbContext.DeleteBudgetedTransactions(budgetedTransactions);
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return new Tuple<bool, string>(false, $"Errors during database update: {e.Message}");
                    }
                }
            }            
            return new Tuple<bool, string>(true, string.Empty);
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

        public Tuple<bool, string> CreateItem()
        {
            Transaction.TransactionId = 0; // Triggers CREATE during CreateUpdateTransaction()
            return CreateUpdateTransaction();
        }

        public Tuple<bool, string> UpdateItem()
        {
            if (Transaction.TransactionId < 1)
                return new Tuple<bool, string>(false, "Transaction needs to be created first in database");

            var (result, message) = CreateUpdateTransaction();
            if (!result)
            {
                // Trigger page reload as DB Update was not successfully
                ViewModelReloadRequired?.Invoke(this);

                return new Tuple<bool, string>(false, message);
            }
            _oldTransactionViewModelItem = null;
            InModification = false;

            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool, string> DeleteItem()
        {
            var (result, message) = DeleteTransaction();
            if (!result)
            {
                return new Tuple<bool, string>(false, message);
            }
            ViewModelReloadRequired?.Invoke(this);
           
            return new Tuple<bool, string>(true, string.Empty);
        }

        public void ProposeBucket()
        {
            var proposal = CheckMappingRules();
            if (proposal == null) return;
            Buckets.Clear();
            Buckets.Add(new PartialBucketViewModelItem(_dbOptions, _yearMonthViewModel, proposal, Transaction.Amount));
        }

        private Bucket CheckMappingRules()
        {
            var targetBucketId = 0;
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                foreach (var ruleSet in dbContext.BucketRuleSet.OrderBy(i => i.Priority))
                {
                    using (var mappingRuleDbContext = new DatabaseContext(_dbOptions))
                    {
                        if (mappingRuleDbContext.MappingRule
                            .Where(i => i.BucketRuleSetId == ruleSet.BucketRuleSetId)
                            .All(DoesRuleApply))
                        {
                            targetBucketId = ruleSet.TargetBucketId;
                            break;
                        }
                    }
                }

                return targetBucketId != 0 ? dbContext.Bucket.First(i => i.BucketId == targetBucketId) : null;
            }

            bool DoesRuleApply(MappingRule mappingRule)
            {
                var cleanedComparisionValue = mappingRule.ComparisionValue.ToLower();
                switch (mappingRule.ComparisionType)
                {
                    case 1:
                        return cleanedComparisionValue == GetFieldValue(mappingRule.ComparisionField);
                    case 2:
                        return cleanedComparisionValue != GetFieldValue(mappingRule.ComparisionField);
                    case 3:
                        return GetFieldValue(mappingRule.ComparisionField).Contains(cleanedComparisionValue);
                    case 4:
                        return !GetFieldValue(mappingRule.ComparisionField).Contains(cleanedComparisionValue);
                }

                return false;
            }

            string GetFieldValue(int comparisionField)
            {
                string result;
                switch (comparisionField)
                {
                    case 1:
                        result = Transaction.AccountId.ToString();
                        break;
                    case 2:
                        result = Transaction.Payee;
                        break;
                    case 3:
                        result = Transaction.Memo;
                        break;
                    case 4:
                        result = Transaction.Amount.ToString();
                        break;
                    default:
                        result = null;
                        break;
                }

                return result == null ? string.Empty : result.ToLower();
            }
        }
    }
}
