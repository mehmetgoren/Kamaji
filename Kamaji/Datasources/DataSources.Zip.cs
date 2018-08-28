namespace Kamaji
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    partial class DataSources
    {
        public static class InternalResources
        {
            private const string Path = @"\Datasources\InternalResources\Zip\";
            /// <summary>
            /// get a specific local zip file.
            /// </summary>
            /// <param name="id">file name</param>
            /// <returns>zip file as array</returns>
            public static async Task<byte[]> GetZip(string id)
            {
                if (!String.IsNullOrEmpty(id))
                {
                    string location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string zipPath = location + Path + id + ".zip";

                    if (File.Exists(zipPath))
                    {
                        try
                        {
                            return await File.ReadAllBytesAsync(id);
                        }
                        catch { }
                    }
                }

                return null;
            }
        }
    }
}
