namespace Kamaji.Node.Controllers
{
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using Kamaji.Worker;
    using Microsoft.AspNetCore.Mvc;
    using ReliableQueue;
    using System.Threading;
    using System.Threading.Tasks;

    public class KamajiController : ApiController
    {
        //private static readonly SemaphoreSlim _ss = new SemaphoreSlim(1);
        //private static async Task AddService(ScanModel model)
        //{
        //    await _ss.WaitAsync();
        //    try
        //    {
        //        IWorker worker = await Workers.Instance.GetWorker(model);//çncelikle preReq ve Resource initi edilsin. Sonra zaten scan verip tekrar aldığımızda init olsun kaynaklar.

        //        using (var q = Queue.Instance.StartTransaction<ScanModel>())//Kuyruğa ekliyoruz.
        //        {
        //            q.Enqueue(model);
        //            q.Commit();
        //        }
        //    }
        //    finally
        //    {
        //        _ss.Release();
        //    }

        //    //servisi başlatacağız ama önce bir factory' ye ihtiyaç var o da model den yapılacak.
        //}
        // POST api/values
        [HttpPost]
        public Task<IActionResult> AssignScan([FromBody] ScanModel model)
        {
            string msg = null;
            return this.ResultAsync(async () =>
            {
                if (model.IsModelValid())
                {
                    // await AddService(model);

                    IWorker worker = await Workers.Instance.GetWorker(model);//çncelikle preReq ve Resource initi edilsin. Sonra zaten scan verip tekrar aldığımızda init olsun kaynaklar.

                    using (var q = Queue.Instance.StartTransaction<ScanModel>())//Kuyruğa ekliyoruz.
                    {
                        q.Enqueue(model);
                        q.Commit();
                    }

                    msg = "The service has been added to the queue";
                    return AssignScanResult.ServiceStarted;
                }

                return AssignScanResult.ModelIsNotValid;

               // ReliableQueue<int> r
            }, msg);
        }

        public enum AssignScanResult// bunu kullanmak gerek. Bir de queue yapısı gereki ki bekleyen işleri sıraya soksun. Bu Kuyruk yapısı her bir node ' da ve sql lite ile sağlanacak.
        {
            ModelIsNotValid = 0,
            ServiceStarted

            //AnExceptionOccured,
            //ServiceAlreadyRunning,
            //ServiceCannotbeStarted
        }


        [HttpGet]
        public Task<IActionResult> StopService(string resourceName, string asset)
        {
            string msg = null;
            return this.ResultAsync(async () =>
            {
                ScanModel scan = new ScanModel { ResourceName = resourceName, Asset = asset };
                WorkerServiceBase worker = WorkerServiceList.Instance.Find(scan);
                if (null != worker)
                {
                    await worker.Stop();
                    return !worker.IsRunning;
                }
                else
                    msg = "The service not found.";

                return false;
            }, msg);
        }
    }
}
