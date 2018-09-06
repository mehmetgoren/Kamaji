namespace Kamaji.Controllers
{
    using ionix.Utils.Extensions;
    using ionix.Utils.Reflection;
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using Kamaji.Data;
    using Kamaji.Data.Models;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;


    public class ScanController : ApiControllerBase
    {
        public ScanController(IKamajiContext db)
            : base(db) { }



        [HttpPost]
        [RequestSizeLimit(int.MaxValue)]
        public Task<IActionResult> SaveScanPrerequisite(ScanPrerequisiteModel model)
        {
            return this.ResultAsync(async () =>
            {
                int ret = 0;
                if (model.IsModelValid())
                {
                    IScanPrerequisiteModel entity = this.Db.ModelFactory.CreateScanPrerequisiteModel();
                    entity.CopyPropertiesFrom(model);

                    await this.Db.ScanPrerequisites.Save(entity);
                    ret = 1;
                }

                return ret;
            });
        }

        [HttpGet]//Node bunu çağırıyor.
        public Task<IActionResult> GetScanPrerequisiteBy(string name)
        {
            return this.ResultAsync(async () =>
            {
                ScanPrerequisiteModel ret = null;
                if (!String.IsNullOrEmpty(name))
                {
                    IScanPrerequisiteModel entity = await this.Db.ScanPrerequisites.GetBy(name);
                    if (null != entity)
                    {
                        ret = new ScanPrerequisiteModel();
                        ret.CopyPropertiesFrom(entity);
                    }
                }

                return ret;
            });
        }



        [HttpPost]
        [RequestSizeLimit(int.MaxValue)]
        public Task<IActionResult> SaveScanResource(ScanResourceModel model)
        {
            return this.ResultAsync(async () =>
            {
                int ret = 0;
                if (model.IsModelValid())
                {
                    IScanResourceModel entity = this.Db.ModelFactory.CreateScanResourceModel();
                    entity.CopyPropertiesFrom(model);

                    if (!String.IsNullOrEmpty(model.ScanPrerequisiteName))
                        entity.ScanPrerequisiteId = await this.Db.ScanPrerequisites.GetScanPrerequisiteId(model.ScanPrerequisiteName);//a scan may not have an prerequest.

                    await this.Db.ScanResources.Save(entity);
                    ret = 1;
                }

                return ret;
            });
        }

        [HttpGet]//Node bunu çağırıyor.
        public Task<IActionResult> GetScanResourceBy(string name)
        {
            return this.ResultAsync(async () =>
            {
                ScanResourceModel ret = null;
                if (!String.IsNullOrEmpty(name))
                {
                    IScanResourceModel entity = await this.Db.ScanResources.GetBy(name);
                    if (null != entity)
                    {
                        ret = new ScanResourceModel();
                        ret.CopyPropertiesFrom(entity);

                        ret.ScanPrerequisiteName = await this.Db.ScanPrerequisites.GetNameBy(entity.ScanPrerequisiteId);
                    }
                }

                return ret;
            });
        }


        [HttpPost]
        public Task<IActionResult> SaveScan(ScanModel model)
        {
            return this.ResultAsync(async () =>
            {
                int ret = 0;
                if (model.IsModelValid())
                {
                    IScanModel entity = this.Db.ModelFactory.CreateScanModel();
                    entity.Asset = model.Asset;
                    entity.Type = model.Type.Cast<int>().Cast<ScanType>();
                    entity.CreatedDate = DateTime.Now;
                    entity.Enabled = true;
                    entity.Period = model.Period;
                    entity.State = ScanState.NotStarted;
                    entity.MaxErrorLimit = model.MaxErrorLimit;
                    entity.MaxOperationLimit = model.MaxOperationLimit;
                    entity.SaveType = model.SaveType.Cast<int>().Cast<ScanSaveType>();
                    entity.Args = model.Args;
                    entity.SaveNullResult = model.SaveNullResult;
                    entity.MaxInstance = model.MaxInstance;

                    if (!String.IsNullOrEmpty(model.ResourceName))
                        entity.ScanResourceId = await this.Db.ScanResources.GetScanResourceIdBy(model.ResourceName);

                    if (!String.IsNullOrEmpty(model.NodeAddress))
                        entity.SelectedNodeId = await this.Db.Nodes.GetIdBy(model.NodeAddress);

                    await this.Db.Scans.Save(entity);
                    ret = 1;
                }

                return ret;
            });
        }


        private async Task<int> CheckMaxInstance(IScanModel scan)
        {
            if (scan.MaxInstance > 0)
            {
                ConsoleObserver.Instance.Notify(nameof(ScanController) + "_" + nameof(CheckMaxInstance), "Max Instance exceed, first unnecessary records will be removed.", scan);
                return await this.Db.ScanInstances.TrimToSize(scan.ScanId, scan.MaxInstance);
            }
            return 0;
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

            return (allowSave, nodeId, scanResourceId, scanModel);
        }

        [HttpPost]
        public Task<IActionResult> SaveScanInstance(ScanInstanceModel model)
        {
            return this.ResultAsync(async () =>
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

                        ConsoleObserver.Instance.Notify(nameof(ScanController) + "_" + nameof(SaveScanInstance), "A scan result has been got and saved.", model);
                    }
                    else
                    {
                        ConsoleObserver.Instance.Notify(nameof(ScanController) + "_" + nameof(SaveScanInstance), "A scan result has been got but the value was null or empty.", model);
                    }
                }

                return ret;
            });
        }

        [HttpPost]
        public Task<IActionResult> SaveScanInstanceOrEditResult(ScanInstanceModel model)//edit by ScanId
        {
            return this.ResultAsync(async () =>
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

                            ConsoleObserver.Instance.Notify(nameof(ScanController) + "_" + nameof(SaveScanInstanceOrEditResult), "A scan result has been edit", model);
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

                            ConsoleObserver.Instance.Notify(nameof(ScanController) + "_" + nameof(SaveScanInstanceOrEditResult), "A scan result has been got and saved.", model);
                        }
                    }
                    else
                    {
                        ConsoleObserver.Instance.Notify(nameof(ScanController) + "_" + nameof(SaveScanInstanceOrEditResult), "A scan result has been got but the value was null or empty.", model);
                    }
                }

                return ret;
            });
        }


        private async Task<IScanModel> GetScanBy(string resourceName, string asset)
        {
            object scanResourceId = await this.Db.ScanResources.GetScanResourceIdBy(resourceName);//ScanResource.Name is indexed and unique
            if (scanResourceId == null)
                throw new InvalidOperationException($"There is no ScanResource item which's name is '{resourceName}' in the database.");

            IScanModel scan = await this.Db.Scans.GetBy(asset, scanResourceId);//Scan.Asset and Scan.ScanResourceId are indexed and unique
            if (scan == null)
                throw new InvalidOperationException($"There is no ScanResource item which's name is '{resourceName}' in the database.");

            return scan;
        }

        [HttpPost]
        public Task<IActionResult> EditScan(ScanModel model)//to change state
        {
            return this.ResultAsync(async () =>
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

                    ConsoleObserver.Instance.Notify(nameof(ScanController) + "_" + nameof(EditScan), "Scan has been edit.", model);
                }

                return ret;
            });
        }


        /// <summary>
        /// added for child scans
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns>Affected records count</returns>
        [HttpPost]
        public Task<IActionResult> AddChildScans(AddChildScansModel model)
        {
            return this.ResultAsync(async () =>
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
                            entity.CreatedDate = DateTime.Now;
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
                            catch(Exception ex)
                            {
                               await Utility.CreateLogger(nameof(ScanController), nameof(AddChildScans)).Code(14).Error(ex).SaveAsync();
                            }
                        }
                       // ret = await this.Db.Scans.BatchSave(childList);
                    }
                }

                return ret;
            });
        }


        [HttpGet]
        public Task<IActionResult> GetScanInstanceListBy(string resourceName, string asset, bool alsoGetParent, bool alsoGetChilds)
        {
            return this.ResultAsync(async () =>
            {
                IEnumerable<ScanInstanceModel> ret = null;
                if (!String.IsNullOrEmpty(resourceName) && !String.IsNullOrEmpty(asset))
                {
                    List<object> scanIdList = new List<object>();
                    IScanModel scan = await this.GetScanBy(resourceName, asset);
                    scanIdList.Add(scan.ScanId);

                    if (scan.ParentId != null && alsoGetParent)
                    {
                        scanIdList.Add(scan.ParentId);
                       // IScanModel parent = await this.Db.Scans.GetBy(scan.ParentId);
                        //if (null != parent)
                          //  scanIdList.Add(parent.ParentId);
                    }

                    if (alsoGetChilds)
                    {
                        var childList = await this.Db.Scans.GetRecursivelyChildList(scan.ScanId);
                        if (!childList.IsEmptyList())
                            scanIdList.AddRange(childList.Select(p => p.ScanId));
                    }

                    var temp = await this.Db.ScanInstances.GetListBy(scanIdList);
                    if (null != temp)
                    {
                        ret = temp.Select(i =>
                        {
                            ScanInstanceModel item = new ScanInstanceModel();
                            item.CopyPropertiesFrom(i);
                            return item;
                        });
                    }
                }

                return ret;
            });
        }
    }
}