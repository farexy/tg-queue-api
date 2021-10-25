using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TG.Core.App.Configuration;
using TG.Core.App.Configuration.Auth;
using TG.Core.App.InternalCalls;
using TG.Core.App.Middlewares;
using TG.Core.App.Swagger;
using TG.Core.Redis.Extensions;

namespace TG.Queue.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddTgJsonOptions()
                .AddInvalidModelStateHandling(); 
            services.AddHealthChecks();
            //.AddNpgSqlHealthCheck();
            //services.AddKubernetesTgApplicationInsights(Configuration);
            services.AddApiVersioning();

            services.AddPostgresDb<ApplicationDbContext>(Configuration, ServiceConst.ServiceName);
            
            services.AddCors(cors => cors.AddDefaultPolicy(p =>
            {
                p.AllowAnyHeader();
                p.AllowAnyMethod();
                p.AllowAnyOrigin();
            }));

            services.AddTgAuth(Configuration);
            services.AddAutoMapper<Startup>();
            services.AddMediatR(typeof(Startup));
                
            services.AddTgServices();

            services.ConfigureInternalCalls(Configuration);

            services.AddTgSwagger(opt =>
            {
                opt.ServiceName = ServiceConst.ServiceName;
                opt.ProjectName = ServiceConst.ProjectName;
                opt.AppVersion = "1";
            });

            services.AddTgRedis(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseTgSwagger();

            app.UseCors();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<TracingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}