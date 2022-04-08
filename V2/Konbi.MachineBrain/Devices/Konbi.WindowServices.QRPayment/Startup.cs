using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Konbi.WindowServices.QRPayment.Configuration;
using Konbi.WindowServices.QRPayment.Jobs;
using Konbi.WindowServices.QRPayment.SelfHost;
using KonbiBrain.Common.Services;
using KonbiBrain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Konbi.WindowServices.QRPayment
{
    public class Startup
    {
        private const string DefaultCorsPolicyName = "CorsPolicy";
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IHostingEnvironment env)
        {
            _hostingEnvironment = env;

            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _appConfiguration = builder.Build();
            
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _appConfiguration["Authentication:Issuer"],
                    ValidAudience = _appConfiguration["Authentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appConfiguration["Authentication:SigningKey"]))
                };
            });

            services.AddMvc(c =>
            {
                c.Filters.Add(new CorsAuthorizationFilterFactory(DefaultCorsPolicyName));
                //c.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddJsonOptions(options =>
            {

                options.SerializerSettings.Formatting = Formatting.Indented;
            });

            //services.AddCors(o => o.AddPolicy("CorsPolicy", builder => {
            //    builder
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    .AllowCredentials()
            //    .WithOrigins("http://localhost:8888");
            //}));

            services.AddSignalR();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("fomo", new Info { Title = "FOMO Pay", Version = "v1" });
                options.SwaggerDoc("token", new Info { Title = "Token", Version = "v1" });

                //options.AddSecurityDefinition("Bearer", new BasicAuthScheme());
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };

                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Token for API. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                //options.DocumentFilter<SwaggerFilterDocument>();
                options.AddSecurityRequirement(security);
                options.IgnoreObsoleteProperties();
            });
           
            ConfigureAppSettings(services);
            ConfigService(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();

            var swaggerFolder = _hostingEnvironment.ContentRootPath + @"\wwwroot\swagger\ui";
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(swaggerFolder),
                RequestPath = $"/swagger-ui"
            });

            app.UseSwagger(options =>
            {
                options.RouteTemplate = "docs/{documentName}.json";
            });
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/docs/fomo.json", "FOMO Pay");
                options.SwaggerEndpoint("/docs/token.json", "Token");

                // Point URL to root
                options.RoutePrefix = string.Empty;

                //// Add custom CSS
                //options.InjectStylesheet("/swagger-ui/custom.css");

                //// Add custom JS
                //options.InjectJavascript("/swagger-ui/jquery-3.4.1.min.js");
                //options.InjectJavascript("/swagger-ui/custom.js");

                options.IndexStream = () => Assembly.GetExecutingAssembly().GetManifestResourceStream("Konbi.WindowServices.QRPayment.wwwroot.swagger.ui.index.html");
            });

            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

           // app.UseCors("CorsPolicy");
        }

        private void ConfigService(IServiceCollection services)
        {
            services.AddSingleton(typeof(Services.Core.LogService));
            services.AddSingleton(typeof(Services.Core.FomoService));
           
            services.AddSingleton< IMessageProducerService, NsqMessageProducerService>();
            services.AddHostedService<SettlePaymentTask>();
            services.BuildServiceProvider();
            

        }

        private void ConfigureAppSettings(IServiceCollection services)
        {
            services.Configure<AppConfiguration>(_appConfiguration.GetSection("App"));
            services.Configure<AuthConfiguration>(_appConfiguration.GetSection("Authentication"));           
            //services.Configure<FomoConfiguration>(_appConfiguration.GetSection("Fomo"));
            services.Configure<FomoConfiguration>(options=> {
                options.KeyId = _appConfiguration["Fomo:KeyId"];
                options.VendorPrivateKey = _appConfiguration["Fomo:VendorPrivateKey"];
                options.FomoPublicKey = _appConfiguration["Fomo:FomoPublicKey"];

                options.MID = _appConfiguration["Fomo:MID"];
                options.TID = _appConfiguration["Fomo:TID"];
                options.Url = _appConfiguration["Fomo:Url"];
               
            });
        }
    }
}
