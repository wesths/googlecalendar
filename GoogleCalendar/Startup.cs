using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using GoogleCalendar.Configurations;
using GoogleCalendar.Configurations.ConfigModels;
using GoogleCalendar.Configurations.Contracts;
using GoogleCalendar.Interfaces;
using GoogleCalendar.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace GoogleCalendar
{
    public class Startup
    {
        

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            //var builder = new ConfigurationBuilder()
            //          .SetBasePath(env.ContentRootPath)
            //          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            //          .AddEnvironmentVariables();

            //Configuration = builder.Build();
            Configuration = configuration;


        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });
            //services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddScoped(cfg => cfg.GetService<IOptionsSnapshot<AppSettings>>().Value);
            services.AddSingleton(Configuration);
            services.AddScoped<ICredentialService, CredentialService>();
            services.AddTransient<IAppointmentService, AppointmentService>();
            services.AddTransient<IConfigService, ConfigService>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
