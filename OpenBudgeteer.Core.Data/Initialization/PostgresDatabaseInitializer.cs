using System.Data;
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
    public void InitializeDatabase(IConfiguration configuration)
    {
        var dbConnectionBuilder = new PostgresConnector(configuration);
        if (string.IsNullOrWhiteSpace(dbConnectionBuilder.RootPassword))
        {
            // Assume DB created and migrated with init container/manually
            return;
        }

        using var connection = new NpgsqlConnection(dbConnectionBuilder.BuildRootConnectionString().ConnectionString);
        connection.Open();

        bool userExists;
        using (var command = new NpgsqlCommand($"SELECT 1 FROM pg_user WHERE usename = '{dbConnectionBuilder.Username}'"))
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
                $"CREATE ROLE {dbConnectionBuilder.Username} " +
                $"WITH NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT " +
                $"LOGIN NOREPLICATION " +
                $"PASSWORD {(string.IsNullOrWhiteSpace(dbConnectionBuilder.Password) ? "NULL" : "'" + dbConnectionBuilder.Password + "'")};";
            
            command.ExecuteNonQuery();
        }
        
        bool dbExists;
        using (var command = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{dbConnectionBuilder.Database}'"))
        {
            command.Connection = connection;
            command.CommandType = CommandType.Text;

            var exists = command.ExecuteScalar();
            dbExists = exists is 1;
        }

        if (dbExists) return;
        {
            using var command = new NpgsqlCommand($"CREATE DATABASE {dbConnectionBuilder.Database} OWNER {dbConnectionBuilder.Username};");
            
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            
            command.ExecuteNonQuery();
        }
        
        {
            using var command = new NpgsqlCommand($"GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO {dbConnectionBuilder.Username};");
            
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            
            command.ExecuteNonQuery();
        }
    }
}