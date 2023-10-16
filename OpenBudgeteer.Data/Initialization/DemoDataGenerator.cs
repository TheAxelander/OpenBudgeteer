using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Contracts.Models;

namespace OpenBudgeteer.Data.Initialization;

public class DemoDataGenerator
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    public DemoDataGenerator(DbContextOptions<DatabaseContext> dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    public void GenerateDemoData()
    {
        if (!IsDatabaseEmpty()) return;
        
        using var dbContext = new DatabaseContext(_dbContextOptions);
        
        // Create Accounts
        var accountChecking = new Account()
        {
            AccountId = Guid.Empty,
            Name = "Checking account",
            IsActive = 1
        };
        var accountSavings = new Account()
        {
            AccountId = Guid.Empty,
            Name = "Savings account",
            IsActive = 1
        };
        dbContext.Account.AddRange(new []{ accountChecking, accountSavings });
        dbContext.SaveChanges();
        
        // Create Buckets Groups
        var bucketGroupRegularExpenses = new BucketGroup()
        {
            BucketGroupId = Guid.Empty,
            Name = "Regular Expenses",
            Position = 1
        };
        var bucketGroupSavings = new BucketGroup()
        {
            BucketGroupId = Guid.Empty,
            Name = "Long Term Savings",
            Position = 2
        };
        dbContext.BucketGroup.AddRange(new []{ bucketGroupRegularExpenses, bucketGroupSavings });
        dbContext.SaveChanges();
        
        // Create Buckets
        var firstOfThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var firstOfPreviousMonth = firstOfThisMonth.AddMonths(-1);
        var bucketRent = new Bucket()
        {
            BucketId = Guid.Empty,
            BucketGroupId = bucketGroupRegularExpenses.BucketGroupId,
            Name = "Rent",
            ValidFrom = firstOfPreviousMonth,
            ColorCode = "IndianRed"
        };
        var bucketHealthInsurance = new Bucket()
        {
            BucketId = Guid.Empty,
            BucketGroupId = bucketGroupRegularExpenses.BucketGroupId,
            Name = "Health Insurance",
            ValidFrom = firstOfPreviousMonth,
            ColorCode = "IndianRed"
        };
        var bucketGroceries = new Bucket()
        {
            BucketId = Guid.Empty,
            BucketGroupId = bucketGroupRegularExpenses.BucketGroupId,
            Name = "Groceries",
            ValidFrom = firstOfPreviousMonth,
            ColorCode = "DarkOliveGreen"
        };
        var bucketCarFuel = new Bucket()
        {
            BucketId = Guid.Empty,
            BucketGroupId = bucketGroupRegularExpenses.BucketGroupId,
            Name = "Car Fuel",
            ValidFrom = firstOfPreviousMonth,
            ColorCode = "DarkOliveGreen"
        };
        var bucketVacationTrip = new Bucket()
        {
            BucketId = Guid.Empty,
            BucketGroupId = bucketGroupSavings.BucketGroupId,
            Name = "Vacation Trip",
            ValidFrom = firstOfThisMonth,
            ColorCode = "Goldenrod"
        };
        var bucketReserves = new Bucket()
        {
            BucketId = Guid.Empty,
            BucketGroupId = bucketGroupSavings.BucketGroupId,
            Name = "Reserves",
            ValidFrom = firstOfPreviousMonth,
            ColorCode = "Orange"
        };
        dbContext.Bucket.AddRange(new []
        {
            bucketRent, bucketHealthInsurance, bucketGroceries, bucketCarFuel,
            bucketVacationTrip, bucketReserves
        });
        dbContext.SaveChanges();
        
        // Create Bucket Versions
        var bucketVersionRent = new BucketVersion()
        {
            BucketVersionId = Guid.Empty,
            BucketId = bucketRent.BucketId,
            BucketType = 2,
            BucketTypeXParam = 1,
            BucketTypeYParam = 400,
            BucketTypeZParam = DateTime.MinValue,
            ValidFrom = bucketRent.ValidFrom,
            Version = 1
        };
        var bucketVersionHealthInsurance = new BucketVersion()
        {
            BucketVersionId = Guid.Empty,
            BucketId = bucketHealthInsurance.BucketId,
            BucketType = 3,
            BucketTypeXParam = 3,
            BucketTypeYParam = 150,
            BucketTypeZParam = bucketHealthInsurance.ValidFrom.AddMonths(2),
            ValidFrom = bucketHealthInsurance.ValidFrom,
            Version = 1
        };
        var bucketVersionGroceries = new BucketVersion()
        {
            BucketVersionId = Guid.Empty,
            BucketId = bucketGroceries.BucketId,
            BucketType = 2,
            BucketTypeXParam = 1,
            BucketTypeYParam = 300,
            BucketTypeZParam = DateTime.MinValue,
            ValidFrom = bucketGroceries.ValidFrom,
            Version = 1
        };
        var bucketVersionCarFuel = new BucketVersion()
        {
            BucketVersionId = Guid.Empty,
            BucketId = bucketCarFuel.BucketId,
            BucketType = 2,
            BucketTypeXParam = 1,
            BucketTypeYParam = 100,
            BucketTypeZParam = DateTime.MinValue,
            ValidFrom = bucketCarFuel.ValidFrom,
            Version = 1
        };
        var bucketVersionVacationTrip = new BucketVersion()
        {
            BucketVersionId = Guid.Empty,
            BucketId = bucketVacationTrip.BucketId,
            BucketType = 4,
            BucketTypeXParam = 0,
            BucketTypeYParam = 600,
            BucketTypeZParam = bucketVacationTrip.ValidFrom.AddMonths(5),
            ValidFrom = bucketVacationTrip.ValidFrom,
            Version = 1
        };
        var bucketVersionReserves = new BucketVersion()
        {
            BucketVersionId = Guid.Empty,
            BucketId = bucketReserves.BucketId,
            BucketType = 1,
            BucketTypeXParam = 0,
            BucketTypeYParam = 0,
            BucketTypeZParam = DateTime.MinValue,
            ValidFrom = bucketReserves.ValidFrom,
            Version = 1
        };
        dbContext.BucketVersion.AddRange(new []
        {
            bucketVersionRent, bucketVersionHealthInsurance, bucketVersionGroceries, bucketVersionCarFuel,
            bucketVersionVacationTrip, bucketVersionReserves
        });
        dbContext.SaveChanges();
        
        // Create Bank Transactions
        var transactionOpeningCheckingAccount = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1),
            AccountId = accountChecking.AccountId,
            Memo = "Opening Transaction",
            Amount = new decimal(1500.35)
        };
        var transactionSalary = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 28).AddMonths(-1),
            AccountId = accountChecking.AccountId,
            Memo = "Salary",
            Amount = new decimal(2300.57)
        };
        var transactionRent1 = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10).AddMonths(-1),
            AccountId = accountChecking.AccountId,
            Memo = "Rent",
            Amount = -400
        };
        var transactionRent2 = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10),
            AccountId = accountChecking.AccountId,
            Memo = "Rent",
            Amount = -400
        };
        var transactionGroceries1 = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 4).AddMonths(-1),
            AccountId = accountChecking.AccountId,
            Memo = "Supermarket",
            Amount = new decimal(-23.87)
        };
        var transactionGroceries2 = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 11).AddMonths(-1),
            AccountId = accountChecking.AccountId,
            Memo = "Supermarket",
            Amount = new decimal(-40.34)
        };
        var transactionGroceries3 = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 2),
            AccountId = accountChecking.AccountId,
            Memo = "Supermarket",
            Amount = new decimal(-10.50)
        };
        var transactionSplit = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 25).AddMonths(-1),
            AccountId = accountChecking.AccountId,
            Memo = "Weekend shopping",
            Amount = new decimal(-170.51)
        };
        var transactionTransfer1 = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 29).AddMonths(-1),
            AccountId = accountChecking.AccountId,
            Memo = "Bank Transfer",
            Amount = -1500
        };
        var transactionTransfer2 = new BankTransaction()
        {
            TransactionId = Guid.Empty,
            TransactionDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 29).AddMonths(-1),
            AccountId = accountSavings.AccountId,
            Memo = "Bank Transfer",
            Amount = 1500
        };
        dbContext.BankTransaction.AddRange(new []
        {
            transactionOpeningCheckingAccount, transactionSalary,
            transactionRent1, transactionRent2,
            transactionGroceries1, transactionGroceries2, transactionGroceries3,
            transactionSplit, transactionTransfer1, transactionTransfer2
        });
        dbContext.SaveChanges();
        
        // Create BucketMovements
        var movementRent1 = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = firstOfPreviousMonth,
            BucketId = bucketRent.BucketId,
            Amount = 400
        };
        var movementRent2 = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = firstOfThisMonth,
            BucketId = bucketRent.BucketId,
            Amount = 400
        };
        var movementInsurance1 = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = firstOfPreviousMonth,
            BucketId = bucketHealthInsurance.BucketId,
            Amount = 50
        };
        var movementInsurance2 = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = firstOfThisMonth,
            BucketId = bucketHealthInsurance.BucketId,
            Amount = 50
        };
        var movementGroceries1 = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = firstOfPreviousMonth,
            BucketId = bucketGroceries.BucketId,
            Amount = 300
        };
        var movementGroceries2 = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = firstOfThisMonth,
            BucketId = bucketGroceries.BucketId,
            Amount = 300
        };
        var movementCarFuel1 = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = firstOfPreviousMonth,
            BucketId = bucketCarFuel.BucketId,
            Amount = 100
        };
        var movementCarFuel2 = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = firstOfThisMonth,
            BucketId = bucketCarFuel.BucketId,
            Amount = 100
        };
        var movementVacationTrip = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = firstOfThisMonth,
            BucketId = bucketVacationTrip.BucketId,
            Amount = 100
        };
        var movementReserves = new BucketMovement()
        {
            BucketMovementId = Guid.Empty,
            MovementDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 28).AddMonths(-1),
            BucketId = bucketReserves.BucketId,
            Amount = 1000
        };
        dbContext.BucketMovement.AddRange(new []
        {
            movementRent1, movementInsurance1, movementGroceries1, movementCarFuel1, movementVacationTrip, movementReserves,
            movementRent2, movementInsurance2, movementGroceries2, movementCarFuel2
        });
        dbContext.SaveChanges();
        
        // Create BudgetedTransactions

        var budgetedTransactionOpeningCheckingAccount = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionOpeningCheckingAccount.TransactionId,
            BucketId = new Guid("00000000-0000-0000-0000-000000000001"),
            Amount = transactionOpeningCheckingAccount.Amount
        };
        var budgetedTransactionSalary = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionSalary.TransactionId,
            BucketId = new Guid("00000000-0000-0000-0000-000000000001"),
            Amount = transactionSalary.Amount
        };
        var budgetedTransactionRent1 = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionRent1.TransactionId,
            BucketId = bucketRent.BucketId,
            Amount = transactionRent1.Amount
        };
        var budgetedTransactionRent2 = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionRent2.TransactionId,
            BucketId = bucketRent.BucketId,
            Amount = transactionRent2.Amount
        };
        var budgetedTransactionGroceries1 = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionGroceries1.TransactionId,
            BucketId = bucketGroceries.BucketId,
            Amount = transactionGroceries1.Amount
        };
        var budgetedTransactionGroceries2 = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionGroceries2.TransactionId,
            BucketId = bucketGroceries.BucketId,
            Amount = transactionGroceries2.Amount
        };
        var budgetedTransactionGroceries3 = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionGroceries3.TransactionId,
            BucketId = bucketGroceries.BucketId,
            Amount = transactionGroceries3.Amount
        };
        var budgetedTransactionSplit1 = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionSplit.TransactionId,
            BucketId = bucketCarFuel.BucketId,
            Amount = new decimal(-90.86) 
        };
        var budgetedTransactionSplit2 = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionSplit.TransactionId,
            BucketId = bucketGroceries.BucketId,
            Amount = transactionSplit.Amount - budgetedTransactionSplit1.Amount
        };
        var budgetedTransactionTransfer1 = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionTransfer1.TransactionId,
            BucketId = new Guid("00000000-0000-0000-0000-000000000002"),
            Amount = transactionTransfer1.Amount
        };
        var budgetedTransactionTransfer2 = new BudgetedTransaction()
        {
            BudgetedTransactionId = Guid.Empty,
            TransactionId = transactionTransfer2.TransactionId,
            BucketId = new Guid("00000000-0000-0000-0000-000000000002"),
            Amount = transactionTransfer2.Amount
        };
        dbContext.BudgetedTransaction.AddRange(new []
        {
            budgetedTransactionOpeningCheckingAccount, budgetedTransactionSalary, budgetedTransactionRent1,
            budgetedTransactionRent2, budgetedTransactionGroceries1, budgetedTransactionGroceries2,
            budgetedTransactionGroceries3, budgetedTransactionSplit1, budgetedTransactionSplit2,
            budgetedTransactionTransfer1, budgetedTransactionTransfer2
        });
        dbContext.SaveChanges();
    }
    
    private bool IsDatabaseEmpty()
    {
        using var dbContext = new DatabaseContext(_dbContextOptions);
        if (dbContext.Account.Any()) return false;
        if (dbContext.Bucket.Any(i => i.BucketGroupId != new Guid("00000000-0000-0000-0000-000000000001"))) return false;
        if (dbContext.BankTransaction.Any()) return false;
        if (dbContext.BucketMovement.Any()) return false;
        if (dbContext.BudgetedTransaction.Any()) return false;
            
        return true;
    }
}