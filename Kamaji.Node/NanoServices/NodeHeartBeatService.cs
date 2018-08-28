namespace Kamaji.Node
{
    using Kamaji.Common;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class NodeHeartBeatService : NanoServiceBase//singleton
    {
        public static readonly NodeHeartBeatService Instance = new NodeHeartBeatService();
        private NodeHeartBeatService()
            :base(true, ConsoleObserver.Instance, TimeSpan.FromSeconds(5))
        {

        }

        protected override ITaskRunner CreateTaskRunner() => SimpleTaskRunner.Instance;

        protected override async Task Execute(IObserver observer, CancellationToken cancellationToken)
        {
            await KamajiClient.Instance.Nodes.HeartBeat();
            observer.Notify("NodeHeart", $"My heartbeat ({DataSources.Jsons.AppSettings.Config.Address}) has been send to {DataSources.Jsons.AppSettings.Config.BrokerAddress}", null);
        }
    }
}
