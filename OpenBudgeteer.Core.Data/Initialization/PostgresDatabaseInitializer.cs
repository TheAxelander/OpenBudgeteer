using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OpenBudgeteer.Core.Data.Connection;

namespace OpenBudgeteer.Core.Data.Initialization;

// Initializes Postgres database
// Creates role (user) if not exists
// Creates DB if not exists
// Grants DBO to newly created role.
public class PostgresDatabaseInitializer : IDatabaseInitializer
{
    public void InitializeDatabase(IDatabaseConnector<DbConnectionStringBuilder> databaseConnector)
    {
        if (string.IsNullOrWhiteSpace(databaseConnector.RootPassword))
        {
            // Assume DB created and migrated with init container/manually
            return;
        }

        using var connection = new NpgsqlConnection(databaseConnector.BuildRootConnectionString().ConnectionString);
        connection.Open();

        bool userExists;
        using (var command = new NpgsqlCommand($"SELECT 1 FROM pg_user WHERE usename = '{databaseConnector.Username}'"))
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
                $"CREATE ROLE {databaseConnector.Username} " +
                $"WITH NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT " +
                $"LOGIN NOREPLICATION " +
                $"PASSWORD {(string.IsNullOrWhiteSpace(databaseConnector.Password) ? "NULL" : "'" + databaseConnector.Password + "'")};";
            
            command.ExecuteNonQuery();
        }
        
        bool dbExists;
        using (var command = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{databaseConnector.Database}'"))
        {
            command.Connection = connection;
            command.CommandType = CommandType.Text;

            var exists = command.ExecuteScalar();
            dbExists = exists is 1;
        }

        if (dbExists) return;
        {
            using var command = new NpgsqlCommand($"CREATE DATABASE {databaseConnector.Database} OWNER {databaseConnector.Username};");
            
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            
            command.ExecuteNonQuery();
        }
        
        {
            using var command = new NpgsqlCommand($"GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO {databaseConnector.Username};");
            
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            
            command.ExecuteNonQuery();
        }
    }
}