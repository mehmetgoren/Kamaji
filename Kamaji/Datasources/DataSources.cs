namespace Kamaji
{
    using System;
    using System.IO;
    using System.Net;
    using Kamaji.Common;
    using Microsoft.Extensions.Configuration;

    public static partial class DataSources
    {
        public static class Jsons
        {
            public static class AppSettings
            {
                private static readonly IConfigurationRoot File =
                    new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

                public static class Config
                {
                    public static string Address
                    {
                        get
                        {
                            string value = File[$"{nameof(Config)}:{nameof(Address)}"];
                            if (String.IsNullOrEmpty(value))
                            {
                                IPAddress ipv4 = SystemInfo.GetIpv4Address();

                                if (null != ipv4)
                                    return "http://" + ipv4 + ":1881";
                            }

                            return value;
                        }
                    }

                    public static string KamajiDataPath
                    {
                        get
                        {
                            string value = File[$"{nameof(Config)}:{nameof(KamajiDataPath)}"];
                            if (String.IsNullOrEmpty(value))
                                throw new NullReferenceException($"{nameof(KamajiDataPath)} can not be null or empty. Please set the value that in appsettings.json");

                            return value;
                        }
                    }
                }
            }
        }
    }
}
