namespace Kamaji.Common
{
    using System;

    public interface IResourceMonitor
    {
        string GetOsVersion();

        int GetProcessorCount();
        double GetCpuFrequency();
        double GetCpuUsage();

        double GetTotalMemory();
        double GetFreeMemory();
        double GetMemoryUsage();

        double GetTotalDiskSize();
        double GetFreeDiskSize();
        double GetDiskUsage();
    }

    public abstract class ResourceMonitorBase : IResourceMonitor
    {
        public abstract string GetOsVersion();


        public virtual int GetProcessorCount() => Environment.ProcessorCount;
        public abstract double GetCpuFrequency();
        public abstract double GetCpuUsage();


        public abstract double GetTotalMemory();
        public abstract double GetFreeMemory();
        public virtual double GetMemoryUsage()
        {
            try
            {
                double free = this.GetFreeMemory();
                double total = this.GetTotalMemory();

                return Math.Round((total - free) / total * 100.0);

            }
            catch
            {
                return -1.0;
            }
        }


       
        public abstract double GetTotalDiskSize();
        public abstract double GetFreeDiskSize();
        public virtual double GetDiskUsage()
        {
            {
                try
                {
                    double free = this.GetFreeDiskSize();
                    double total = this.GetTotalDiskSize();

                    return Math.Round((total - free) / total * 100.0);

                }
                catch
                {
                    return -1.0;
                }
            }
        }
    }
}
