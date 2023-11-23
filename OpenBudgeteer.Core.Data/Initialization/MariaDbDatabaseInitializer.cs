using System;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace OpenBudgeteer.Core.Data.Initialization;

// Creates MySQL/MariaDB databases for the user
// Creates user if not exists
// Creates DB if not exists
// Grants DBO to user on database
public partial class MariaDbDatabaseInitializer : IDatabaseInitializer
{
    public void InitializeDatabase(IConfiguration configuration)
    {
        var rootPassword = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_ROOT_PASSWORD);
        if (string.IsNullOrWhiteSpace(rootPassword))
        {
            // Assume DB created and migrated with init container/manually
            return;
        }
        
        var databaseName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_DATABASE, "openbudgeteer");
        if (!DatabaseNameRegex().IsMatch(databaseName!))
        {
            throw new InvalidOperationException("Database name provided is illegal or SQLi attempt");
        }

        var userName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_USER, databaseName);
        if (!DatabaseNameRegex().IsMatch(userName!))
        {
            throw new InvalidOperationException("User name provided is illegal or SQLi attempt");
        }

        var connectionStringRoot = new MySqlConnectionStringBuilder
        {
            Server = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PORT, 3306u),
            UserID = "root",
            Password = rootPassword,
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };
        
        var connectionStringUser = new MySqlConnectionStringBuilder
        {
            Server = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PORT, 3306u),
            Database = databaseName,
            UserID = userName,
            Password = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_PASSWORD),
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };
        
        using var connection = new MySqlConnection(connectionStringRoot.ConnectionString);
        connection.Open();
        
        using (var command = new MySqlCommand("CREATE USER IF NOT EXISTS @userId IDENTIFIED BY @password;"))
        {
            command.Connection = connection;
            command.Parameters.AddWithValue("@userId", userName);
            command.Parameters.AddWithValue("@password", connectionStringUser.Password);
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand())
        {
            command.Connection = connection;
            // SQLi - CREATE DATABASE with params is NOT supported in MySQL/MariaDB!
            command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`;";
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand())
        {
            command.Connection = connection;
            // SQLi - GRANT with params is NOT supported in MySQL/MariaDB!
            command.CommandText = $"GRANT ALL PRIVILEGES ON {databaseName}.* TO '{userName}';";
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

    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_-]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}