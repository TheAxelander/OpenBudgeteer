using System;
using System.Data.Common;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Entities;

namespace OpenBudgeteer.Core.Data.Connection;

public abstract class BaseDatabaseConnector<T> : IDatabaseConnector<T> where T : DbConnectionStringBuilder
{
    public abstract string Provider { get; }
    public string Server { get; protected set; }
    public int Port { get; protected set; }
    public string Database { get; protected set; }
    public string Username { get; protected set; }
    public string Password { get; protected set; }
    public string RootPassword { get; protected set; }

    public BaseDatabaseConnector(IConfiguration configuration)
    {
        Server = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_SERVER) ?? string.Empty;
        Port = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PORT, 0);
        Database = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_DATABASE) ?? string.Empty;
        Username = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_USER) ?? string.Empty;
        Password = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_PASSWORD) ?? string.Empty;
        RootPassword = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_ROOT_PASSWORD) ?? string.Empty;
    }

    public abstract T BuildConnectionString();
    public abstract T BuildRootConnectionString();
    
    public virtual DbContextOptions<DatabaseContext> GetDbContextOptions()
    {
        var optionsBuilder = BuildDbContextOptions();
        
#if DEBUG
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif
        
        return optionsBuilder.Options;
    }

    protected abstract DbContextOptionsBuilder<DatabaseContext> BuildDbContextOptions();
    
    public virtual bool IsDatabaseOnline(int maxAttempts = IDatabaseConnector<T>.MAXIMUM_ATTEMPTS_TO_CONNECT)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                var tcpClient = new TcpClient(Server, Port);
                tcpClient.Close();
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Waiting for database.");
                Task.Delay(IDatabaseConnector<T>.RETRY_AFTER_MILLISEC).Wait();
            }
        }

        return false;
    }

    public abstract bool IsDatabaseAccessible(bool useRoot = false);
}