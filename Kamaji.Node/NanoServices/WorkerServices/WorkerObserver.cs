namespace Kamaji.Node
{
    using Kamaji.Common;

    public class WorkerObserver : IObserver
    {
        public static readonly WorkerObserver Instance = new WorkerObserver();
        private WorkerObserver() { }

        public void Notify(string id, string message, object args)
        {
            //Bu web öncelikle Kamajiye o da websocket lere gönderecek.
            //şu an işçin bu var.
            ConsoleObserver.Instance.Notify(id, message, args);
        }
    }
}
