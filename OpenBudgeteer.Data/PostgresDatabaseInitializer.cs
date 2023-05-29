using System;
using System.Data;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace OpenBudgeteer.Data;

// Initializes Postgres database
// Creates role (user) if not exists
// Creates DB if not exists
// Grants DBO to newly created role.ó
public partial class PostgresDatabaseInitializer : IDatabaseInitializer
{
    public void InitializeDatabase(IConfiguration configuration)
    {
        var databaseName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_DATABASE, "postgres");
        if (!DatabaseNameRegex().IsMatch(databaseName))
        {
            throw new InvalidOperationException("Database name provided is illegal or SQLi attempt");
        }

        var userName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_USER, databaseName);
        if (!DatabaseNameRegex().IsMatch(userName))
        {
            throw new InvalidOperationException("User name provided is illegal or SQLi attempt");
        }
        
        var rootPassword = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_ROOT_PASSWORD);
        if (string.IsNullOrWhiteSpace(rootPassword))
        {
            // Assume DB created and migrated with init container/manually
            return;
        }

        var connectionStringRoot = new NpgsqlConnectionStringBuilder()
        {
            Host = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PORT, 5432),
            Username = "postgres",
            Password = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_ROOT_PASSWORD),
        };

        var connectionStringUser = new NpgsqlConnectionStringBuilder
        {
            Host = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PORT, 5432),
            Database = databaseName,
            Username = userName,
            Password = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_PASSWORD, null),
        };
        
        if (!EnsureServerAvailable(connectionStringRoot.Host, connectionStringRoot.Port))
        {
            throw new InvalidOperationException("Specified server not available");
        }

        using var connection = new NpgsqlConnection(connectionStringRoot.ConnectionString);
        connection.Open();

        bool userExists;
        using (var command = new NpgsqlCommand($"SELECT 1 FROM pg_user WHERE usename = '{userName}'"))
        {
            command.Connection = connection;
            command.CommandType = CommandType.Text;

            var exists = command.ExecuteScalar();
            userExists = exists is 1;
        }

        if (!userExists)
        {
            using var command = new NpgsqlCommand();
            
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            command.CommandText =
                $"CREATE ROLE {userName} " +
                $"WITH NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT " +
                $"LOGIN NOREPLICATION " +
                $"PASSWORD {(string.IsNullOrWhiteSpace(connectionStringUser.Password) ? "NULL" : "'" + connectionStringUser.Password + "'")};";
            
            command.ExecuteNonQuery();
        }
        
        bool dbExists;
        using (var command = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'"))
        {
            command.Connection = connection;
            command.CommandType = CommandType.Text;

            var exists = command.ExecuteScalar();
            dbExists = exists is 1;
        }

        if (dbExists) return;
        {
            using var command = new NpgsqlCommand();
            
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            command.CommandText = $"CREATE DATABASE {databaseName} OWNER {userName};";
            
            command.ExecuteNonQuery();
        }
    }
    
    private static bool EnsureServerAvailable(string serverIp, int serverPort)
    {
        const int MAXIMUM_ATTEMPTS_TO_CONNECT = 10;
        const int RETRY_AFTER_MILLISEC = 5000;
        
        for (var i = 0; i < MAXIMUM_ATTEMPTS_TO_CONNECT; i++)
        {
            try
            {
                var tcpClient = new TcpClient(serverIp, serverPort);
                tcpClient.Close();
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Waiting for database.");
                Task.Delay(RETRY_AFTER_MILLISEC).Wait();
            }
        }

        return false;
    }
    
    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_-]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}