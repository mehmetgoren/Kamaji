namespace Kamaji.Node
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using ionix.Utils.Extensions;
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using ReliableQueue;

    partial class KamajiClient
    {
        public Node Nodes { get; }

        public sealed class Node
        {
            private static class KamajiNodeActions
            {
                internal const string Register = "api/Node/" + nameof(Register);
                internal const string HeartBeat = "api/Node/" + nameof(HeartBeat);
                internal const string DbDateTime = "api/Node/" + nameof(DbDateTime);
            }


            public async Task<bool?> Register()
            {
                var logger = Utility.CreateLogger(nameof(KamajiClient), nameof(Register));
                try
                {
                    NodeRegisterModel model = new NodeRegisterModel();
                    model.Address = DataSources.Jsons.AppSettings.Config.Address;

                    IResourceMonitor resourceMonitor = SystemInfo.CreateResourceMonitor();
                    model.ThreadCount = SystemInfo.ProcessorCount;
                    model.TotalMemory = resourceMonitor.GetTotalMemory().ConvertTo<int>();
                    model.ComputerName = SystemInfo.ComputerName();
                    model.IpAddress = SystemInfo.GetIpv4Address()?.ToString();

                    string[] arr = model.Address.Split(':');
                    if (arr.Length > 1 && int.TryParse(arr[arr.Length - 1], out int port))
                        model.Port = port;

                    bool success = await RestClient.Instance.PostAsync<bool>($"{KamajiNodeActions.Register}", model);

                    await logger.Code(1).Info("A token has been taken and conected to Kamaji").SaveAsync();

                    return success;
                }
                catch (Exception ex)
                {
                    await logger.Code(1).Error(ex).SaveAsync();
                    return null;
                }
            }


            public async Task HeartBeat()
            {
                NodeHeartBeatModel model = new NodeHeartBeatModel();
                model.Address = DataSources.Jsons.AppSettings.Config.Address;

                IResourceMonitor resourceMonitor = SystemInfo.CreateResourceMonitor();
                model.CpuUsage = resourceMonitor.GetCpuUsage();
                model.MemoryUsage = resourceMonitor.GetMemoryUsage();

                model.TotalExecutingJobCount = WorkerServiceList.Instance.Count(p => p.IsRunning);  //Workers.Instance.TotalWorkerCount;//şu an için. Node' ların wırker execute yapısı ortaya çıkınca burasıu güncellenecek.
                model.TotalQueuedJobCount = Queue.Instance.Count();

                await RestClient.Instance.PostAsync<bool>($"{KamajiNodeActions.HeartBeat}", model);
            }


            public Task<DateTime> DbDateTime()
            {
                return RestClient.Instance.GetAsync<DateTime>($"{KamajiNodeActions.DbDateTime}");
            }
        }
    }
}
