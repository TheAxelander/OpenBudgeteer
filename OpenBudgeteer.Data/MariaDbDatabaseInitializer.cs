using System;
using System.Data;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace OpenBudgeteer.Data;

public partial class MariaDbDatabaseInitializer : IDatabaseInitializer
{
    private const string CONNECTION_SERVER = "CONNECTION_SERVER";
    private const string CONNECTION_PORT = "CONNECTION_PORT";
    private const string CONNECTION_DATABASE = "CONNECTION_DATABASE";
    private const string CONNECTION_USER = "CONNECTION_USER";
    private const string CONNECTION_PASSWORD = "CONNECTION_PASSWORD";
    private const string CONNECTION_ROOT_PASSWORD = "CONNECTION_ROOT_PASSWORD";
    
    public void InitializeDatabase(IConfiguration configuration)
    {
        var databaseName = configuration.GetValue(CONNECTION_DATABASE, "openbudgeteer");
        if (!DatabaseNameRegex().IsMatch(databaseName))
        {
            throw new InvalidOperationException("Database name provided is illegal or SQLi attempt");
        }

        var userName = configuration.GetValue(CONNECTION_USER, databaseName);
        if (!DatabaseNameRegex().IsMatch(userName))
        {
            throw new InvalidOperationException("User name provided is illegal or SQLi attempt");
        }

        var connectionStringRoot = new MySqlConnectionStringBuilder
        {
            Server = configuration.GetValue(CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(CONNECTION_PORT, 3306u),
            UserID = "root",
            Password = configuration.GetValue<string>(CONNECTION_ROOT_PASSWORD),
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };
        
        var connectionStringUser = new MySqlConnectionStringBuilder
        {
            Server = configuration.GetValue(CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(CONNECTION_PORT, 3306u),
            Database = databaseName,
            UserID = userName,
            Password = configuration.GetValue<string>(CONNECTION_PASSWORD),
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };
        
        if (!EnsureServerAvailable(connectionStringRoot.Server, (int)connectionStringRoot.Port))
        {
            throw new InvalidOperationException("Specified server not available");
        }
        
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
            command.CommandText = $@"CREATE DATABASE IF NOT EXISTS `{databaseName}`;";
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand())
        {
            command.Connection = connection;
            // SQLi - GRANT with params is NOT supported in MySQL/MariaDB!
            command.CommandText = @$"GRANT ALL PRIVILEGES ON {databaseName}.* TO '{userName}';";
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = @$"FLUSH PRIVILEGES;";
            command.CommandType = CommandType.Text;

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

    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}