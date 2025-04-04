using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Core.Data;
using StackExchange.Redis;

namespace OpenBudgeteer.Blazor.Common.Extensions;

public static class RedisInitializationExtension
{
    public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var server = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_REDIS_SERVER, "localhost");
        var port = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_REDIS_PORT, 6379);
        var username = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_REDIS_USER, string.Empty);
        var password = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_REDIS_PASSWORD, string.Empty);
        var prefix = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_REDIS_PREFIX, string.Empty);
        
        // Check connectivity
        var isOnline = IsDatabaseOnline(server, port);
        if (!isOnline)
        {
            throw new InvalidOperationException("Redis database is not online.");
        }
        
        // Register Redis database
        var connectionString = $"{server}:{port}";
        if (!string.IsNullOrEmpty(username)) connectionString += $",user={username}";
        if (!string.IsNullOrEmpty(password)) connectionString += $",password={password}";
        
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));
        services.AddScoped(x => new RedisService(x.GetRequiredService<IConnectionMultiplexer>(), prefix));
    }
    
    private static bool IsDatabaseOnline(string server, int port, int maxAttempts = 10)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                var tcpClient = new TcpClient(server, port);
                tcpClient.Close();
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Waiting for Redis database.");
                Task.Delay(5000).Wait();
            }
        }

        return false;
    }
}