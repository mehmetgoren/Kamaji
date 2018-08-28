namespace Kamaji.Common.Models
{
    using System.ComponentModel.DataAnnotations;

    public sealed class NodeRegisterModel
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public string IpAddress { get; set; }

        public int Port { get; set; }

        [Required]
        public string ComputerName { get; set; }

        public int ThreadCount { get; set; }

        public int TotalMemory { get; set; }
    }

    public sealed class NodeHeartBeatModel
    {
        [Required]
        public string Address { get; set; }

        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }

        public int TotalExecutingJobCount { get; set; }
        public int TotalQueuedJobCount { get; set; }
    }
}
