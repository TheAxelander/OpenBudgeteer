using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Services;
using OpenBudgeteer.Core.ViewModels.Helper;
using Tewr.Blazor.FileReader;

namespace OpenBudgeteer.Blazor;

public class Startup
{
    private const string APPSETTINGS_CULTURE = "APPSETTINGS_CULTURE";
    private const string APPSETTINGS_THEME = "APPSETTINGS_THEME";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLocalization();
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddFileReaderService();
        services.AddDatabase(Configuration);
        services.AddScoped<IServiceManager, ServiceManager>(x => new ServiceManager(x.GetRequiredService<DbContextOptions<DatabaseContext>>()));
        services.AddScoped(x => new YearMonthSelectorViewModel(x.GetRequiredService<IServiceManager>()));
        
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required to read ANSI Text files
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        
        app.UseRequestLocalization(Configuration.GetValue<string>(APPSETTINGS_CULTURE, "en-US") ?? "en-US");
        AppSettings.Theme = Configuration.GetValue(APPSETTINGS_THEME, "Default") ?? "Default";

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
