using Abp.AspNetCore;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.AspNetZeroCore.Web.Authentication.JwtBearer;
using Abp.Castle.Logging.Log4Net;
using Abp.Dependency;
using Abp.Extensions;
using Abp.PlugIns;
using Castle.Facilities.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.Identity;
using KonbiCloud.Web.Chat.SignalR;
using KonbiCloud.Web.IdentityServer;
using KonbiCloud.Web.RFIDTable.SignalR;
using KonbiCloud.Web.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using PaulMiami.AspNetCore.Mvc.Recaptcha;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using StackExchange.Profiling.Storage;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using Microsoft.AspNetCore.Http;
using System.Text;
using StackExchange.Profiling.Internal;

namespace KonbiCloud.Web.Startup
{
    public class Startup
    {
        public static string SqliteConnectionString { get; } = "Data Source=KonbiCloud; Mode=Memory; Cache=Shared";

        private const string DefaultCorsPolicyName = "localhost";

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IHostingEnvironment _hostingEnvironment;


        public Startup(IHostingEnvironment env)
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
            // TrungPQ add for set Directory is folder run service when host windows service.
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
        }
        private static bool AllowAnyOrigins(string origin)
        {
            var isAllowed = true;

            // Your logic.

            return isAllowed;
        }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //MVC
            services.AddMvc(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory(DefaultCorsPolicyName));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1); ;

            services.AddSignalR(options => { options.EnableDetailedErrors = true; });

            //Configure CORS for angular2 UI
            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, builder =>
                {
                    //App:CorsOrigins in appsettings.json can contain more than one address with splitted by comma.
                    //builder
                    //    .WithOrigins(
                    //        // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                    //        _appConfiguration["App:CorsOrigins"]
                    //            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                    //            .Select(o => o.RemovePostFix("/"))
                    //            .ToArray()
                    //    )
                    //    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    //    .AllowAnyHeader()
                    //    .AllowAnyMethod()
                    //    .AllowCredentials();
                    // Turn off Checking Cross Origin
                    builder
                       .SetIsOriginAllowed(AllowAnyOrigins)
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
                });
            });

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);
            ConfigureAppSettings(services);

            //Identity server
            if (bool.Parse(_appConfiguration["IdentityServer:IsEnabled"]))
            {
                IdentityServerRegistrar.Register(services, _appConfiguration);
            }

            //Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "KonbiCloud API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);

                //Note: This is just for showing Authorize button on the UI. 
                //Authorize button's behaviour is handled in wwwroot/swagger/ui/index.html
                options.AddSecurityDefinition("Bearer", new BasicAuthScheme());
            });

            //Recaptcha
            services.AddRecaptcha(new RecaptchaOptions
            {
                SiteKey = _appConfiguration["Recaptcha:SiteKey"],
                SecretKey = _appConfiguration["Recaptcha:SecretKey"]
            });

            //Hangfire (Enable to use Hangfire instead of default job manager)
            //services.AddHangfire(config =>
            //{
            //    config.UseSqlServerStorage(_appConfiguration.GetConnectionString("Default"));
            //});


            //mini profiler
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                // Control which SQL formatter to use
                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
              

                return;


            }).AddEntityFramework();
            //Configure Abp and Dependency Injection
            return services.AddAbp<KonbiCloudWebHostModule>(options =>
            {
                //Configure Log4Net logging
                  options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig("log4net.config")
                );


                options.PlugInSources.AddFolder(Path.Combine(_hostingEnvironment.WebRootPath, "Plugins"), SearchOption.AllDirectories);
            });


           
        }
       
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            #region MiniProfiler
            // profiling, url to see last profile check: http://localhost:62258/profiler/results
            //You're looking for /profiler/results-index, /profiler/results and /profiler/results-list.
            //This issue here is to address exactly this problem that the / profiler page is non existent and I wan't to have at least some basic urls on /profiler.
            app.UseMiniProfiler();
          
            #endregion

            //Initializes ABP framework.
            app.UseAbp(options =>
            {
                options.UseAbpRequestLocalization = false; //used below: UseAbpRequestLocalization
            });

           app.UseCors(DefaultCorsPolicyName); //Enable CORS!

            app.UseAuthentication();
            app.UseJwtTokenMiddleware();

            if (bool.Parse(_appConfiguration["IdentityServer:IsEnabled"]))
            {
                app.UseJwtTokenMiddleware("IdentityBearer");
                app.UseIdentityServer();
            }

            app.UseStaticFiles();
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }
            var path = Path.Combine(imagePath, _appConfiguration["PlateImageFolder"]);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var tranPath = Path.Combine(imagePath, _appConfiguration["TransactionImageFolder"]);
            if (!Directory.Exists(tranPath))
            {
                Directory.CreateDirectory(tranPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(imagePath),
                RequestPath = $"/{Const.ImageFolder}"
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                if (scope.ServiceProvider.GetService<DatabaseCheckHelper>().Exist(_appConfiguration["ConnectionStrings:Default"]))
                {
                    app.UseAbpRequestLocalization();
                }
            }

            app.UseSignalR(routes =>
            {
                routes.MapHub<AbpCommonHub>("/signalr");
                routes.MapHub<ChatHub>("/signalr-chat");
                //routes.MapHub<RFIDTableHub>("/signalr-rfidtable");
                routes.MapHub<TestMessageHub>("/signalr-test-message");
            });

            //Hangfire dashboard & server (Enable to use Hangfire instead of default job manager)
            //app.UseHangfireDashboard("/hangfire", new DashboardOptions
            //{
            //    Authorization = new[] { new AbpHangfireAuthorizationFilter(AppPermissions.Pages_Administration_HangfireDashboard)  }
            //});
            //app.UseHangfireServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "defaultWithArea",
                    template: "{area}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "KonbiCloud API V1");
                options.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("KonbiCloud.Web.wwwroot.swagger.ui.index.html");
            }); //URL: /swagger

            //new SqliteStorage(SqliteConnectionString).WithSchemaCreation();
           
        }
        private static string profilerIndex()
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html>");
            sb.AppendLine("<title>MiniProfiler Index</title>");
            sb.AppendLine($"<link href=\"/profiler/includes.min.css?v={MiniProfilerBaseOptions.Version}\" rel=\"stylesheet\" />");
            sb.AppendLine("<body>");
            sb.AppendLine("<h2><a href='/profiler/results-index'>List of all tracings (auto-refresh)</a></h2>");
            sb.AppendLine("<h2><a href='/profiler/results'>Current/last request</a></h2>");
            sb.AppendLine("<h4><a href='/profiler/results-list'>List of all requests as json</a></h4>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
        private void ConfigureAppSettings(IServiceCollection services)
        {
            //services.Configure<AzureIoTHubOption>
            //    (_appConfiguration.GetSection("AzureIoTHubOption"));
            //services.Configure<RedisOption>
            //   (_appConfiguration.GetSection("RedisOption"));
            //services.Configure<SlackOption>
            //    (_appConfiguration.GetSection("Slack"));
            //services.Configure<EventBusConfiguration>
            //    (_appConfiguration.GetSection("EventBus"));
        }

    }
}
