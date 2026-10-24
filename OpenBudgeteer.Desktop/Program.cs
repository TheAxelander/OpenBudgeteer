using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using DotNetEnv;
using DotNetEnv.Configuration;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using OpenBudgeteer.Blazor;
using OpenBudgeteer.Blazor.Common.Extensions;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Initialization;
using OpenBudgeteer.Core.Data.Services.EFCore;
using OpenBudgeteer.Core.ViewModels.Helper;
using Photino.Blazor;

namespace OpenBudgeteer.Desktop;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddDotNetEnv(".env", LoadOptions.TraversePath())
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Desktop App is single-user, no login required
                [ConfigurationKeyConstants.APPSETTINGS_AUTH_ENABLED] = "false"
            })
            .Build();

        var culture = new CultureInfo(configuration.GetValue<string>(ConfigurationKeyConstants.APPSETTINGS_CULTURE, "en-US")!);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required to read ANSI Text files

        var builder = PhotinoBlazorAppBuilder.CreateDefault(args);

        builder.Services.AddSingleton<IConfiguration>(configuration);
        builder.Services.AddLogging(x => x
#if DEBUG
            .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information)
#endif
            .AddFilter("OpenBudgeteer", configuration.GetValue(ConfigurationKeyConstants.LOGLEVEL_DEFAULT_JSON, LogLevel.Information))
            .AddConsole());
        builder.Services.AddLocalization();
        builder.Services.AddMudServices();
        builder.Services.AddDatabase(configuration); // Check, establish and register database connection
        builder.Services.AddRedis(configuration); // Check, establish and register Redis database connection
        builder.Services.AddSingleton<AppAuthenticationService>();
        builder.Services.AddScoped<IServiceManager, EFCoreServiceManager>(x =>
            new EFCoreServiceManager(
                x.GetRequiredService<IDbContextFactory<DatabaseContext>>(),
                x.GetRequiredService<ILoggerFactory>()));
        builder.Services.AddScoped(x => new YearMonthSelectorViewModel(x.GetRequiredService<IServiceManager>()));
        builder.Services.AddSingleton(x => new AppSettingService(x.GetRequiredService<RedisService>()));
        builder.Services.AddSingleton(x => new MudThemeService(x.GetRequiredService<RedisService>()));

        builder.RootComponents.Add<Routes>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var app = builder.Build();

        // Photino has no generic host, so run what the Web App does via IHostedService manually
        new DatabaseMigratorService(
                app.Services.GetRequiredService<IDbContextFactory<DatabaseContext>>(),
                configuration)
            .StartAsync(CancellationToken.None).GetAwaiter().GetResult();
        new AppInitializerHostedService(
                app.Services.GetRequiredService<AppSettingService>(),
                app.Services.GetRequiredService<MudThemeService>())
            .StartAsync(CancellationToken.None).GetAwaiter().GetResult();

        app.MainWindow
            .SetTitle("OpenBudgeteer")
            .SetSize(1400, 900)
            .Center();

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            app.MainWindow.ShowMessage("Fatal exception", e.ExceptionObject.ToString());
        };

        app.Run();
    }
}
