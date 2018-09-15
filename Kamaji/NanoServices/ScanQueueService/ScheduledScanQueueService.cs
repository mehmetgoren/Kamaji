namespace Kamaji
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Kamaji.Data;
    using Kamaji.Data.Models;

    internal sealed class ScheduledScanQueueService : ScanQueueServiceBase
    {
        public static readonly ScheduledScanQueueService Instance = new ScheduledScanQueueService();
        private ScheduledScanQueueService() { }

        protected async override Task<IEnumerable<IScanModel>> GetScanList(IKamajiContext db)
        {
            DateTime dbDateTimeNow = await db.GetDbDateTime();
            return await db.Scans.GetScheduledListBy(true, dbDateTimeNow, ScanState.NotStarted, ScanState.NodeShutdown, ScanState.AssignFailed);//Buraya parantid null olan da eklenebilir child lar otomatik başlıyorsa.
        }
    }
}
