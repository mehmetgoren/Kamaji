namespace Kamaji.Node
{
    using Kamaji.Common;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;

    public static class ZipExtractor
    {
        /// <summary>
        /// Saves db zip file to local executing folder.
        /// </summary>
        /// <param name="fileName">just set the db Id</param>
        /// <param name="dbData">Db zip data.</param>
        /// <returns>Brand new extracted folder path</returns>
        public static string SaveFiles(string fileName, byte[] zipData)
        {
            if (null != zipData && zipData.Any() && !String.IsNullOrEmpty(fileName))
            {
                string location = Utility.GetExecutionPath();
                string zipFileName = location + "\\" + fileName;
                File.WriteAllBytes(zipFileName, zipData);

                string ret = zipFileName + "\\Extracted";
                ZipFile.ExtractToDirectory(zipFileName, ret);

                return ret;
            }

            return null;
        }
    }
}
