namespace Kamaji
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Kamaji.Common;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    internal static partial class DataSources
    {
        internal static class Jsons
        {
            internal static class AppSettings
            {
                private static readonly IConfigurationRoot Configuration =
                    new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

                internal static class Config
                {
                    internal static string Address
                    {
                        get
                        {
                            string value = Configuration[$"{nameof(Config)}:{nameof(Address)}"];
                            if (String.IsNullOrEmpty(value))
                            {
                                IPAddress ipv4 = SystemInfo.GetIpv4Address();

                                if (null != ipv4)
                                    return "http://" + ipv4 + ":1881";
                            }

                            return value;
                        }
                    }

                    internal static string ConnectionString
                    {
                        get
                        {
                            string value = Configuration[$"{nameof(Config)}:{nameof(ConnectionString)}"];
                            if (String.IsNullOrEmpty(value))
                                throw new InvalidOperationException("No Connection string found on appsettings");

                            return value;
                        }
                    }

                    internal static string KamajiDataPath
                    {
                        get
                        {
                            string value = Configuration[$"{nameof(Config)}:{nameof(KamajiDataPath)}"];
                            if (String.IsNullOrEmpty(value))
                                throw new NullReferenceException($"{nameof(KamajiDataPath)} can not be null or empty. Please set the value that in appsettings.json");

                            return value;
                        }
                    }

                    private static readonly object syncRoot = new object();
                    private static NodesConfig nodes;
                    internal static NodesConfig Nodes
                    {
                        get
                        {
                            if (null == nodes)
                            {
                                lock (syncRoot)
                                {
                                    if (null == nodes)
                                    {
                                        nodes = new NodesConfig();

                                        try
                                        {
                                            var nodesConfig = Configuration.GetSection($"{nameof(Config)}:{nameof(Nodes)}");
                                            List<IConfigurationSection> children = nodesConfig.GetChildren().ToList();
                                            nodes.Timeout = int.Parse(children[0].Value);
                                        }
                                        catch { }
                                    }
                                }
                            }

                            return nodes;
                        }
                    }
                }
            }
        }
    }

    internal sealed class NodesConfig
    {
        public int Timeout { get; set; } = 10;
    }
}
