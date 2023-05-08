using System;
using System.Data;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace OpenBudgeteer.Data;

public class MariaDbDatabaseInitializer : IDatabaseInitializer
{
    private const string CONNECTION_SERVER = "CONNECTION_SERVER";
    private const string CONNECTION_PORT = "CONNECTION_PORT";
    private const string CONNECTION_DATABASE = "CONNECTION_DATABASE";
    private const string CONNECTION_USER = "CONNECTION_USER";
    private const string CONNECTION_PASSWORD = "CONNECTION_PASSWORD";
    private const string CONNECTION_MYSQL_ROOT_PASSWORD = "CONNECTION_MYSQL_ROOT_PASSWORD";
    
    public void InitializeDatabase(IConfiguration configuration)
    {
        var connectionStringRoot = new MySqlConnectionStringBuilder
        {
            Server = configuration.GetValue<string>(CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue<uint>(CONNECTION_PORT, 3306u),
            Database = configuration.GetValue<string>(CONNECTION_DATABASE, "openbudgeteer"),
            UserID = "root",
            Password = configuration.GetValue<string>(CONNECTION_MYSQL_ROOT_PASSWORD),
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };
        
        var connectionStringUser = new MySqlConnectionStringBuilder
        {
            Server = configuration.GetValue<string>(CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue<uint>(CONNECTION_PORT, 3306u),
            Database = configuration.GetValue<string>(CONNECTION_DATABASE, "openbudgeteer"),
            UserID = configuration.GetValue<string>(CONNECTION_USER, "openbudgeteer"),
            Password = configuration.GetValue<string>(CONNECTION_PASSWORD),
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };
        
        if (!EnsureServerAvailable(connectionStringRoot.Server, (int)connectionStringRoot.Port))
        {
            throw new InvalidOperationException("Specified server not available");
        }
        
        const string createUserCommand = @"CREATE USER IF NOT EXISTS @userId IDENTIFIED WITH caching_sha2_password BY @password;";
        const string createDatabaseCommand = @"CREATE DATABASE IF NOT EXISTS @database;";
        const string rootDatabaseCommand = @"GRANT ALL PRIVILEGES ON @database TO @userId;";
        
        using var connection = new MySqlConnection(connectionStringRoot.ConnectionString);
        using (var command = new MySqlCommand(createUserCommand))
        {
            command.Connection = connection;
            command.Parameters.AddWithValue("@userId", connectionStringUser.UserID);
            command.Parameters.AddWithValue("@password", connectionStringUser.Password);
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand(createDatabaseCommand))
        {
            command.Connection = connection;
            command.Parameters.AddWithValue("@database", connectionStringUser.Database);
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }
        
        using (var command = new MySqlCommand(rootDatabaseCommand))
        {
            command.Connection = connection;
            command.Parameters.AddWithValue("@database", connectionStringUser.Database);
            command.Parameters.AddWithValue("@userId", connectionStringUser.UserID);
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
}