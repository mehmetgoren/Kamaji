namespace Kamaji.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The Run method waits until the run operation done.
    /// </summary>
    public sealed class SimpleTaskRunner : ITaskRunner
    {
        public static readonly SimpleTaskRunner Instance = new SimpleTaskRunner();
        private SimpleTaskRunner() { }

        public async Task Run(Func<IObserver, CancellationToken, Task> func, IObserver observer, CancellationToken cancellationToken)
        {
            if (null != func)
            {
                await func(observer, cancellationToken);
            }
        }
    }
}
