using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Blazor;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Services;
using OpenBudgeteer.Core.Data.Services.EFCore;
using OpenBudgeteer.Core.ViewModels.Helper;
using Tewr.Blazor.FileReader;

const string APPSETTINGS_CULTURE = "APPSETTINGS_CULTURE";
const string APPSETTINGS_THEME = "APPSETTINGS_THEME";

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFileReaderService();
builder.Services.AddHostedService<HostedDatabaseMigrator>();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddScoped<IServiceManager, EFCoreServiceManager>(x => new EFCoreServiceManager(x.GetRequiredService<DbContextOptions<DatabaseContext>>()));
builder.Services.AddScoped(x => new YearMonthSelectorViewModel(x.GetRequiredService<IServiceManager>()));
        
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
        
app.UseRequestLocalization(builder.Configuration.GetValue<string>(APPSETTINGS_CULTURE, "en-US") ?? "en-US");
AppSettings.Theme = builder.Configuration.GetValue(APPSETTINGS_THEME, "Default") ?? "Default";

//app.UseRouting();
app.UseAntiforgery();
/*app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorComponents<App>().AddInteractiveServerRenderMode();
});*/
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

