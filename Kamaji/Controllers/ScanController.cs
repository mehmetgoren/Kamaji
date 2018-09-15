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
                    entity.CreatedDate = await this.Db.GetDbDateTime();
                    entity.LastModifiedDate = entity.CreatedDate;

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
                    entity.CreatedDate = await this.Db.GetDbDateTime();
                    entity.LastModifiedDate = entity.CreatedDate;

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
                    entity.CreatedDate = await this.Db.GetDbDateTime();
                    entity.LastModifiedDate = entity.CreatedDate;
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




        [HttpPost]
        public Task<IActionResult> SaveScanInstance(ScanInstanceModel model) 
            => this.ResultAsync(() => new ScanWorks(this.Db).SaveScanInstance(model));

        [HttpPost]
        public Task<IActionResult> SaveScanInstanceOrEditResult(ScanInstanceModel model) 
            => this.ResultAsync(() => new ScanWorks(this.Db).SaveScanInstanceOrEditResult(model)); //edit by ScanId




        [HttpPost]
        public Task<IActionResult> EditScan(ScanModel model)//to change state
            => this.ResultAsync(() => new ScanWorks(this.Db).EditScan(model)); //edit by ScanId


        /// <summary>
        /// added for child scans
        /// </summary>
        /// <param name="modelList"></param>
        /// <returns>Affected records count</returns>
        [HttpPost]
        public Task<IActionResult> AddChildScans(AddChildScansModel model)
            => this.ResultAsync(() => new ScanWorks(this.Db).AddChildScans(model));


        [HttpGet]
        public Task<IActionResult> GetScanInstanceListBy(string resourceName, string asset, bool alsoGetParent, bool alsoGetChilds)
        {
            return this.ResultAsync(async () =>
            {
                IEnumerable<ScanInstanceModel> ret = null;
                if (!String.IsNullOrEmpty(resourceName) && !String.IsNullOrEmpty(asset))
                {
                    List<object> scanIdList = new List<object>();
                    IScanModel scan = await new ScanWorks(this.Db).GetScanBy(resourceName, asset);
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