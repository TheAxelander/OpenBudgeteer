using System;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace OpenBudgeteer.Data;

public partial class PostgresDatabaseInitializer : IDatabaseInitializer
{
    private const string CONNECTION_SERVER = "CONNECTION_SERVER";
    private const string CONNECTION_PORT = "CONNECTION_PORT";
    private const string CONNECTION_DATABASE = "CONNECTION_DATABASE";
    private const string CONNECTION_USER = "CONNECTION_USER";
    private const string CONNECTION_PASSWORD = "CONNECTION_PASSWORD";
    private const string CONNECTION_ROOT_PASSWORD = "CONNECTION_ROOT_PASSWORD";


    public void InitializeDatabase(IConfiguration configuration)
    {
        var databaseName = configuration.GetValue(CONNECTION_DATABASE, "postgres");
        if (!DatabaseNameRegex().IsMatch(databaseName))
        {
            throw new InvalidOperationException("Database name provided is illegal or SQLi attempt");
        }

        var userName = configuration.GetValue(CONNECTION_USER, databaseName);
        if (!DatabaseNameRegex().IsMatch(userName))
        {
            throw new InvalidOperationException("User name provided is illegal or SQLi attempt");
        }

        var connectionStringRoot = new NpgsqlConnectionStringBuilder()
        {
            Host = configuration.GetValue(CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(CONNECTION_PORT, 5432),
            Username = "postgres",
            Password = configuration.GetValue<string>(CONNECTION_ROOT_PASSWORD),
        };

        var connectionStringUser = new NpgsqlConnectionStringBuilder
        {
            Host = configuration.GetValue(CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(CONNECTION_PORT, 5432),
            Database = databaseName,
            Username = userName,
            Password = configuration.GetValue<string>(CONNECTION_PASSWORD, null),
        };

        using var connection = new NpgsqlConnection(connectionStringRoot.ConnectionString);
        connection.Open();

        bool userExists;
        using (var command = new NpgsqlCommand($"SELECT 1 FROM pg_user WHERE username = '{userName}'"))
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
        
        if (!dbExists)
        {
            using var command = new NpgsqlCommand();
            
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            command.CommandText = $"CREATE DATABASE {databaseName} WITH OWNER {userName};";
            
            command.ExecuteNonQuery();
        }
    }
    
    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}