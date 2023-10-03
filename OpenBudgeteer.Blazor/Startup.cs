using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.ViewModels;
using OpenBudgeteer.Data;
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
        services.AddScoped<YearMonthSelectorViewModel>();
        services.AddDatabase(Configuration);
        
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
        
        app.UseRequestLocalization(Configuration.GetValue<string>(APPSETTINGS_CULTURE));
        AppSettings.Theme = Configuration.GetValue(APPSETTINGS_THEME, "Default");

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
