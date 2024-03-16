using OpenBudgeteer.Core.Data.Contracts.Services;

namespace OpenBudgeteer.Core.Test.ViewModelTest;

public abstract class BaseTest
{
    protected readonly TestServiceManager ServiceManager;

    protected BaseTest(string dbName)
    {
        ServiceManager = TestServiceManager.CreateUsingSqlite(dbName);
        //ServiceManager = TestServiceManager.CreateUsingMySql();
        ServiceManager.CleanupDatabase();
    }

    protected void Cleanup()
    {
        ServiceManager.CleanupDatabase();
    }
}