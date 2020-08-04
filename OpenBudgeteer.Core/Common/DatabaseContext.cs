using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Models;

namespace OpenBudgeteer.Core.Common
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Account> Account { get; set; }
        public DbSet<BankTransaction> BankTransaction { get; set; }
        public DbSet<Bucket> Bucket { get; set; }
        public DbSet<BucketGroup> BucketGroup { get; set; }
        public DbSet<BucketMovement> BucketMovement { get; set; }
        public DbSet<BucketVersion> BucketVersion { get; set; }
        public DbSet<BudgetedTransaction> BudgetedTransaction { get; set; }
        public DbSet<ImportProfile> ImportProfile { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        #region Create

        public int CreateAccount(Account account)
            => CreateAccounts(new List<Account>() { account });

        public int CreateAccounts(IEnumerable<Account> accounts)
        {
            Account.AddRange(accounts);
            return SaveChanges();
        }

        public int CreateBankTransaction(BankTransaction bankTransaction)
            => CreateBankTransactions(new List<BankTransaction>() { bankTransaction });

        public int CreateBankTransactions(IEnumerable<BankTransaction> bankTransactions)
        {
            BankTransaction.AddRange(bankTransactions);
            return SaveChanges();
        }

        public int CreateBucket(Bucket bucket)
            => CreateBuckets(new List<Bucket>() { bucket });

        public int CreateBuckets(IEnumerable<Bucket> buckets)
        {
            Bucket.AddRange(buckets);
            return SaveChanges();
        }

        public int CreateBucketGroup(BucketGroup bucketGroup)
            => CreateBucketGroups(new List<BucketGroup>() { bucketGroup });

        public int CreateBucketGroups(IEnumerable<BucketGroup> bucketGroups)
        {
            BucketGroup.AddRange(bucketGroups);
            return SaveChanges();
        }

        public int CreateBucketMovement(BucketMovement bucketMovement)
            => CreateBucketMovements(new List<BucketMovement>() { bucketMovement });

        public int CreateBucketMovements(IEnumerable<BucketMovement> bucketMovements)
        {
            BucketMovement.AddRange(bucketMovements);
            return SaveChanges();
        }

        public int CreateBucketVersion(BucketVersion bucketVersion)
            => CreateBucketVersions(new List<BucketVersion>() { bucketVersion });

        public int CreateBucketVersions(IEnumerable<BucketVersion> bucketVersions)
        {
            BucketVersion.AddRange(bucketVersions);
            return SaveChanges();
        }

        public int CreateBudgetedTransaction(BudgetedTransaction budgetedTransaction)
            => CreateBudgetedTransactions(new List<BudgetedTransaction>() { budgetedTransaction });

        public int CreateBudgetedTransactions(IEnumerable<BudgetedTransaction> budgetedTransactions)
        {
            BudgetedTransaction.AddRange(budgetedTransactions);
            return SaveChanges();
        }

        public int CreateImportProfile(ImportProfile importProfile)
            => CreateImportProfiles(new List<ImportProfile>() { importProfile });

        public int CreateImportProfiles(IEnumerable<ImportProfile> importProfiles)
        {
            ImportProfile.AddRange(importProfiles);
            return SaveChanges();
        }

        #endregion

        #region Update

        public int UpdateAccount(Account account)
            => UpdateAccounts(new List<Account>() { account });

        public int UpdateAccounts(IEnumerable<Account> accounts)
        {
            foreach (var account in accounts)
            {
                var dbAccount = Account.First(i => i.AccountId == account.AccountId);
                dbAccount.Name = account.Name;
                dbAccount.IsActive = account.IsActive;
            }
            return SaveChanges();
        }

        public int UpdateBankTransaction(BankTransaction bankTransaction)
            => UpdateBankTransactions(new List<BankTransaction>() { bankTransaction });

        public int UpdateBankTransactions(IEnumerable<BankTransaction> bankTransactions)
        {
            foreach (var bankTransaction in bankTransactions)
            {
                var dbBankTransaction = BankTransaction.First(i => i.TransactionId == bankTransaction.TransactionId);
                dbBankTransaction.AccountId = bankTransaction.AccountId;
                dbBankTransaction.Amount = bankTransaction.Amount;
                dbBankTransaction.Memo = bankTransaction.Memo;
                dbBankTransaction.Payee = bankTransaction.Payee;
                dbBankTransaction.TransactionDate = bankTransaction.TransactionDate;
            }
            return SaveChanges();
        }

        public int UpdateBucket(Bucket bucket)
            => UpdateBuckets(new List<Bucket>() { bucket });

        public int UpdateBuckets(IEnumerable<Bucket> buckets)
        {
            foreach (var bucket in buckets)
            {
                var dbBucket = Bucket.First(i => i.BucketId == bucket.BucketId);
                dbBucket.Name = bucket.Name;
                dbBucket.BucketGroupId = bucket.BucketGroupId;
                dbBucket.ColorCode = bucket.ColorCode;
                dbBucket.IsInactive = bucket.IsInactive;
                dbBucket.IsInactiveFrom = bucket.IsInactiveFrom;
                dbBucket.ValidFrom = bucket.ValidFrom;
            }
            return SaveChanges();
        }

        public int UpdateBucketGroup(BucketGroup bucketGroup)
            => UpdateBucketGroups(new List<BucketGroup>() { bucketGroup });

        public int UpdateBucketGroups(IEnumerable<BucketGroup> bucketGroups)
        {
            foreach (var bucketGroup in bucketGroups)
            {
                var dbBucketGroup = BucketGroup.First(i => i.BucketGroupId == bucketGroup.BucketGroupId);
                dbBucketGroup.Name = bucketGroup.Name;
                dbBucketGroup.Position = bucketGroup.Position;
            }
            return SaveChanges();
        }

        public int UpdateBucketMovement(BucketMovement bucketMovement)
            => UpdateBucketMovements(new List<BucketMovement>() { bucketMovement });

        public int UpdateBucketMovements(IEnumerable<BucketMovement> bucketMovements)
        {
            foreach (var bucketMovement in bucketMovements)
            {
                var dbBucketMovement = BucketMovement.First(i => i.BucketMovementId == bucketMovement.BucketMovementId);
                dbBucketMovement.Amount = bucketMovement.Amount;
                dbBucketMovement.BucketId = bucketMovement.BucketId;
                dbBucketMovement.MovementDate = bucketMovement.MovementDate;
            }
            return SaveChanges();
        }

        public int UpdateBucketVersion(BucketVersion bucketVersion)
            => UpdateBucketVersions(new List<BucketVersion>() { bucketVersion });

        public int UpdateBucketVersions(IEnumerable<BucketVersion> bucketVersions)
        {
            foreach (var bucketVersion in bucketVersions)
            {
                var dbBucketVersion = BucketVersion.First(i => i.BucketVersionId == bucketVersion.BucketVersionId);
                dbBucketVersion.BucketType = bucketVersion.BucketType;
                dbBucketVersion.BucketTypeXParam = bucketVersion.BucketTypeXParam;
                dbBucketVersion.BucketTypeYParam = bucketVersion.BucketTypeYParam;
                dbBucketVersion.BucketTypeZParam = bucketVersion.BucketTypeZParam;
                dbBucketVersion.Notes = bucketVersion.Notes;
                dbBucketVersion.ValidFrom = bucketVersion.ValidFrom;
            }
            return SaveChanges();
        }

        public int UpdateBudgetedTransaction(BudgetedTransaction budgetedTransaction)
            => UpdateBudgetedTransactions(new List<BudgetedTransaction>() { budgetedTransaction });

        public int UpdateBudgetedTransactions(IEnumerable<BudgetedTransaction> budgetedTransactions)
        {
            foreach (var budgetedTransaction in budgetedTransactions)
            {
                var dbBudgetedTransaction = BudgetedTransaction.First(i => i.BudgetedTransactionId == budgetedTransaction.BudgetedTransactionId);
                dbBudgetedTransaction.Amount = budgetedTransaction.Amount;
                dbBudgetedTransaction.BucketId = budgetedTransaction.BucketId;
            }
            return SaveChanges();
        }

        public int UpdateImportProfile(ImportProfile importProfile)
            => UpdateImportProfiles(new List<ImportProfile>() { importProfile });

        public int UpdateImportProfiles(IEnumerable<ImportProfile> importProfiles)
        {
            foreach (var importProfile in importProfiles)
            {
                var dbImportProfile = ImportProfile.First(i => i.ImportProfileId == importProfile.ImportProfileId);
                dbImportProfile.AccountId = importProfile.AccountId;
                dbImportProfile.ProfileName = importProfile.ProfileName;
                dbImportProfile.HeaderRow = importProfile.HeaderRow;
                dbImportProfile.Delimiter = importProfile.Delimiter;
                dbImportProfile.TextQualifier = importProfile.TextQualifier;
                dbImportProfile.DateFormat = importProfile.DateFormat;
                dbImportProfile.NumberFormat = importProfile.NumberFormat;
                dbImportProfile.AmountColumnName = importProfile.AmountColumnName;
                dbImportProfile.MemoColumnName = importProfile.MemoColumnName;
                dbImportProfile.PayeeColumnName = importProfile.PayeeColumnName;
                dbImportProfile.TransactionDateColumnName = importProfile.TransactionDateColumnName;
            }
            return SaveChanges();
        }

        #endregion

        #region Delete

        public int DeleteAccount(Account account)
            => DeleteAccounts(new List<Account>() { account });

        public int DeleteAccounts(IEnumerable<Account> accounts)
        {
            Account.RemoveRange(accounts);
            return SaveChanges();
        }

        public int DeleteBankTransaction(BankTransaction bankTransaction)
            => DeleteBankTransactions(new List<BankTransaction>() { bankTransaction });

        public int DeleteBankTransactions(IEnumerable<BankTransaction> bankTransactions)
        {
            BankTransaction.RemoveRange(bankTransactions);
            return SaveChanges();
        }

        public int DeleteBucket(Bucket bucket)
            => DeleteBuckets(new List<Bucket>() { bucket });

        public int DeleteBuckets(IEnumerable<Bucket> buckets)
        {
            Bucket.RemoveRange(buckets);
            return SaveChanges();
        }

        public int DeleteBucketGroup(BucketGroup bucketGroup)
            => DeleteBucketGroups(new List<BucketGroup>() { bucketGroup });

        public int DeleteBucketGroups(IEnumerable<BucketGroup> bucketGroups)
        {
            BucketGroup.RemoveRange(bucketGroups);
            return SaveChanges();
        }

        public int DeleteBucketMovement(BucketMovement bucketMovement)
            => DeleteBucketMovements(new List<BucketMovement>() { bucketMovement });

        public int DeleteBucketMovements(IEnumerable<BucketMovement> bucketMovements)
        {
            BucketMovement.RemoveRange(bucketMovements);
            return SaveChanges();
        }

        public int DeleteBucketVersion(BucketVersion bucketVersion)
            => DeleteBucketVersions(new List<BucketVersion>() { bucketVersion });

        public int DeleteBucketVersions(IEnumerable<BucketVersion> bucketVersions)
        {
            BucketVersion.RemoveRange(bucketVersions);
            return SaveChanges();
        }

        public int DeleteBudgetedTransaction(BudgetedTransaction budgetedTransaction)
            => DeleteBudgetedTransactions(new List<BudgetedTransaction>() { budgetedTransaction });

        public int DeleteBudgetedTransactions(IEnumerable<BudgetedTransaction> budgetedTransactions)
        {
            BudgetedTransaction.RemoveRange(budgetedTransactions);
            return SaveChanges();
        }

        public int DeleteImportProfile(ImportProfile importProfile)
            => DeleteImportProfiles(new List<ImportProfile>() { importProfile });

        public int DeleteImportProfiles(IEnumerable<ImportProfile> importProfiles)
        {
            ImportProfile.RemoveRange(importProfiles);
            return SaveChanges();
        }

        #endregion
    }
}
