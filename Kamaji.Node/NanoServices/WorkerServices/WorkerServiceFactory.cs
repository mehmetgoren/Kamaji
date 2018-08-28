namespace Kamaji.Node
{
    using Kamaji.Common.Models;
    using Kamaji.Worker;
    using System;

    internal static class WorkerServiceFactory
    {
        internal static WorkerServiceBase Create(IWorker worker, ScanModel model)
        {
            switch(model.Type)
            {
                case ScanModel.ScanType.Simple:
                    return new SimpleWorkerService(worker, model);
                case ScanModel.ScanType.NoDelay:
                    return new NoDelayWorkerService(worker, model);
                case ScanModel.ScanType.Once:
                    return new OnceWorkerService(worker, model);
                default:
                    throw new NotSupportedException(model.Type.ToString());
            }
        }
    }
}
