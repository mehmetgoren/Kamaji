namespace Kamaji.Controllers
{
    using System;
    using System.Threading.Tasks;
    using ionix.Utils.Extensions;
    using ionix.Utils.Reflection;
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using Kamaji.Data;
    using Kamaji.Data.Models;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    public sealed class NodeController : ApiControllerBase
    {
        public NodeController(IKamajiContext db)
            : base(db) { }


        private static bool _hasAny;
        private static void StartNanoServices()
        {
            Task.Run(async () =>
            {
                var logger = Utility.CreateLogger(nameof(NodeController), nameof(StartNanoServices)).Code(111);
                await logger.Info("ScanQueueService is starting...").SaveAsync();
                await ScanQueueService.Instance.Start();
                await logger.Info("ScanQueueService has been stopped...").SaveAsync();
            });
        }

        //Buralarda IObserver Gerekli ki web arayüzünde gerçek zamanlı gözüksün.
        [HttpPost]
        public Task<IActionResult> Register([FromBody] NodeRegisterModel node)
        {
            return this.ResultAsync(async () =>
            {
                bool ret = false;
                if (node.IsModelValid())
                {
                    INodeModel model = null;
                    if (!String.IsNullOrEmpty(node.Address))
                    {
                        model = await this.Db.Nodes.GetBy(node.Address);
                    }

                    if (null == model)
                    {
                        model = this.Db.ModelFactory.CreateNodeModel();
                    }

                    model.CopyPropertiesFrom(node);

                    model.ConnectionStatus = NodeConnectionStatus.Online;
                    model.LastConnectionTime = DateTime.Now;

                    model.CpuUsage = 0;
                    model.MemoryUsage = 0;
                    model.TotalBeatsCount = 0;

                    model = await this.Db.Nodes.Save(model);

                    NodeResourceUsages.Instance.Get(model.Address)?.Reset();

                    _=this.HandleOfflineData(node.Address);

                    ret = true;

                    if (!_hasAny)
                    {
                        _hasAny = true;
                        StartNanoServices();
                    }
                }

                return ret;
            });
        }

        private async Task HandleOfflineData(string nodeAddress)
        {
            ScanWorks scan = new ScanWorks(this.Db);
            var offlineDataList = await NodesClient.Instance.GetOfflineData(nodeAddress);
            if (!offlineDataList.IsEmptyList())
            {
                foreach (var offline in offlineDataList)
                {
                    if (!String.IsNullOrEmpty(offline.Operation) && !String.IsNullOrEmpty(offline.Json))
                    {
                        try
                        {
                            switch (offline.Operation)
                            {
                                case "SaveScanInstance":
                                    ScanInstanceModel sim = JsonConvert.DeserializeObject<ScanInstanceModel>(offline.Json);
                                    await scan.SaveScanInstance(sim);
                                    break;
                                case "SaveScanInstanceOrEditResult":
                                    ScanInstanceModel sim2 = JsonConvert.DeserializeObject<ScanInstanceModel>(offline.Json);
                                    await scan.SaveScanInstanceOrEditResult(sim2);
                                    break;
                                case "EditScan"://Burası kuyruğa eklendiği için iptal edildçi.
                                    //ScanModel sm = JsonConvert.DeserializeObject<ScanModel>(offline.Json);
                                    //  await scan.EditScan(sm);
                                    break;
                                case "AddChildScans":
                                    AddChildScansModel asm = JsonConvert.DeserializeObject<AddChildScansModel>(offline.Json);
                                    await scan.AddChildScans(asm);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            var logger = Utility.CreateLogger(nameof(HandleOfflineData), nameof(HandleOfflineData)).Code(468);
                            await logger.Error(ex.FindRoot() ?? ex).SaveAsync();
                            ConsoleObserver.Instance.Notify(nameof(HandleOfflineData), nameof(HandleOfflineData), ex);
                        }
                    }

                    try
                    {
                        if (offline.Id > 0)
                            await NodesClient.Instance.DeleteOfflineData(nodeAddress, offline.Id);
                    }
                    catch (Exception ex)
                    {
                        var logger = Utility.CreateLogger(nameof(HandleOfflineData), nameof(HandleOfflineData)).Code(467);
                        await logger.Error(ex.FindRoot() ?? ex).SaveAsync();
                        ConsoleObserver.Instance.Notify(nameof(HandleOfflineData), nameof(HandleOfflineData), ex);

                    }
                }
            }
        }



        //Buralarda IObserver Gerekli ki web arayüzünde gerçek zamanlı gözüksün.
        [HttpPost]
        public Task<IActionResult> HeartBeat(NodeHeartBeatModel model)
        {
            return this.ResultAsync(async () =>
            {
                bool ret = false;
                if (model.IsModelValid())
                {
                    INodeModel node = null;
                    if (!String.IsNullOrEmpty(model.Address))
                    {
                        node = await this.Db.Nodes.GetBy(model.Address);
                        if (null != node)
                        {
                            node.CopyPropertiesFrom(model);
                            node.LastConnectionTime = DateTime.Now;

                            var usage = NodeResourceUsages.Instance.Get(node.Address);
                            usage.Add(node.CpuUsage, node.MemoryUsage);

                            node.CpuUsage = usage.CpuUsage;
                            node.MemoryUsage = usage.MemoryUsage;
                            node.TotalBeatsCount = usage.TotalBeatsCount;

                            await this.Db.Nodes.Save(node);

                            ret = true;
                        }
                    }
                }

                return ret;
            });
        }

        [HttpGet]
        public Task<IActionResult> DbDateTime()
        {
            return this.ResultAsync(this.Db.GetDbDateTime);
        }
    }
}
