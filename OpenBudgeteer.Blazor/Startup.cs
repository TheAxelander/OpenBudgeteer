using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazor.FileReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.ViewModels;

namespace OpenBudgeteer.Blazor
{
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
            var connectionString = $"Server={configurationSection?["Server"]};" +
                                   $"Port={configurationSection?["Port"]};" +
                                   $"Database={configurationSection?["Database"]};" +
                                   $"User={configurationSection?["User"]};" +
                                   $"Password={configurationSection?["Password"]}";
            services.AddDbContext<DatabaseContext>(options => options.UseMySql(
                    connectionString, 
                    b => b.MigrationsAssembly("OpenBudgeteer")), 
                ServiceLifetime.Transient);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required to read ANSI Text files
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseContext dbContext)
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

            if (dbContext.Database.GetPendingMigrations().Any()) dbContext.Database.Migrate();
            // TODO Get Culture from Settings
            var cultureInfo = new CultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
