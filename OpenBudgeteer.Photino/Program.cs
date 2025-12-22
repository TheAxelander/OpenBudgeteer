using System.Globalization;
using System.Text;
using Dapper;
using DuckDB.NET.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Core.Common.AppSettings;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.DuckDb.Migrations;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.DuckDb;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Photino.Services;
using Photino.Blazor;

namespace OpenBudgeteer.Photino;

public class Program
{
    private static bool _dapperConfigured;

    [STAThread]
    public static void Main(string[] args)
    {
        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);

        // Initialize the file configuration service first (creates config directory if needed)
        var configFileService = new ConfigFileService();
        Console.WriteLine($"Configuration file: {configFileService.ConfigFilePath}");

        appBuilder.Services.AddSingleton(configFileService);
        appBuilder.Services.AddLocalization();
        appBuilder.Services.AddLogging(x => x
#if DEBUG
            .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information)
#endif
            .AddFilter("OpenBudgeteer", LogLevel.Warning)
            .AddConsole());
        appBuilder.Services.AddMudServices();
        appBuilder.Services.AddSingleton(InitializeDatabase()); // Prepare database (check, establish, run migrations, register)
        appBuilder.Services.AddScoped<IServiceManager, DuckDbServiceManager>(x =>
        {
            var connection = x.GetRequiredService<DuckDBConnection>();
            return new DuckDbServiceManager(
                () =>
                {
                    var dbConnection = new DuckDBConnection(connection.ConnectionString);
                    dbConnection.Open();
                    return dbConnection;
                },
                x.GetRequiredService<ILoggerFactory>());
        });
        appBuilder.Services.AddScoped(
            x => new YearMonthSelectorViewModel(x.GetRequiredService<IServiceManager>()));
        appBuilder.Services.AddSingleton<IAppSettingService, PhotinoAppSettingService>(
            x => new PhotinoAppSettingService(x.GetRequiredService<ConfigFileService>()));
        appBuilder.Services.AddSingleton<IMudThemeService, PhotinoMudThemeService>(
            x => new PhotinoMudThemeService(x.GetRequiredService<ConfigFileService>()));

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required to read ANSI Text files

        var cultureInfo = CultureInfo.GetCultureInfoByIetfLanguageTag(
            configFileService.GetStringValue(ConfigurationKeyConstants.APPSETTINGS_CULTURE, "en-US"));
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        // Register root component and selector
        appBuilder.RootComponents.Add<App>("app");
        var app = appBuilder.Build();

        // Manually initialize services (Photino doesn't run IHostedService)
        // Rework of OpenBudgeteer.Blazor.Common.Services.AppInitializerHostedService
        var appSettingService = app.Services.GetRequiredService<IAppSettingService>();
        var mudThemeService = app.Services.GetRequiredService<IMudThemeService>();
        Task.Run(async () =>
        {
            await appSettingService.InitializeAsync();
            await mudThemeService.InitializeAsync();
        }).Wait();

        // Customize window
        app.MainWindow
            .SetIconFile(Path.Combine(AppContext.BaseDirectory, "wwwroot", "icon.ico"))
            .SetTitle("OpenBudgeteer")
            .SetLogVerbosity(0) // Suppress Log Messages floating the console output
            .SetHeight(1200)
            .SetWidth(1600);

        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
        };

        app.Run();
        return;

        DuckDBConnection InitializeDatabase()
        {
            // TODO Check if it makes sense to add this to the manual service init section
            // Rework of OpenBudgeteer.Blazor.Common.Services.DatabaseMigratorService
            // Configure Dapper type handlers for DuckDB
            if (!_dapperConfigured)
            {
                SqlMapper.AddTypeHandler(new DuckDbGuidTypeHandler());
                _dapperConfigured = true;
            }

            // Initialize DuckDB connection (reuse folder from config service)
            var appDataPath = configFileService.ConfigDirectory;
            var databasePath = Path.Combine(appDataPath, "OpenBudgeteer.duckdb");
            var duckDbConnection = new DuckDBConnection($"DataSource={databasePath}");

            // Run DB Migration
            duckDbConnection.Open();
            DuckDbMigrationRunner.ApplyMigrations(duckDbConnection);

            // Optional create Demo Data
            var initWithDemoData = configFileService.GetBoolValue(ConfigurationKeyConstants.APPSETTINGS_DEMO_DATA, false);
            if (initWithDemoData)
            {
                // TODO: Initialize demo data using DuckDB if needed
            }

            return duckDbConnection;
        }
    }
}

