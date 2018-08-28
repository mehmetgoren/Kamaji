namespace Kamaji.Node
{
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using ReliableQueue;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class QueueService : NanoServiceBase//singleton
    {
        public static readonly QueueService Instance = new QueueService();
        private QueueService()
            :base(true, ConsoleObserver.Instance, TimeSpan.FromMilliseconds(2000))//kamaji den bunu config bilgisi alarak yap.
        {

        }

        protected override ITaskRunner CreateTaskRunner() => SimpleTaskRunner.Instance;

        protected override async Task Execute(IObserver observer, CancellationToken cancellationToken)
        {
            ScanModel scan = null;
            using (var tran = Queue.Instance.StartTransaction<ScanModel>())
            {
                scan = tran.Dequeue();
                // observer.Notify("QueueService", "Kuyruktan scan seçildi.", null);

                if (null != scan)//Yani kuyrukta veri varsa.
                {
                    Kamaji.Worker.IWorker worker = await Workers.Instance.GetWorker(scan);//öncelikle preReq ve Resource initi edilsin. Sonra zaten scan verip tekrar aldığımızda init olsun kaynaklar.
                    observer.Notify("QueueService", "Worker Alındı", null);

                    WorkerServiceBase service = WorkerServiceList.Instance.Find(scan);
                    if (null == service)
                    {
                        service = WorkerServiceFactory.Create(worker, scan);
                        WorkerServiceList.Instance.Add(service);
                        observer.Notify("QueueService", "Servis ilk defa oluşturuldu", null);
                    }
                    if (service.IsRunning)
                    {
                        tran.Enqueue(scan);//running ise tekrardan kuyruğa ekle.
                        observer.Notify("QueueService", "Servis Runnig olduğu için tekrara kuyruğa eklendi. ", null);
                    }
                    else
                    {
                       _=service.Start();//başlat bir an önceki commit yapabilelim. like Python.
                    }
                }
                //else
                   // observer.Notify("QueueService", "Kuyrukta Scan yok.", null);

                tran.Commit();
            }
        }
    }
}
