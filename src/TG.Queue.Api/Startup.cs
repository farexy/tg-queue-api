using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TG.Core.App.Configuration;
using TG.Core.App.Configuration.Auth;
using TG.Core.App.Configuration.TgConfig;
using TG.Core.App.InternalCalls;
using TG.Core.App.Middlewares;
using TG.Core.App.Swagger;
using TG.Core.Redis.Extensions;
using TG.Core.ServiceBus.Extensions;
using TG.Core.ServiceBus.Messages;
using TG.Queue.Api.Config;
using TG.Queue.Api.Config.Options;
using TG.Queue.Api.ServiceClients;
using TG.Queue.Api.Services;

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
            services.Configure<BattleSettings>(Configuration.GetSection(TgConfigs.BattleSettings));

            services.AddServiceClient<IBattleServersClient>(Configuration.GetServiceInternalUrl(TgServices.Manager));
            services.AddServiceClient<IBattlesClient>(Configuration.GetServiceInternalUrl(TgServices.Game));
            services.AddServiceClient<IUsersClient>(Configuration.GetServiceInternalUrl(TgServices.Game));

            services.AddTgSwagger(opt =>
            {
                opt.ServiceName = ServiceConst.ServiceName;
                opt.ProjectName = ServiceConst.ProjectName;
                opt.AppVersion = "1";
            });

            services.AddTgRedis(Configuration);
            services.AddTransient<IBattlesStorage, BattlesStorage>();
            services.AddScoped<ITestBattlesHelper, TestBattlesHelper>();

            services.AddServiceBus(Configuration)
                .AddQueueProducer<PrepareBattleMessage>();
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
                endpoints.MapTgConfigs(ServiceConst.ServiceName);
            });
        }
    }
}