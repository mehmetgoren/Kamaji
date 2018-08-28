namespace Kamaji
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using System;

    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(DataSources.Jsons.AppSettings.Config.Address)
                .UseStartup<Startup>();
                //.UseKestrel(options =>
                //{
                //    options.Limits.MaxRequestBodySize = 52428800 * 5; //50MB
                //});
    }
}
