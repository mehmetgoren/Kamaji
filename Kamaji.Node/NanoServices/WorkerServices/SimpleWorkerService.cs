namespace Kamaji.Node
{
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using Kamaji.Worker;

    public sealed class SimpleWorkerService : WorkerServiceBase
    {
        internal SimpleWorkerService(IWorker worker, ScanModel model) 
            : base(worker, model)
        {
        }

        protected override ITaskRunner CreateTaskRunner() => SimpleTaskRunner.Instance;
    }
}
