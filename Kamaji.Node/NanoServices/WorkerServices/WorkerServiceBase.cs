namespace Kamaji.Node
{
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using ionix.Utils.Extensions;
    using WorkerResult = Kamaji.Worker.WorkerResult;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using ionix.Utils.Reflection;
    using System.Linq;


    //task runner' ı da modele göre bu oluştursun. Yalnız o tipleri henüz oluşturmadık. Ya da bir factory ile TaskRunner ve NonaBase karması yapsın.
    public abstract class WorkerServiceBase : NanoServiceBase
    {
        protected Kamaji.Worker.IWorker Worker { get; }
        protected internal ScanModel Model { get;  }

        //Burada Scan' e NanoServiceBase' in proportylerini ata.
        protected WorkerServiceBase(Kamaji.Worker.IWorker worker, ScanModel model)
            : base(false, WorkerObserver.Instance)
        {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Worker = worker ?? throw new ArgumentNullException(nameof(worker));

            this.Interval = TimeSpan.FromMilliseconds(model.Period);

            this.NotifyInfoProvider = new Func<NotifyInfo>(() => new NotifyInfo { Key = this.GetType().Name, Args = this.Model });
        }


        public override int MaxOperationLimit { get => this.Model.MaxOperationLimit; set => this.Model.MaxOperationLimit = value; }

        public override int MaxErrorLimit { get => this.Model.MaxErrorLimit; set => this.Model.MaxErrorLimit = value; }


        protected virtual Task<WorkerResult> RunWorker(ProxyObserver observer, string asset, Worker.IScanRepository repository) => this.Worker.Run(observer, this.Model.Asset, repository, null);//args' ı değiştirmek istersen override et.

        //template method pattern
        protected sealed override async Task Execute(IObserver observer, CancellationToken cancellationToken)
        {
            DateTime dbDateTime = await KamajiClient.Instance.Nodes.DbDateTime();

            ScanInstanceModel scanInstance = new ScanInstanceModel();
            scanInstance.Asset = this.Model.Asset;
            scanInstance.NodeAddress = DataSources.Jsons.AppSettings.Config.Address;
            scanInstance.ResourceName = this.Model.ResourceName;

            scanInstance.StartTime = dbDateTime;
            Stopwatch bench = Stopwatch.StartNew();
            WorkerResult result;
            try
            {
                result = await this.RunWorker(ProxyObserver.Create(observer), this.Model.Asset, new ScanRepository(this));
                //şimdi buynu gönderelim Kamaji' ye gönderelim. Bakalım onda bir karşılayacı var mı?
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch(Exception ex)
            {
                result = new WorkerResult(null, false, ex.FindRoot()?.Message);
            }
            bench.Stop();
            scanInstance.EndTime = dbDateTime.Add(bench.Elapsed);
            if (result.Success)
                scanInstance.Result = JsonConvert.SerializeObject(result.Result);
            else
                scanInstance.FailedReason = result.FailedReason;


            //sonra da result' ı kayıt edeceğiz.
            switch(this.Model.SaveType)
            {
                case ScanModel.ScanSaveType.InsertNew:
                    await KamajiClient.Instance.Scans.SaveScanInstance(scanInstance);
                    break;
                case ScanModel.ScanSaveType.Upsert:
                    await KamajiClient.Instance.Scans.SaveScanInstanceOrEditResult(scanInstance);
                    break;
                default:
                    throw new NotSupportedException(this.Model.SaveType.ToString());
            }               
        }


        protected override void OnStateChanged(ServiceState state)
        {
            ScanModel.ScanState? dbState = null;
            switch(state)
            {
                case ServiceState.Starting:
                    dbState = ScanModel.ScanState.Running;
                    break;
                case ServiceState.Stopped:
                    dbState = ScanModel.ScanState.Stopped;
                    break;
                case ServiceState.FailedDueToMaxErrorExceed:
                    dbState = ScanModel.ScanState.Failed;
                    break;
                case ServiceState.CompletedDueToMaxOperationLimit:
                    dbState = ScanModel.ScanState.Completed;
                    break;
            }
            if (null != dbState)
            {
                try
                {
                    this.Model.State = dbState.Value;
                    KamajiClient.Instance.Scans.EditScan(this.Model);
                }
                catch (Exception ex)
                {
                    Utility.CreateLogger(this.GetType().Name, nameof(OnStateChanged)).Code(18).Error(ex).SaveAsync();
                }
            }
        }

        /// <summary>
        /// this is a proxy type. you can pass a WebSocket Observer
        /// </summary>
        protected sealed class ProxyObserver : Kamaji.Worker.IObserver
        {
            public static ProxyObserver Create(Kamaji.Common.IObserver concrete) => new ProxyObserver(concrete);

            private Kamaji.Common.IObserver Concrete { get; }

            internal ProxyObserver(IObserver concrete)
            {
                this.Concrete = concrete;
            }

            public void Notify(string id, string message, object args) => this.Concrete.Notify(id, message, args);
        }

        protected sealed class ScanRepository : Worker.IScanRepository
        {
            private WorkerServiceBase Parent { get; }

            internal ScanRepository(WorkerServiceBase parent)
            {
                this.Parent = parent;
            }


            public async Task AddChildScan(string parentAsset, IEnumerable<string> childAssetList)
            {
                if (!this.Parent.IsRunning)//Stop olduysa durdurulsun
                    return;

                List<ScanModel> childList = new List<ScanModel>();
                foreach (string childAsset in childAssetList)
                {
                    if (!String.IsNullOrEmpty(childAsset) && childAsset != parentAsset)//checking parent to prevent duplication.
                    {
                        ScanModel child = new ScanModel();
                        child.CopyPropertiesFrom(this.Parent.Model);
                        child.Asset = childAsset;//Kamaji sadece bu field' e bakıyor.
                                                 // yeni asset atanmıyor.

                        childList.Add(child);
                    }
                }

               await KamajiClient.Instance.Scans.AddChildScans(new AddChildScansModel { Parent = this.Parent.Model, Childs = childList });
            }


            public async Task<IEnumerable<string>> GetResults(bool alsoGetParentResult, bool alsoGetChildsResults)
            {
                List<string> ret = new List<string>();

                if (!this.Parent.IsRunning)//Stop olduysa durdurulsun
                    return ret;

                var results =  await KamajiClient.Instance.Scans.GetScanInstanceListBy(this.Parent.Model.ResourceName, this.Parent.Model.Asset, alsoGetParentResult, alsoGetChildsResults);
                if (!results.IsEmptyList())
                {
                    ret.AddRange(results.Select(p => p.Result));
                }

                return ret;
            }
        }
    }
}
