namespace Kamaji
{
    using Kamaji.Common;
    using Kamaji.Data;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using Kamaji.Data.Models;
    using System.Linq;
    using ionix.Utils.Extensions;

    internal sealed class BrokenNodeService : NanoServiceBase
    {
        public static BrokenNodeService Instance = new BrokenNodeService();
        private BrokenNodeService()
            : base(true, ConsoleObserver.Instance, TimeSpan.FromSeconds(1))
        {
            this.MaxErrorLimit = 0;
        }

        protected override ITaskRunner CreateTaskRunner() => SimpleTaskRunner.Instance;


        private static readonly int nodeTimeout = DataSources.Jsons.AppSettings.Config.Nodes.Timeout;
        protected override async Task Execute(IObserver observer, CancellationToken cancellationToken)
        {
            using (IKamajiContext db = DI.Provider.GetService<IKamajiContext>())
            {
                IEnumerable<INodeModel> brokenNodes = await db.Nodes.GetAll();//eğer 5000 node bağlanırsa bunu değiştir.
                if (!brokenNodes.IsEmptyList())
                {
                    DateTime dbDate = await db.GetDbDateTime();
                    brokenNodes = brokenNodes.Where(p => p.LastConnectionTime.AddSeconds(nodeTimeout) < dbDate);//get all nodes that lives. Bu 10 saniyeyi config' e taşı.
                    if (brokenNodes.Any())
                    {
                        foreach (INodeModel node in brokenNodes)
                        {
                            IEnumerable<IScanModel> scans = await db.Scans.GetListByLastAssignedNodeId(true, node.NodeId);
                            if (!scans.IsEmptyList())
                            {
                                foreach (IScanModel scan in scans)
                                {
                                    scan.State = ScanState.NodeShutdown;
                                    await db.Scans.Edit(scan);
                                    string msg = $"Warning!!!.A scan({scan.Asset}) has been assigned to Shutdown due to it's assigned node has been broken.";
                                    observer?.Notify($"{nameof(BrokenNodeService)}Execute", msg, null);
                                    _=Utility.CreateLogger(nameof(BrokenNodeService), nameof(Execute)).Code(648).Warning(msg).SaveAsync();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
