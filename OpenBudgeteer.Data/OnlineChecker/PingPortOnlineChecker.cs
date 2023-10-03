using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Data.OnlineChecker;

// When we do the init, the MariaDB/MySQL database doesn't exist.
// So a select 1 would fail at the MDB version checker.
// But it has a TCP port we can use...
public class PingPortOnlineChecker : IDatabaseOnlineChecker
{
    public bool IsDbOnline(IConfiguration configuration)
    {
        var serverIp = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_SERVER, "localhost");
        var serverPort = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PORT, 3306);
        
        for (var i = 0; i < IDatabaseOnlineChecker.MAXIMUM_ATTEMPTS_TO_CONNECT; i++)
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
                Task.Delay(IDatabaseOnlineChecker.RETRY_AFTER_MILLISEC).Wait();
            }
        }

        return false;
    }
}