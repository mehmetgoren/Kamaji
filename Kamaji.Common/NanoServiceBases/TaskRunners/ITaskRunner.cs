namespace Kamaji.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;


    public interface ITaskRunner
    {
        Task Run(Func<IObserver, CancellationToken, Task> func, IObserver observer, CancellationToken cancellationToken);
    }
}
