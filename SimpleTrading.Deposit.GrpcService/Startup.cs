using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDependencies;
using Prometheus;
using ProtoBuf.Grpc.Server;
using SimpleTrading.BaseMetrics;
using SimpleTrading.ServiceStatusReporterConnector;

namespace SimpleTrading.Deposit.GrpcService
{
    public class Startup
    {
        private static readonly MyIoc Ioc = new MyIoc();
        private static readonly SettingModel Settings = SettingsReader.SettingsReader.ReadSettings<SettingModel>();
        private MyServiceBus.TcpClient.MyServiceBusTcpClient _bus;

        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            services.AddControllers();

            services.AddApplicationInsightsTelemetry(Configuration);
            var logger = Ioc.BindSeqLogger(Settings);
            Ioc.BindAzureTables(Settings);
            Ioc.BindGrpcServices(Settings);
            Ioc.BindDatabaseDepositRepository(Settings);
            Ioc.BindPaymentManager(Settings, logger);
            Ioc.BindQueues();
            Ioc.BindServices();
            var myServiceBusTcpClient = Ioc.BindServiceBus();
            _bus = myServiceBusTcpClient;

            services.AddCodeFirstGrpc(options =>
            {
                options.Interceptors.Add<LoggerInterceptor>(logger);
                options.BindMetricsInterceptors();
            });
            
            ServiceLocator.Init(Ioc);
            BackgroundJobs.Init();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.BindIsAlive();
            app.BindServicesTree(Assembly.GetExecutingAssembly());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<DepositManagerApi>();
                endpoints.MapMetrics();
            });
            
            BackgroundJobs.Start();
            _bus.Start();
        }
    }
}