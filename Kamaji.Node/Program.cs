namespace Kamaji.Node
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Guid? token = await KamajiClient.Instance.TakeAToken();// Node needs to connec to Kamaji first.

            if (null == token)
            {
                Console.WriteLine("Couldn't connect to Kamaji.");
                Console.ReadKey();
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                RestClient.AuthToken = token.Value;
                await KamajiClient.Instance.Nodes.Register();

                CreateWebHostBuilder(args).Build().Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(DataSources.Jsons.AppSettings.Config.Address)
                .UseStartup<Startup>();
    }
}
