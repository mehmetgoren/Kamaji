﻿namespace Kamaji
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

    internal abstract class ScanQueueServiceBase : NanoServiceBase
    {
        protected ScanQueueServiceBase()
            : base(true, ConsoleObserver.Instance, TimeSpan.FromSeconds(1))
        {
            this.MaxErrorLimit = 0;
        }

        protected sealed override ITaskRunner CreateTaskRunner() => SimpleTaskRunner.Instance;


        protected abstract Task<IEnumerable<IScanModel>> GetScanList(IKamajiContext db);

        protected sealed override async Task Execute(IObserver observer, CancellationToken cancellationToken)
        {
            using (IKamajiContext db = DI.Provider.GetService<IKamajiContext>())
            {
                IEnumerable<IScanModel> scanList = await this.GetScanList(db);
                if (null != scanList && scanList.Any())
                {
                    scanList = scanList.OrderBy(p => p.CreatedDate);
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
                                    scan.State = ScanState.AssignFailed;
                                    await OnAssingFailed(db, scan);
                                    observer.Notify("ScanQueueService.Execute", $"Warning!!!!. A {scan.Asset} of {scanResource.Name} job couldn't assign to a node which name  is '{node.Address}'.", null);
                                }
                            }
                        }
                        else
                        {
                            // scan.Enabled = false;//Burası biraz sıkıntılı
                            scan.State = ScanState.AssignFailed;
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

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        private static async Task<INodeModel> ChoseNode(IKamajiContext db, IScanModel scan)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                INodeModel ret = null;
                if (scan.SelectedNodeId != null)
                {
                    ret = await db.Nodes.GetBy(scan.SelectedNodeId);
                }
                else
                {
                    ret = await GetOptimumNode(db);
                }

                return ret;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private static readonly int nodeTimeout = DataSources.Jsons.AppSettings.Config.Nodes.Timeout;
        private static async Task<INodeModel> GetOptimumNode(IKamajiContext db)
        {
            IEnumerable<INodeModel> nodes = await db.Nodes.GetAll();//eğer 5000 node bağlanırsa bunu değiştir.
            if (null != nodes && nodes.Any())
            {
                double CalculateAvailabilityScore(INodeModel node)
                {
                    double availableMem = 1.0 - node.MemoryUsage;
                    double availebleCpu = 1.0 - node.CpuUsage;
                    double threatCount = node.ThreadCount;

                    return availebleCpu * threatCount * availableMem / Math.Max(node.TotalExecutingJobCount + node.TotalQueuedJobCount, 1.0);
                }

                DateTime dbDate = await db.GetDbDateTime();
                nodes = nodes.Where(p => p.LastConnectionTime.AddSeconds(nodeTimeout) > dbDate);//get all nodes that lives. Bu 10 saniyeyi config' e taşı.
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
