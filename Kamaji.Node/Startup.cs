namespace Kamaji.Node
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopping.Register(() =>
            {
                try
                {
                    foreach (var service in WorkerServiceList.Instance)
                    {
                        if (service.IsRunning)
                        {
                            var model = service.Model;
                            model.State = Common.Models.ScanModel.ScanState.Cancelled;

                            KamajiClient.Instance.Scans.EditScan(model).Wait();
                        }
                    }
                }
                catch(Exception ex)
                {
                     Common.Utility.CreateLogger(nameof(Startup), "ApplicationStopping").Code(512).Error(ex).SaveAsync().Wait();
                }

                try
                {
                    Workers.Instance.Dispose();
                }
                catch (Exception ex)
                {
                    Common.Utility.CreateLogger(nameof(Startup), "ApplicationStopping").Code(513).Error(ex).SaveAsync().Wait();
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuth();
            app.UseMvc();

            StartNanoServices();
        }

        private static void StartNanoServices()
        {
            Task.Run(async() =>
            {
                var logger = Common.Utility.CreateLogger(nameof(Startup), nameof(StartNanoServices)).Code(111);
                await logger.Info("NodeHeartBeatService is starting...").SaveAsync();
                await NodeHeartBeatService.Instance.Start();
                await logger.Info("NodeHeartBeatService has been stopped...").SaveAsync();
            });


            Task.Run(async () =>
            {
                var logger = Common.Utility.CreateLogger(nameof(Startup), nameof(StartNanoServices)).Code(111);
                await logger.Info("QueueService is starting...").SaveAsync();
                await QueueService.Instance.Start();
                await logger.Info("QueueService has been stopped...").SaveAsync();
            });

        }
    }
}
