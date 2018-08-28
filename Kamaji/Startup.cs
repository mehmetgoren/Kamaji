namespace Kamaji
{
    using Kamaji.Data;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //Buraya scan leri seçip dağıtma gelecek.
            //Node' a da duplex comminication için bir servis yaz.
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            InitKamajiRepository(services);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuth();
            app.UseMvc();
        }

        private void InitKamajiRepository(IServiceCollection services)
        {
            AssemblyResolver ar = new AssemblyResolver(DataSources.Jsons.AppSettings.Config.KamajiDataPath);
            var db = ar.Resolve<IKamajiContext>();
            db.Init(new ConnectionStringProvider());

            services.AddScoped(typeof(IKamajiContext), db.GetType());
            DI.ServiceCollection.AddScoped(typeof(IKamajiContext), db.GetType());
        }
    }
}
