using System.Text;
using DotNetEnv;
using DotNetEnv.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
    .AddDotNetEnv(".env", LoadOptions.TraversePath())
    .AddConfiguration(builder.Configuration) // Overwrite values from appsettings.json, compose.yml file or CLI 
    .Build();

builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddLocalization();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddLogging(x => x
#if DEBUG
    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information)
#endif
    .AddFilter("OpenBudgeteer", configuration.GetValue(ConfigurationKeyConstants.LOGLEVEL_DEFAULT_JSON, LogLevel.Information))
    .AddConsole());
builder.Services.AddMudServices();
builder.Services.AddDatabase(configuration); // Check, establish and register database connection
builder.Services.AddHostedService<DatabaseMigratorService>(); // Run database migrations
builder.Services.AddRedis(configuration); // Check, establish and register Redis database connection 
builder.Services.AddScoped<IServiceManager, EFCoreServiceManager>(x => 
    new EFCoreServiceManager(
        x.GetRequiredService<IDbContextFactory<DatabaseContext>>(),
        x.GetRequiredService<ILoggerFactory>()));
builder.Services.AddScoped(x => new YearMonthSelectorViewModel(x.GetRequiredService<IServiceManager>()));
builder.Services.AddSingleton(x => new AppSettingService(x.GetRequiredService<RedisService>()));
builder.Services.AddSingleton(x => new MudThemeService(x.GetRequiredService<RedisService>()));
builder.Services.AddHostedService<AppInitializerHostedService>(); // Initialize and get settings from Redis

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required to read ANSI Text files

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
        
app.UseRequestLocalization(configuration.GetValue<string>(ConfigurationKeyConstants.APPSETTINGS_CULTURE, "en-US"));

app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
