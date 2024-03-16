using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace OpenBudgeteer.Core.Data.OnlineChecker;

// Postgres uses POSIX sockets by default, so we can't build on pinging TCP ports.
public partial class PostgresOnlineChecker : IDatabaseOnlineChecker
{
    public bool IsDbOnline(IConfiguration configuration)
    {
        var databaseName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_DATABASE, "postgres");
        if (!DatabaseNameRegex().IsMatch(databaseName!))
        {
            throw new InvalidOperationException("Provided database name is not valid");
        }

        var userName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_USER, databaseName);
        if (!DatabaseNameRegex().IsMatch(userName!))
        {
            throw new InvalidOperationException("Provided user name is not valid");
        }
        
        var password = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_ROOT_PASSWORD, string.Empty);
        if (string.IsNullOrWhiteSpace(password))
        {
            // Database should be there...
            password = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PASSWORD, string.Empty);
        }
        else
        {
            // We need to init, so we can't build on user's existence.
            userName = "postgres";
        }

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PORT, 5432),
            Database = databaseName,
            Username = userName,
            Password = password,
        };

        for (var i = 0; i < IDatabaseOnlineChecker.MAXIMUM_ATTEMPTS_TO_CONNECT; i++)
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
                connection.Open();
                
                using var command = new NpgsqlCommand("SELECT 1");
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.ExecuteScalar();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Waiting for database: {e.Message}");
                Task.Delay(IDatabaseOnlineChecker.RETRY_AFTER_MILLISEC).Wait();
            }
        }

        return false;
    }

    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}