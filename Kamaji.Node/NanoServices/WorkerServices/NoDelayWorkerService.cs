namespace Kamaji.Node
{
    using System;
    using Kamaji.Common;
    using Kamaji.Worker;

    public sealed class NoDelayWorkerService : WorkerServiceBase
    {
        internal NoDelayWorkerService(IWorker worker, Common.Models.ScanModel model)
            : base(worker, model)
        {
        }

        protected override ITaskRunner CreateTaskRunner() => NonDelayTaskRunner.Instance;
    }
}
