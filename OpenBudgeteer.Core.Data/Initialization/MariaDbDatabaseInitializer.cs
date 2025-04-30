using System.Data;
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
    public void InitializeDatabase(IConfiguration configuration)
    {
        var dbConnectionBuilder = new MariaDbConnector(configuration);
        if (string.IsNullOrWhiteSpace(dbConnectionBuilder.RootPassword))
        {
            // Assume DB created and migrated with init container/manually
            return;
        }
        
        using var connection = new MySqlConnection(dbConnectionBuilder.BuildRootConnectionString().ConnectionString);
        connection.Open();
        
        using (var command = new MySqlCommand("CREATE USER IF NOT EXISTS @userId IDENTIFIED BY @password;"))
        {
            command.Connection = connection;
            command.Parameters.AddWithValue("@userId", dbConnectionBuilder.Username);
            command.Parameters.AddWithValue("@password", dbConnectionBuilder.Password);
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand())
        {
            command.Connection = connection;
            // SQLi - CREATE DATABASE with params is NOT supported in MySQL/MariaDB!
            command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{dbConnectionBuilder.Database}`;";
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand())
        {
            command.Connection = connection;
            // SQLi - GRANT with params is NOT supported in MySQL/MariaDB!
            command.CommandText = $"GRANT ALL PRIVILEGES ON `{dbConnectionBuilder.Database}`.* TO `{dbConnectionBuilder.Username}`;";
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