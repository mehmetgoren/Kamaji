namespace Kamaji.Worker
{
    using System;
    using System.Threading.Tasks;

    public abstract class WorkerBase : IWorker
    {
        protected abstract Task<object> Run_Internal(IObserver observer, string asset, IScanRepository repository, object args);
        public async Task<WorkerResult> Run(IObserver observer, string asset, IScanRepository repository, object args)
        {
            try
            {
                object result = await Run_Internal(observer, asset, repository, args);
                return new WorkerResult(result);
            }
            catch (Exception ex)
            {
                return new WorkerResult(null, false, ex.Message);
            }
        }


        public abstract Task SetupEnvironment();
        public abstract void Dispose();
    }
}
