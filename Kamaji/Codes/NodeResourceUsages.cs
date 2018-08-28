namespace Kamaji
{
    using Kamaji.Data.Models;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class NodeResourceUsage
    {
        private readonly List<double> _cpuUsageList = new List<double>();
        private readonly List<double> _memoryUsageList = new List<double>();
        private ulong _totalBeatsCount;

        internal NodeResourceUsage Add(double cpuUsage, double memoryUsage)
        {
            this._cpuUsageList.Add(cpuUsage);
            this._memoryUsageList.Add(memoryUsage);
            this._totalBeatsCount++;

            return this;
        }

        internal void Reset()
        {
            this._cpuUsageList.Clear();
            this._memoryUsageList.Clear();
            this._totalBeatsCount = 0;
        }

        internal double CpuUsage => this._cpuUsageList.Sum() / Math.Max(this._totalBeatsCount, 1);
        internal double MemoryUsage => this._memoryUsageList.Sum() / Math.Max(this._totalBeatsCount, 1);
        internal ulong TotalBeatsCount => this._totalBeatsCount;
    }

    internal class NodeResourceUsages
    {
        internal static readonly NodeResourceUsages Instance = new NodeResourceUsages();
        private readonly ConcurrentDictionary<string, NodeResourceUsage> _dic = new ConcurrentDictionary<string, NodeResourceUsage>();

        internal NodeResourceUsage Get(string name)
        {
            if (!this._dic.TryGetValue(name, out NodeResourceUsage usage))
            {
                usage = new NodeResourceUsage();
                this._dic[name] = usage;
            }

            return usage;
        }
    }
}
