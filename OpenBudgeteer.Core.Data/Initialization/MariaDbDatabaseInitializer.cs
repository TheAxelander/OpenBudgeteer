using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using OpenBudgeteer.Core.Data.Connection;

namespace OpenBudgeteer.Core.Data.Initialization;

// Creates MySQL/MariaDB databases for the user
// Creates user if not exists
// Creates DB if not exists
// Grants DBO to user on database
public class MariaDbDatabaseInitializer : IDatabaseInitializer
{
    public void InitializeDatabase(IDatabaseConnector<DbConnectionStringBuilder> databaseConnector)
    {
        if (string.IsNullOrWhiteSpace(databaseConnector.RootPassword))
        {
            // Assume DB created and migrated with init container/manually
            return;
        }
        
        using var connection = new MySqlConnection(databaseConnector.BuildRootConnectionString().ConnectionString);
        connection.Open();
        
        using (var command = new MySqlCommand("CREATE USER IF NOT EXISTS @userId IDENTIFIED BY @password;"))
        {
            command.Connection = connection;
            command.Parameters.AddWithValue("@userId", databaseConnector.Username);
            command.Parameters.AddWithValue("@password", databaseConnector.Password);
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand())
        {
            command.Connection = connection;
            // SQLi - CREATE DATABASE with params is NOT supported in MySQL/MariaDB!
            command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{databaseConnector.Database}`;";
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand())
        {
            command.Connection = connection;
            // SQLi - GRANT with params is NOT supported in MySQL/MariaDB!
            command.CommandText = $"GRANT ALL PRIVILEGES ON `{databaseConnector.Database}`.* TO `{databaseConnector.Username}`;";
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = "FLUSH PRIVILEGES;";
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
    }
}