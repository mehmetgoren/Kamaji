namespace Kamaji.Node
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Connections;
    using Microsoft.AspNetCore.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
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

                //Run(args);
                CreateWebHostBuilder(args).Build().Run();
            }
        }

        private static void Run(string[] args)//access denied for process.
        {
            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch(IOException ex)//should be tested on linux.
            {
                Process me = Process.GetCurrentProcess();
                if (ex.InnerException is AddressInUseException aaex)
                {
                    var processList = TaskManagerInfo.GetTaskManegerInfos().Where(p => p.PortNumber == DataSources.Jsons.AppSettings.Config.Address.Split(':')[2] && p.PID != me.Id);
                    foreach (var process in processList)
                    {
                       var temp = Process.GetProcessById(process.PID);
                        try
                        {
                            temp?.Kill();
                        }
                        catch { }
                    }

                    Run(args);
                }
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(DataSources.Jsons.AppSettings.Config.Address)
                .UseStartup<Startup>();
    }


    internal sealed class TaskManagerInfo
    {
        internal static List<TaskManagerInfo> GetTaskManegerInfos()
        {
            var ports = new List<TaskManagerInfo>();

            try
            {
                using (Process p = new Process())
                {
                    ProcessStartInfo ps = new ProcessStartInfo();
                    ps.Arguments = "-a -n -o";
                    ps.FileName = "netstat.exe";
                    ps.UseShellExecute = false;
                    ps.WindowStyle = ProcessWindowStyle.Hidden;
                    ps.RedirectStandardInput = true;
                    ps.RedirectStandardOutput = true;
                    ps.RedirectStandardError = true;

                    p.StartInfo = ps;
                    p.Start();

                    StreamReader stdOutput = p.StandardOutput;
                    StreamReader stdError = p.StandardError;

                    string content = stdOutput.ReadToEnd() + stdError.ReadToEnd();
                    string exitStatus = p.ExitCode.ToString();

                    if (exitStatus != "0")
                    {
                        // Command Errored. Handle Here If Need Be
                    }

                    //Get The Rows
                    string[] rows = Regex.Split(content, "\r\n");
                    foreach (string row in rows)
                    {
                        //Split it baby
                        string[] tokens = Regex.Split(row, "\\s+");
                        if (tokens.Length > 4 && (tokens[1].Equals("UDP") || tokens[1].Equals("TCP")))
                        {
                            string localAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");
                            ports.Add(new TaskManagerInfo
                            {
                                PID = tokens[1] == "UDP" ? Convert.ToInt16(tokens[4]) : Convert.ToInt16(tokens[5]),
                                Protocol = localAddress.Contains("1.1.1.1") ? String.Format("{0}v6", tokens[1]) : String.Format("{0}v4", tokens[1]),
                                PortNumber = localAddress.Split(':')[1],
                                ProcessName = tokens[1] == "UDP" ? LookupProcess(Convert.ToInt16(tokens[4])) : LookupProcess(Convert.ToInt16(tokens[5]))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ports;
        }
        private static string LookupProcess(int pid)
        {
            string procName;
            try { procName = Process.GetProcessById(pid).ProcessName; }
            catch (Exception) { procName = "-"; }

            return procName;
        }



        public short PID { get; set; }

        public string Name => $"PID: '{this.PID}', Process Name: '{this.ProcessName}' ({this.Protocol} port {this.PortNumber})";
        public string PortNumber { get; set; }
        public string ProcessName { get; set; }
        public string Protocol { get; set; }

        public override string ToString() => this.Name;
    }
}
