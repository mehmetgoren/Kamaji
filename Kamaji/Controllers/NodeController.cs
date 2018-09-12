namespace Kamaji.Controllers
{
    using System;
    using System.Threading.Tasks;
    using ionix.Utils.Reflection;
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using Kamaji.Data;
    using Kamaji.Data.Models;
    using Microsoft.AspNetCore.Mvc;

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
        public IActionResult DbDateTime()
        {
            return this.Result(this.Db.GetDbDateTime);
        }
    }
}
