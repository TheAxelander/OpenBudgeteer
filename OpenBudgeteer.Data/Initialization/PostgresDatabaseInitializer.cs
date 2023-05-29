using System;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace OpenBudgeteer.Data.Initialization;

// Initializes Postgres database
// Creates role (user) if not exists
// Creates DB if not exists
// Grants DBO to newly created role.
public partial class PostgresDatabaseInitializer : IDatabaseInitializer
{
    public void InitializeDatabase(IConfiguration configuration)
    {
        var rootPassword = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_ROOT_PASSWORD);
        if (string.IsNullOrWhiteSpace(rootPassword))
        {
            // Assume DB created and migrated with init container/manually
            return;
        }
        
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

        var connectionStringRoot = new NpgsqlConnectionStringBuilder
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
            using var command = new NpgsqlCommand($"CREATE DATABASE {databaseName} OWNER {userName};");
            
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            
            command.ExecuteNonQuery();
        }
        
        {
            using var command = new NpgsqlCommand($"GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO {userName};");
            
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            
            command.ExecuteNonQuery();
        }
    }
    
    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}