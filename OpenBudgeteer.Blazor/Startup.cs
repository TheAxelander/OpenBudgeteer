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
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddFileReaderService();
        services.AddScoped<YearMonthSelectorViewModel>();
        var configurationSection = Configuration.GetSection("Connection");
        var provider = configurationSection?["Provider"];
        string connectionString;
        switch (provider)
        {
            case "mysql":
                connectionString = $"Server={configurationSection?["Server"]};" +
                               $"Port={configurationSection?["Port"]};" +
                               $"Database={configurationSection?["Database"]};" +
                               $"User={configurationSection?["User"]};" +
                               $"Password={configurationSection?["Password"]}";
                
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

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });

        // TODO Get Culture from Settings
        var cultureInfo = new CultureInfo("de-DE");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }
}
