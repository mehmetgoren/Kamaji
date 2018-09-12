namespace Kamaji
{
    using ionix.Utils.Extensions;
    using ionix.Utils.Reflection;
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using Kamaji.Data;
    using Kamaji.Data.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    

    internal class ScanWorks
    {
        public IKamajiContext Db { get; }

        internal ScanWorks(IKamajiContext db)
        {
            this.Db = db;
        }


        private async Task<(bool, object, object, IScanModel)> GetScanIds(ScanInstanceModel model)
        {
            bool allowSave = true;
            object nodeId = await this.Db.Nodes.GetIdBy(model.NodeAddress);//NodeAddress is indexed and unique.
            if (nodeId == null)
                throw new InvalidOperationException($"There is no node item for '{model.NodeAddress}' in the database.");


            object scanResourceId = await this.Db.ScanResources.GetScanResourceIdBy(model.ResourceName);//ScanResource.Name is indexed and unique
            if (scanResourceId == null)
                throw new InvalidOperationException($"There is no ScanResource item which's name is '{model.ResourceName}' in the database.");


            IScanModel scanModel = await this.Db.Scans.GetBy(model.Asset, scanResourceId);//Scan.Asset and Scan.ScanResourceId are indexed and unique
            if (scanModel == null)
                throw new InvalidOperationException($"There is no Scan item which's name is '{model.ResourceName}' in the database.");

            if (String.IsNullOrEmpty(model.FailedReason) && String.IsNullOrEmpty(model.Result))//Hem hata olmamış hem de null değer gelmiş.
            {
                allowSave = scanModel.SaveNullResult;
            }

            if (allowSave)
            {
                allowSave = scanModel.LastAssignedNodeId.Equals(nodeId);//offline gibi durumlarda atanan node' den gelmediyse kayıt etme.
                if (!allowSave)
                {
                   _=Utility.CreateLogger(nameof(ScanWorks), nameof(GetScanIds)).Code(786).Warning($"Last-Scan-Ids are not match. Orginal is {scanModel.LastAssignedNodeId} but incoming is {nodeId}").SaveAsync();
                }
            }

            return (allowSave, nodeId, scanResourceId, scanModel);
        }

        private async Task<int> CheckMaxInstance(IScanModel scan)
        {
            if (scan.MaxInstance > 0)
            {
                int affected = await this.Db.ScanInstances.TrimToSize(scan.ScanId, scan.MaxInstance);
                if (affected > 0)
                    ConsoleObserver.Instance.Notify(nameof(ScanWorks) + "_" + nameof(CheckMaxInstance), $"Max Instance exceed, total {affected} unnecessary records has been removed.", scan);
            }
            return 0;
        }

        internal async Task<int> SaveScanInstance(ScanInstanceModel model)
        {
            int ret = 0;
            if (model.IsModelValid())
            {
                IScanInstanceModel entity = this.Db.ModelFactory.CreateScanInstanceModel();
                entity.CopyPropertiesFrom(model);

                (bool allowSave, object nodeId, object scanResourceId, IScanModel scan) = await this.GetScanIds(model);

                if (allowSave)
                {
                    entity.NodeId = nodeId;
                    entity.ScanId = scan.ScanId;

                    await this.Db.ScanInstances.Save(entity);
                    await this.CheckMaxInstance(scan);
                    ret = 1;

                    ConsoleObserver.Instance.Notify(nameof(ScanWorks) + "_" + nameof(SaveScanInstance), "A scan result has been got and saved.", model);
                }
                else
                {
                    ConsoleObserver.Instance.Notify(nameof(ScanWorks) + "_" + nameof(SaveScanInstance), "A scan result has been got but the value was null or empty.", model);
                }
            }

            return ret;
        }


        internal async Task<int> SaveScanInstanceOrEditResult(ScanInstanceModel model)
        {
            int ret = 0;
            if (model.IsModelValid())
            {
                (bool allowSave, object nodeId, object scanResourceId, IScanModel scan) = await this.GetScanIds(model);

                if (allowSave)
                {
                    IScanInstanceModel entity = await this.Db.ScanInstances.GetFirstBy(scan.ScanId);
                    if (null != entity)
                    {
                        ret = await this.Db.ScanInstances.EditResult(entity.ScanId, nodeId, model.StartTime, model.EndTime, model.Result);

                        ConsoleObserver.Instance.Notify(nameof(ScanWorks) + "_" + nameof(SaveScanInstanceOrEditResult), "A scan result has been edit.", model);
                    }
                    else
                    {
                        entity = this.Db.ModelFactory.CreateScanInstanceModel();
                        entity.CopyPropertiesFrom(model);

                        entity.NodeId = nodeId;
                        entity.ScanId = scan.ScanId;

                        await this.Db.ScanInstances.Save(entity);
                        await this.CheckMaxInstance(scan);
                        ret = 1;

                        ConsoleObserver.Instance.Notify(nameof(ScanWorks) + "_" + nameof(SaveScanInstanceOrEditResult), "A scan result has been got and saved.", model);
                    }
                }
                else
                {
                    ConsoleObserver.Instance.Notify(nameof(ScanWorks) + "_" + nameof(SaveScanInstanceOrEditResult), "A scan result has been got but the value was null or empty.", model);
                }
            }

            return ret;
        }


        internal async Task<IScanModel> GetScanBy(string resourceName, string asset)
        {
            object scanResourceId = await this.Db.ScanResources.GetScanResourceIdBy(resourceName);//ScanResource.Name is indexed and unique
            if (scanResourceId == null)
                throw new InvalidOperationException($"There is no ScanResource item which's name is '{resourceName}' in the database.");

            IScanModel scan = await this.Db.Scans.GetBy(asset, scanResourceId);//Scan.Asset and Scan.ScanResourceId are indexed and unique
            if (scan == null)
                throw new InvalidOperationException($"There is no ScanResource item which's name is '{resourceName}' in the database.");

            return scan;
        }

        internal async Task<int> EditScan(ScanModel model)
        {
            int ret = 0;
            if (model.IsModelValid())
            {
                IScanModel scan = await this.GetScanBy(model.ResourceName, model.Asset);

                scan.Period = model.Period;
                scan.Type = model.Type.Cast<int>().Cast<ScanType>();
                if (model.State.HasValue)
                    scan.State = model.State.Value.Cast<int>().Cast<ScanState>();

                await this.Db.Scans.Edit(scan);

                ret = 1;

                ConsoleObserver.Instance.Notify(nameof(ScanWorks) + "_" + nameof(EditScan), "Scan has been edit.", model);
            }

            return ret;
        }


        internal async Task<int> AddChildScans(AddChildScansModel model)
        {
            int ret = 0;
            if (model.IsModelValid())
            {
                IScanModel parent = await this.GetScanBy(model.Parent.ResourceName, model.Parent.Asset);

                List<IScanModel> childList = new List<IScanModel>();
                foreach (ScanModel child in model.Childs)
                {
                    if (model.IsModelValid())
                    {
                        IScanModel entity = this.Db.ModelFactory.CreateScanModel();
                        entity.Asset = child.Asset;
                        entity.CreatedDate = await this.Db.GetDbDateTime();
                        entity.Enabled = true;
                        entity.Period = parent.Period;
                        entity.State = ScanState.NotStarted;
                        entity.MaxErrorLimit = parent.MaxErrorLimit;
                        entity.MaxOperationLimit = parent.MaxOperationLimit;

                        entity.Type = parent.Type;
                        entity.ScanResourceId = parent.ScanResourceId;
                        entity.SelectedNodeId = parent.SelectedNodeId;
                        entity.SaveType = parent.SaveType;
                        entity.ParentId = parent.ScanId;

                        childList.Add(entity);
                    }
                }
                if (childList.Any())
                {

                    foreach (IScanModel child in childList)
                    {
                        try
                        {
                            await this.Db.Scans.Save(child);
                        }
                        catch (Exception ex)
                        {
                            await Utility.CreateLogger(nameof(ScanWorks), nameof(AddChildScans)).Code(14).Error(ex).SaveAsync();
                        }
                    }
                    // ret = await this.Db.Scans.BatchSave(childList);
                }
            }

            return ret;
        }
    }
}
