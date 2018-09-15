namespace Kamaji
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Kamaji.Data;
    using Kamaji.Data.Models;

    public sealed class OnDemandScanQueueService : ScanQueueServiceBase
    {
        public static readonly OnDemandScanQueueService Instance = new OnDemandScanQueueService();
        private OnDemandScanQueueService() { }

        protected override Task<IEnumerable<IScanModel>> GetScanList(IKamajiContext db)
            => db.Scans.GetOnDemandListBy(true, ScanState.NotStarted, ScanState.NodeShutdown, ScanState.AssignFailed);//Buraya parantid null olan da eklenebilir child lar otomatik başlıyorsa.
    }
}
