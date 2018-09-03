namespace Kamaji
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ionix.Utils.Extensions;
    using Kamaji.Common;
    using Kamaji.Data;
    using Kamaji.Data.Models;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class ScanQueueService : NanoServiceBase
    {
        public static readonly ScanQueueService Instance = new ScanQueueService();
        private ScanQueueService()
            : base(true,ConsoleObserver.Instance, TimeSpan.FromSeconds(1))
        {

        }

        protected override ITaskRunner CreateTaskRunner() => SimpleTaskRunner.Instance;


        protected override async Task Execute(IObserver observer, CancellationToken cancellationToken)
        {
            using (IKamajiContext db = DI.Provider.GetService<IKamajiContext>())
            {
                IEnumerable<IScanModel> scanList = await db.Scans.GetListBy(true, ScanState.NotStarted, ScanState.Cancelled);//Buraya parantid null olan da eklenebilir child lar otomatik başlıyorsa
                if (null != scanList && scanList.Any())
                {
                    foreach (IScanModel scan in scanList)
                    {
                        INodeModel node = await ChoseNode(db, scan);
                        if (null != node)//eğer node bağlandıysa.
                        {
                            IScanResourceModel scanResource = await db.ScanResources.GetBy(scan.ScanResourceId);
                            if (null != scanResource)
                            {
                                string prerequisiteName = await db.ScanPrerequisites.GetNameBy(scanResource.ScanPrerequisiteId);

                                bool result = (await NodesClient.Instance.AssignScan(node, prerequisiteName, scanResource.Name, scan)) == 1;

                                if (result)
                                {
                                    scan.State = ScanState.Assigned;
                                    scan.LastAssignedNodeId = node.NodeId;
                                    await db.Scans.Edit(scan);
                                    observer.Notify("ScanQueueService.Execute", $"a {scan.Asset} of {scanResource.Name} job has been assign to a node which name  is '{node.Address}'.", null);
                                }
                                else
                                {
                                    await OnAssingFailed(db, scan);
                                    observer.Notify("ScanQueueService.Execute", $"Warning!!!!. A {scan.Asset} of {scanResource.Name} job couldn't assign to a node which name  is '{node.Address}'.", null);
                                }
                            }
                        }
                        else
                        {
                            scan.Enabled = false;
                            await OnAssingFailed(db, scan);
                            observer.Notify("ScanQueueService.Execute", $"Warning!!!!... Assigning has been failed. The scan asset: {scan.Asset}.", null);
                        }
                    }
                }
            }
        }

        private static async Task OnAssingFailed(IKamajiContext db, IScanModel scan)
        {
            scan.State = ScanState.AssignFailed;
            await db.Scans.Edit(scan);
        }

        private static async Task<INodeModel> ChoseNode(IKamajiContext db, IScanModel scan)
        {
            INodeModel ret = null;
            if (null != scan)
            {
                if (scan.SelectedNodeId != null)
                {
                    ret = await db.Nodes.GetBy(scan.SelectedNodeId);
                }
                else
                {
                    ret = await GetOptimumNode(db);
                }
            }

            return ret;
        }

        private static async Task<INodeModel> GetOptimumNode(IKamajiContext db)
        {
            IEnumerable<INodeModel> nodes = await db.Nodes.GetAll();
            if (null != nodes && nodes.Any())
            {
                double CalculateAvailabilityScore(INodeModel node)
                {
                    double availableMem = 1.0 - node.MemoryUsage;
                    double availebleCpu = 1.0 - node.CpuUsage;
                    double threatCount = node.ThreadCount;

                    return availebleCpu * threatCount * availableMem / Math.Max(node.TotalExecutingJobCount + node.TotalQueuedJobCount, 1.0);
                }

                DateTime dbDate = db.GetDbDateTime();
                nodes = nodes.Where(p => p.LastConnectionTime.AddSeconds(10) > dbDate);//get all nodes that lives. Bu 10 saniyeyi config' e taşı.
                //direk node' lara söylememiz lazım kaç adet task ları olduğunu. hem böylece multi-task scan' leri de daha iyi yakalayabiliriz.
                if (nodes.Any())
                {
                    double maxScore = double.MinValue;
                    INodeModel selected = null;
                    nodes.ForEach(node =>
                    {
                        double score = CalculateAvailabilityScore(node);
                        if (maxScore < score)
                        {
                            selected = node;
                            maxScore = score;
                        }
                    });
                    return selected;
                }
            }

            return null;
        }
    }
}
