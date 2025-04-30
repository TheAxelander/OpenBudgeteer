using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Entities;

namespace OpenBudgeteer.Core.Data.Connection;

public interface IDatabaseConnector<out T> where T : DbConnectionStringBuilder
{
    string Provider { get; }
    string Server { get; }
    int Port { get; }
    string Database { get; }
    string Username { get; }
    string Password { get; }
    string RootPassword { get; }
    
    const int MAXIMUM_ATTEMPTS_TO_CONNECT = 10;
    const int RETRY_AFTER_MILLISEC = 5000;

    public T BuildConnectionString();
    public T BuildRootConnectionString();
    public DbContextOptions<DatabaseContext> GetDbContextOptions();
    
    public bool IsDatabaseOnline(int maxAttempts = MAXIMUM_ATTEMPTS_TO_CONNECT);
    public bool IsDatabaseAccessible(bool useRoot = false);
}