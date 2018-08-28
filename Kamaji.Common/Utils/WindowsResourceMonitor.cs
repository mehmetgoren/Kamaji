namespace Kamaji.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    public class WindowsResourceMonitor : ResourceMonitorBase
    {
        protected static List<string> ExecuteCmd(string args)
        {
            List<string> results = new List<string>();

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = args, //[environment]::OSVersion.Version // "/c powershell service", //"/c ipconfig",  //"/c ping 192.168.0.21",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();


            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();

                if (!String.IsNullOrEmpty(line))
                    results.Add(line);
            }

            return results;
        }


        public override string GetOsVersion()
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                ExecuteCmd("/c powershell [environment]::OSVersion.Version")[2].Split(' ')
                    .Where(i => double.TryParse(i, out double temp)).ToList().ForEach(i => sb.Append(i).Append('.'));
                if (sb.Length > 0)
                    sb.Remove(sb.Length - 1, 1);
            }
            catch
            {
                sb.Append("can not obtain windows server info");
            }

            return sb.ToString();
        }



        public override double GetCpuFrequency() => 1.0;//çarpanda etkisiz olsun diye. Çünkü windows' tan alamıyoruz.

        public override double GetCpuUsage()
        {
            try
            {
                var list = ExecuteCmd(@"/c powershell Get-Counter '\Processor(_Total)\% Processor Time'");
                return Math.Round(Convert.ToDouble(list[3])) * 4.0;
            }
            catch
            {
                return -1.0;
            }
        }



        public override double GetTotalMemory()
        {
            var list = ExecuteCmd("/c powershell [Math]::Round((Get-WmiObject -Class win32_computersystem -ComputerName localhost).TotalPhysicalMemory/1Mb)");
            double total = Convert.ToDouble(list[0]);
            return total;
        }

        public override double GetFreeMemory()
        {
            var list = ExecuteCmd(@"/c powershell Get-Counter '\Memory\Available MBytes'");
            double free = Convert.ToDouble(list[3]);
            return free;
        }




        public override double GetTotalDiskSize()
        {
            try
            {
                var list = ExecuteCmd(@"/c powershell Get-WmiObject -Class win32_logicalDisk");

                double total = 0.0;
                for (int j = 0; j < list.Count; j += 6)
                {
                    total += Convert.ToDouble(list[j + 4].Split(':')[1]);
                }

                return total;

            }
            catch
            {
                return -1.0;
            }
        }

        public override double GetFreeDiskSize()
        {
            try
            {
                var list = ExecuteCmd(@"/c powershell Get-WmiObject -Class win32_logicalDisk");

                double free = 0.0;
                for (int j = 0; j < list.Count; j += 6)
                {
                    free += Convert.ToDouble(list[j + 3].Split(':')[1]);
                }

                return free;

            }
            catch
            {
                return -1.0;
            }
        }

        public override double GetDiskUsage()//overrided, because it' s more perfomant.
        {
            try
            {
                var list = ExecuteCmd(@"/c powershell Get-WmiObject -Class win32_logicalDisk");

                double free = 0.0, total = 0.0;
                for (int j = 0; j < list.Count; j += 6)
                {
                    free += Convert.ToDouble(list[j + 3].Split(':')[1]);
                    total += Convert.ToDouble(list[j + 4].Split(':')[1]);
                }

                return Math.Round((total - free) / total * 100.0);

            }
            catch
            {
                return -1.0;
            }
        } 
    }
}
