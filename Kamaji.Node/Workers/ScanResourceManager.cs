namespace Kamaji.Node
{
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class ScanResourceManager
    {
        private static readonly string ScanResourcesPath = Utility.GetExecutionPath() + "\\ScanResources\\";


        private static async Task ExtractZip(string folderPath, byte[] files)
        {
            string zipFilePath = folderPath + ".zip";

            await File.WriteAllBytesAsync(zipFilePath, files);
            ZipFile.ExtractToDirectory(zipFilePath, folderPath);
            File.Delete(zipFilePath);
        }

        private static bool IsScanPrereqExist(string scanPrerequisiteName) => Directory.Exists(ScanResourcesPath + scanPrerequisiteName);
        private static async Task<DirectoryInfo> CreateScanPrereqFolder(string scanPrerequisiteName, byte[] files)
        {
            string folderPath = ScanResourcesPath + scanPrerequisiteName;
            DirectoryInfo ret = Directory.CreateDirectory(folderPath);

            await ExtractZip(folderPath, files);

            return ret;
        }
        private static DirectoryInfo GetExistPrereq(string scanPrerequisiteName) => new DirectoryInfo(ScanResourcesPath + scanPrerequisiteName);

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        internal static async Task<DirectoryInfo> GetResourceDirectory(ScanModel model)
        {
            await _semaphoreSlim.WaitAsync();

            DirectoryInfo resourceFolder = null;
            try
            {
                DirectoryInfo preReqFolder = null;
                string resourceFolderPath = null;
                if (!String.IsNullOrEmpty(model.PrerequisiteName))
                {
                    if (!IsScanPrereqExist(model.PrerequisiteName))
                    {
                        ScanPrerequisiteModel prerequisiteModel = await KamajiClient.Instance.Scans.GetScanPrerequisiteBy(model.PrerequisiteName);
                        preReqFolder = await CreateScanPrereqFolder(prerequisiteModel.Name, prerequisiteModel.Resources);
                    }
                    else
                        preReqFolder = GetExistPrereq(model.PrerequisiteName);

                    resourceFolderPath = preReqFolder.FullName + "\\" + model.ResourceName;
                }
                else
                    resourceFolderPath = ScanResourcesPath + model.ResourceName;//if it has no prereq, just copy to the root folder.



                if (!Directory.Exists(resourceFolderPath))
                {
                    ScanResourceModel resourceModel = await KamajiClient.Instance.Scans.GetScanResourceBy(model.ResourceName);
                    if (null != resourceModel)
                    {
                        byte[] resources = resourceModel.Resources;
                        resourceFolder = Directory.CreateDirectory(resourceFolderPath);

                        await ExtractZip(resourceFolderPath, resources);
                    }
                }
                else
                    resourceFolder = new DirectoryInfo(resourceFolderPath);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return resourceFolder;
        }
    }
}
