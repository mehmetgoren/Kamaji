namespace Kamaji.Data.Models
{
    using System;

    public enum NodeConnectionStatus: sbyte
    {
        Offline = 0,
        Online = 1,
    }

    public interface INodeModel
    {
        object NodeId { get; set; }

        string Address { get; set; }

        string IpAddress { get; set; }

        int Port { get; set; }

        string ComputerName { get; set; }

        ushort ThreadCount { get; set; }

        int TotalMemory { get; set; }

        int TotalExecutingJobCount { get; set; }

        int TotalQueuedJobCount { get; set; }

        /// <summary>
        /// if a worker wants to be offline, it set its ConnectionStatus to offline
        /// </summary>
        NodeConnectionStatus ConnectionStatus {get; set; }

        DateTime LastConnectionTime { get; set; }

        double CpuUsage { get; set; }
        double MemoryUsage { get; set; }
        ulong TotalBeatsCount { get; set; }
    }
}
