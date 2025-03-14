using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using OpenBudgeteer.Blazor;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Services.EFCore;
using OpenBudgeteer.Core.ViewModels.Helper;

const string APPSETTINGS_CULTURE = "APPSETTINGS_CULTURE";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddDatabase(builder.Configuration); // Check, establish and register database connection
builder.Services.AddHostedService<HostedDatabaseMigrator>(); // Run database migrations
builder.Services.AddScoped<IServiceManager, EFCoreServiceManager>(x => new EFCoreServiceManager(x.GetRequiredService<DbContextOptions<DatabaseContext>>()));
builder.Services.AddScoped(x => new YearMonthSelectorViewModel(x.GetRequiredService<IServiceManager>()));
builder.Services.AddSingleton(x => new AppSettings(builder.Configuration));

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
        
app.UseRequestLocalization(builder.Configuration.GetValue<string>(APPSETTINGS_CULTURE, "en-US"));

app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

