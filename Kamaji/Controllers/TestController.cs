namespace Kamaji.Controllers
{
    using ionix.Utils.Extensions;
    using ionix.Utils.Reflection;
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using Kamaji.Data;
    using Kamaji.Data.Models;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [DoNotAuthorize]
    public class TestController : ApiControllerBase
    {
        public TestController(IKamajiContext db)
            : base(db) { }

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


        //http://192.168.0.21:1000/api/Test/GetResult?resourceName=nmap_windows&asset=-PS 127.0.0.1
        [HttpGet]
        public async Task<IActionResult> GetResult(string resourceName, string asset)
        {
            object ret = null;
            if (!String.IsNullOrEmpty(resourceName) && !String.IsNullOrEmpty(asset))
            {
                List<object> scanIdList = new List<object>();
                IScanModel scan = await this.GetScanBy(resourceName, asset);
                scanIdList.Add(scan.ScanId);

                ret = await this.Db.ScanInstances.GetListBy(new[] { scan.ScanId });
            }
            return Json(ret);
        }

        //http://192.168.0.21:1000/api/Test/Spider?resourceName=WebPageSpider&asset=http://toastytech.com/evil/
        [HttpGet]
        public async Task<IActionResult> Spider(string resourceName, string asset)
        {
            HashSet<string> ret = new HashSet<string>();
            if (!String.IsNullOrEmpty(resourceName) && !String.IsNullOrEmpty(asset))
            {
                List<object> scanIdList = new List<object>();
                IScanModel scan = await this.GetScanBy(resourceName, asset);
                scanIdList.Add(scan.ScanId);

                if (scan.ParentId != null)
                {
                    scanIdList.Add(scan.ParentId);
                    // IScanModel parent = await this.Db.Scans.GetBy(scan.ParentId);
                    //if (null != parent)
                    //  scanIdList.Add(parent.ParentId);
                }

                var childList = await this.Db.Scans.GetRecursivelyChildList(scan.ScanId);
                if (!childList.IsEmptyList())
                    scanIdList.AddRange(childList.Select(p => p.ScanId));


                var temp = await this.Db.ScanInstances.GetListBy(scanIdList);
                if (null != temp)
                {
                    foreach (IScanInstanceModel scanInstance in temp)
                    {
                        if (scanInstance.Result != null)
                        {
                            var links = JsonConvert.DeserializeObject<List<string>>(scanInstance.Result);
                            ret.AddRange(links);
                        }
                    }
                }
            }



            const string host = "http://toastytech.com";
            HashSet<string> spiderLinks = new HashSet<string>(System.IO.File.ReadAllLines("g:\\LinksSpider.csv").Select(i => host + i));

            List<string> kamaji = new List<string>();
            foreach (string link in ret)
            {
                if (!spiderLinks.Contains(link))
                    kamaji.Add(link);
            }

            List<string> spiders = new List<string>();
            foreach (string link in spiderLinks)
            {
                if (!ret.Contains(link))
                    spiders.Add(link);
            }

            System.IO.File.WriteAllText("g:\\LinksKamaji.json", JsonConvert.SerializeObject(ret));


            var result = new { count = ret.Count, links = ret, kamaji, spiders };


            //System.IO.File.WriteAllText("g:\\result.json", JsonConvert.SerializeObject(result));

            return Json(result);
        }


        //http://192.168.0.21:1000/api/Test/StopService?resourceName=WebPageHtmlSource&asset=https://www.w3.org/
        [HttpGet]
        public async Task<IActionResult> StopService(string resourceName, string asset)
        {
            var node = await this.Db.Nodes.GetBy("http://192.168.0.21:1001");
            return Json(await NodesClient.Instance.StopService(node, resourceName, asset));
        }
    }
}
