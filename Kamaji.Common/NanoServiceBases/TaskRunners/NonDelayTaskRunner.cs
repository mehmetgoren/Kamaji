namespace Kamaji.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The run method does not cause a delay.
    /// </summary>
    public sealed class NonDelayTaskRunner : ITaskRunner
    {
        public static readonly NonDelayTaskRunner Instance = new NonDelayTaskRunner();
        private NonDelayTaskRunner() { }

        public Task Run(Func<IObserver, CancellationToken, Task> func, IObserver observer, CancellationToken cancellationToken)
        {
            if (null != func)
            {
                func(observer, cancellationToken).Start();//await beklemedcen direk interval' ı beklemeye başlar.
            }

            return Task.Delay(0);
        }
    }
}
