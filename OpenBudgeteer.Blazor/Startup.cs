using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.ViewModels;
using Tewr.Blazor.FileReader;

namespace OpenBudgeteer.Blazor;

public class Startup
{
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
        
        var configurationConnectionSection = Configuration.GetSection("Connection");
        var provider = configurationConnectionSection?["Provider"];
        string connectionString;
        switch (provider)
        {
            case "mysql":
                connectionString = $"Server={configurationConnectionSection?["Server"]};" +
                               $"Port={configurationConnectionSection?["Port"]};" +
                               $"Database={configurationConnectionSection?["Database"]};" +
                               $"User={configurationConnectionSection?["User"]};" +
                               $"Password={configurationConnectionSection?["Password"]}";
                
                services.AddDbContext<DatabaseContext>(options => options.UseMySql(
                        connectionString,
                        ServerVersion.AutoDetect(connectionString),
                        b => b.MigrationsAssembly("OpenBudgeteer.Core")),
                    ServiceLifetime.Transient);

                // Check on Pending Db Migrations
                var mySqlDbContext = new MySqlDatabaseContextFactory().CreateDbContext(Configuration);
                if (mySqlDbContext.Database.GetPendingMigrations().Any()) mySqlDbContext.Database.Migrate();
                
                break;
            case "sqlite":
                connectionString = "Data Source=openbudgeteer.db";
                services.AddDbContext<DatabaseContext>(options => options.UseSqlite(
                        connectionString,
                        b => b.MigrationsAssembly("OpenBudgeteer.Core")),
                    ServiceLifetime.Transient);

                // Check on Pending Db Migrations
                var sqliteDbContext = new SqliteDatabaseContextFactory().CreateDbContext(connectionString);
                if (sqliteDbContext.Database.GetPendingMigrations().Any()) sqliteDbContext.Database.Migrate();

                break;
            default:
                throw new ArgumentOutOfRangeException($"Database provider {provider} not supported");
        }
        
        // var configurationAppSettingSection = Configuration.GetSection("AppSettings");
        // var cultureInfo = new CultureInfo(configurationAppSettingSection["Culture"]);
        // CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        // CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        
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
        
        var configurationAppSettingSection = Configuration.GetSection("AppSettings");
        app.UseRequestLocalization(configurationAppSettingSection["Culture"]);

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
