using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Blazor;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        using (var scope = host.Services.CreateScope())
        {
            EnsureDatabaseMigrated(scope.ServiceProvider);
        }

        host.Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

    private static void EnsureDatabaseMigrated(IServiceProvider serviceLocator)
    {
        var initializer = serviceLocator.GetRequiredService<IDatabaseInitializer>();
        var configuration = serviceLocator.GetRequiredService<IConfiguration>();
        initializer.InitializeDatabase(configuration);
            
        var db = serviceLocator.GetRequiredService<DatabaseContext>();
        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }
    
    
}
