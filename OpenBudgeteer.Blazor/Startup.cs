using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
    private const string CONNECTION_PROVIDER = "CONNECTION_PROVIDER";
    private const string CONNECTION_SERVER = "CONNECTION_SERVER";
    private const string CONNECTION_PORT = "CONNECTION_PORT";
    private const string CONNECTION_DATABASE = "CONNECTION_DATABASE";
    private const string CONNECTION_USER = "CONNECTION_USER";
    private const string CONNECTION_PASSWORD = "CONNECTION_PASSWORD";
    private const string CONNECTION_MYSQL_ROOT_PASSWORD = "CONNECTION_MYSQL_ROOT_PASSWORD";

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
        
        var provider = Configuration.GetValue<string>(CONNECTION_PROVIDER);
        switch (provider)
        {
            case "mysql":
                SetupMySqlConnection(services);
                break;
            case "sqlite":
                SetupSqliteConnection(services);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Database provider {provider} not supported");
        }
        
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required to read ANSI Text files
    }

    private void SetupMySqlConnection(IServiceCollection services)
    {
        // Check availability of MySql Database
        var iterations = 0;
        while (iterations < 10)
        {
            try
            {
                var tcpClient = new TcpClient(
                    Configuration.GetValue<string>(CONNECTION_SERVER),
                    Configuration.GetValue<int>(CONNECTION_PORT));
                tcpClient.Close();
                break;
            }
            catch (Exception)
            {
                Console.WriteLine("Waiting for database.");
                Task.Delay(5000).Wait();
                iterations++;
            }
        }

        // Create User and Database
        if (!string.IsNullOrEmpty(Configuration.GetValue<string>(CONNECTION_MYSQL_ROOT_PASSWORD)))
        {
            var rootDbConnectionString =
                $"Server={Configuration.GetValue<string>(CONNECTION_SERVER)};" +
                $"Port={Configuration.GetValue<string>(CONNECTION_PORT)};" +
                $"User=root;" +
                $"Password={Configuration.GetValue<string>(CONNECTION_MYSQL_ROOT_PASSWORD)}";
            var mySqlRootDbContext = new MySqlDatabaseContextFactory().CreateDbContext(rootDbConnectionString);
            mySqlRootDbContext.Database.ExecuteSqlRaw(
                $"CREATE USER IF NOT EXISTS '{Configuration.GetValue<string>(CONNECTION_USER)}' " +
                $"IDENTIFIED WITH caching_sha2_password BY '{Configuration.GetValue<string>(CONNECTION_PASSWORD)}';");
            mySqlRootDbContext.Database.ExecuteSqlRaw(
                $"CREATE DATABASE IF NOT EXISTS `{Configuration.GetValue<string>(CONNECTION_DATABASE)}`;");
            mySqlRootDbContext.Database.ExecuteSqlRaw(
                $"GRANT ALL PRIVILEGES ON `{Configuration.GetValue<string>(CONNECTION_DATABASE)}`.* " +
                $"TO '{Configuration.GetValue<string>(CONNECTION_USER)}';");
        }

        // Configure final Database Connection
        var connectionString =
            $"Server={Configuration.GetValue<string>(CONNECTION_SERVER)};" +
            $"Port={Configuration.GetValue<string>(CONNECTION_PORT)};" +
            $"Database={Configuration.GetValue<string>(CONNECTION_DATABASE)};" +
            $"User={Configuration.GetValue<string>(CONNECTION_USER)};" +
            $"Password={Configuration.GetValue<string>(CONNECTION_PASSWORD)}";

        services.AddDbContext<DatabaseContext>(options => options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            b => b.MigrationsAssembly("OpenBudgeteer.Core")),
            ServiceLifetime.Transient);

        // Check on Pending Database Migrations
        var mySqlDbContext = new MySqlDatabaseContextFactory().CreateDbContext(connectionString);
        if (mySqlDbContext.Database.GetPendingMigrations().Any()) mySqlDbContext.Database.Migrate();
    }

    private void SetupSqliteConnection(IServiceCollection services)
    {
        var connectionString = "Data Source=database/openbudgeteer.db";
        services.AddDbContext<DatabaseContext>(options => options.UseSqlite(
            connectionString,
            b => b.MigrationsAssembly("OpenBudgeteer.Core")),
            ServiceLifetime.Transient);

        // Check on Pending Db Migrations
        var sqliteDbContext = new SqliteDatabaseContextFactory().CreateDbContext(connectionString);
        if (sqliteDbContext.Database.GetPendingMigrations().Any()) sqliteDbContext.Database.Migrate();
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
