using System.Globalization;
using System.Text;
using DuckDB.NET.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using OpenBudgeteer.Blazor;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Core.Common.AppSettings;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Connection;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.DuckDb.Migrations;
using OpenBudgeteer.Core.Data.Services.DuckDb;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Desktop.Services;
using Photino.Blazor;

namespace OpenBudgeteer.Desktop;

internal class Program
{
    private const string DATABASE_FILE_NAME = "OpenBudgeteer.duckdb";

    [STAThread]
    private static void Main(string[] args)
    {
        // Settings are stored in a local config file (instead of .env and Redis like the Web App)
        var configFileService = new ConfigFileService();
        Console.WriteLine($"Configuration file: {configFileService.ConfigFilePath}");

        // Shared Blazor components check if authentication is enabled. Desktop App is single-user, no login required
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ConfigurationKeyConstants.APPSETTINGS_AUTH_ENABLED] = "false"
            })
            .Build();

        var culture = new CultureInfo(configFileService.GetStringValue(ConfigurationKeyConstants.APPSETTINGS_CULTURE, "en-US"));
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required to read ANSI Text files

        var databaseConnection = InitializeDatabase(configFileService); // Prepare database (establish, run migrations)

        var builder = PhotinoBlazorAppBuilder.CreateDefault(args);

        builder.Services.AddSingleton<IConfiguration>(configuration);
        builder.Services.AddSingleton(configFileService);
        builder.Services.AddLogging(x => x
            .AddFilter("OpenBudgeteer", LogLevel.Warning)
            .AddConsole());
        builder.Services.AddLocalization();
        builder.Services.AddMudServices();
        builder.Services.AddSingleton(databaseConnection);
        builder.Services.AddSingleton(new DatabaseConnectionInfo(
            "DuckDB",
            configFileService.ConfigDirectory,
            DATABASE_FILE_NAME,
            Environment.UserName));
        builder.Services.AddScoped<IServiceManager, DuckDbServiceManager>(x =>
            new DuckDbServiceManager(
                () =>
                {
                    var connection = new DuckDBConnection(databaseConnection.ConnectionString);
                    connection.Open();
                    return connection;
                },
                x.GetRequiredService<ILoggerFactory>()));
        builder.Services.AddScoped(x => new YearMonthSelectorViewModel(x.GetRequiredService<IServiceManager>()));
        builder.Services.AddSingleton<IAppSettingService, DesktopAppSettingService>(
            x => new DesktopAppSettingService(x.GetRequiredService<ConfigFileService>()));
        builder.Services.AddSingleton<IMudThemeService, DesktopMudThemeService>(
            x => new DesktopMudThemeService(x.GetRequiredService<ConfigFileService>()));

        builder.RootComponents.Add<Routes>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var app = builder.Build();

        // Photino has no generic host, so run what the Web App does via IHostedService manually
        new AppInitializerHostedService(
                app.Services.GetRequiredService<IAppSettingService>(),
                app.Services.GetRequiredService<IMudThemeService>())
            .StartAsync(CancellationToken.None).GetAwaiter().GetResult();

        app.MainWindow
            .SetTitle("OpenBudgeteer")
            .SetLogVerbosity(0) // Suppress Log Messages flooding the console output
            .SetSize(1400, 900)
            .Center();

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            app.MainWindow.ShowMessage("Fatal exception", e.ExceptionObject.ToString());
        };

        app.Run();
    }

    private static DuckDBConnection InitializeDatabase(ConfigFileService configFileService)
    {
        var databasePath = Path.Combine(configFileService.ConfigDirectory, DATABASE_FILE_NAME);
        DuckDbMigrationRunner.ApplyMigrations(databasePath); // Runs before opening the connection to allow a file backup

        var connection = new DuckDBConnection($"DataSource={databasePath}");
        connection.Open();

        // TODO: Initialize demo data using DuckDB if APPSETTINGS_DEMO_DATA is set

        return connection;
    }
}
