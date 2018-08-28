namespace Kamaji.Worker
{
    using System;
    using System.Threading.Tasks;

    public interface IWorker : IDisposable
    {
        Task SetupEnvironment();

        Task<WorkerResult> Run(IObserver observer, string asset, IScanRepository repository, object args);//örneğin Machine Learning ile dolar tahmini yaparken kullanacağız arfs parametresini. Kamaji Adresi verecek.
    }
}
