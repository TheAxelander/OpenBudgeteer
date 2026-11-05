using System;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockServiceManager : IServiceManager
{
    public IAccountService AccountService { get; }
    public IBankTransactionService BankTransactionService { get; }
    public IBucketGroupService BucketGroupService { get; }
    public IBucketMovementService BucketMovementService { get; }
    public IBucketService BucketService { get; }
    public IBucketRuleSetService BucketRuleSetService { get; }
    public IBudgetedTransactionService BudgetedTransactionService { get; }
    public IImportProfileService ImportProfileService { get; }
    public IRecurringBankTransactionService RecurringBankTransactionService { get; }
    
    private readonly ILoggerFactory _loggerFactory;

    public MockServiceManager(MockDatabase mockDatabase)
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug);
        });
        
        AccountService = new MockAccountService(mockDatabase, new Logger<MockAccountService>(_loggerFactory));
        BankTransactionService = new MockBankTransactionService(mockDatabase, new  Logger<MockBankTransactionService>(_loggerFactory));
        BucketGroupService = new MockBucketGroupService(mockDatabase, new Logger<MockBucketGroupService>(_loggerFactory));
        BucketMovementService = new MockBucketMovementService(mockDatabase, new Logger<MockBucketMovementService>(_loggerFactory));
        BucketService = new MockBucketService(mockDatabase, new Logger<MockBucketService>(_loggerFactory));
        BucketRuleSetService = new MockBucketRuleSetService(mockDatabase, new Logger<MockBucketRuleSetService>(_loggerFactory));
        BudgetedTransactionService = new MockBudgetedTransactionService(mockDatabase, new Logger<MockBudgetedTransactionService>(_loggerFactory));
        ImportProfileService = new MockImportProfileService(mockDatabase, new Logger<MockImportProfileService>(_loggerFactory));
        RecurringBankTransactionService = new MockRecurringBankTransactionService(mockDatabase, new Logger<MockRecurringBankTransactionService>(_loggerFactory));
    }
    
    public ILogger CreateLogger(Type type)
    {
        return _loggerFactory.CreateLogger(type);
    }
}