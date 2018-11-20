using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
#if (OrganizationalAuth || IndividualB2CAuth)
using Microsoft.AspNetCore.Authentication;
#endif
#if (OrganizationalAuth)
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
#endif
#if (IndividualB2CAuth)
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
#endif
#if (UseAzureStorage)
using Microsoft.Extensions.Azure.Storage;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#if (AddSwagger)
using NSwag.AspNetCore;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
#endif

namespace Company.WebApplication1
{
    public class App
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) => {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
#if (UseAzureStorage)
                .UseAzureStorage()
#endif
                .UseStartup<App>();

        public App(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if (OrganizationalAuth)
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
#elif (IndividualB2CAuth)
            services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
                .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));
#endif
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
#if (AddSwagger)
            services.AddSwagger();
#endif
#if (UseAzureStorage)
            services.AddSingleton<ITodoRepository, AzureStorageTodoRepository>();
#else
            services.AddSingleton<ITodoRepository, ExampleTodoRepository>();
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
#if (OrganizationalAuth || IndividualAuth)
            app.UseAuthentication();
#endif
            app.UseMvc();
#if (AddSwagger)
            app.UseSwaggerUi3WithApiExplorer();
#endif
        }
    }
}