namespace Kamaji.Node
{
    using Kamaji.Common;
    using Kamaji.Worker;

    public sealed class OnceWorkerService : WorkerServiceBase
    {
        internal OnceWorkerService(IWorker worker, Common.Models.ScanModel model) 
            : base(worker, model) { }


        public override int MaxErrorLimit { get => 1;  set { } }

        public override int MaxOperationLimit { get => 1; set { } }

        protected override ITaskRunner CreateTaskRunner() => SimpleTaskRunner.Instance;

    }
}
