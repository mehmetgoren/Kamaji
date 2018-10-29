namespace KamajiTests
{
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;
    using System.Threading.Tasks;

    [TestClass]
    public class ScanControllerTests
    {
        const string RootPath = @"X:\Projects\Kamaji\src\KamajiTests\";

        [TestMethod]
        public async Task SaveScanPrerequisiteTest()
        {
            int result = 0;
            await RestClient.Instance.SignIn();
            ScanPrerequisiteModel model;

            string path = RootPath + "puppeteer_1.9.0&express_4.16.4.zip";// Utility.GetExecutionPath() + "\\puppeteer_1.9.0&express_4.16.4.zip";

            model = new ScanPrerequisiteModel();
            model.Name = "puppeteer";
            model.Version = "1.0.0";
            model.Resources = await File.ReadAllBytesAsync(path);
            result = await RestClient.Instance.PostAsync<int>("Scan/SaveScanPrerequisite", model);

            Assert.AreEqual(result, 1);
        }

        [TestMethod]
        public async Task SaveScanResourceTest()
        {
            int result = 0;
            await RestClient.Instance.SignIn();
            ScanResourceModel model;


            model = new ScanResourceModel();
            model.Name = "WebPageHtmlSource";
            model.Version = "1.0.0";
            model.ScanPrerequisiteName = "puppeteer";
            model.Resources = await File.ReadAllBytesAsync(RootPath + "WebPageHtmlSourceWorker.zip" /*Utility.GetExecutionPath() + "\\WebPageHtmlSourceWorker.zip*/);
            result = await RestClient.Instance.PostAsync<int>("Scan/SaveScanResource", model);


            model = new ScanResourceModel();
            model.Name = "WebPageScreenshotWorker";
            model.Version = "1.0.0";
            model.ScanPrerequisiteName = "puppeteer";
            model.Resources = await File.ReadAllBytesAsync(RootPath + "WebPageScreenshotWorker.zip");
            result = await RestClient.Instance.PostAsync<int>("Scan/SaveScanResource", model);


            model = new ScanResourceModel();
            model.Name = "WebPageSpider";
            model.Version = "1.0.0";
            model.ScanPrerequisiteName = "puppeteer";
            model.Resources = await File.ReadAllBytesAsync(RootPath + "WebPageSpider.zip");
            result = await RestClient.Instance.PostAsync<int>("Scan/SaveScanResource", model);


            //model = new ScanResourceModel();
            //model.Name = "TerminalWorker";
            //model.Version = "1.0.0";
            //model.Resources = await File.ReadAllBytesAsync(Utility.GetExecutionPath() + "\\TerminalWorker.zip");
            //result = await RestClient.Instance.PostAsync<int>("Scan/SaveScanResource", model);

            //model = new ScanResourceModel();
            //model.Name = "Dollars";
            //model.Version = "1.0.0";
            //model.ScanPrerequisiteName = "puppeteer";
            //model.Resources = await File.ReadAllBytesAsync(Utility.GetExecutionPath() + "\\Dollars.zip");
            //result = await RestClient.Instance.PostAsync<int>("Scan/SaveScanResource", model);

            // model = new ScanResourceModel();
            // model.Name = "nmap_windows";
            // model.Version = "1.0.0";
            //// model.ScanPrerequisiteName = "nmap_windows";
            // model.Resources = await File.ReadAllBytesAsync(Utility.GetExecutionPath() + "\\NmapWorker.zip");
            // result = await RestClient.Instance.PostAsync<int>("Scan/SaveScanResource", model);

            Assert.AreEqual(result, 1);
        }

        [TestMethod]
        public async Task SaveScanTest()
        {
            int result = 0;
            await RestClient.Instance.SignIn();
            ScanModel model;

            //model = new ScanModel();
            //model.Asset = "https://www.w3.org/";
            //model.Period = 5000;
            //model.ResourceName = "WebPageHtmlSource";
            //result = await RestClient.Instance.PostAsync<int>("Scan/SaveScan", model);


            //model = new ScanModel();
            //model.Asset = "https://odatv.com/";
            //model.Period = 10000;
            //model.ResourceName = "WebPageScreenshotWorker";
            //result = await RestClient.Instance.PostAsync<int>("Scan/SaveScan", model);


            model = new ScanModel();
            model.Asset = "http://toastytech.com/evil/";//"https://demos.telerik.com/aspnet-mvc/tripxpert/";
            model.Type = ScanModel.ScanType.Once;
            model.ResourceName = "WebPageSpider";
            result = await RestClient.Instance.PostAsync<int>("Scan/SaveScan", model);


            //model = new ScanModel();
            //model.Asset = "get-process -ComputerName ANKARA";
            //model.SaveType = ScanModel.ScanSaveType.Upsert;
            //model.ResourceName = "TerminalWorker";
            //model.Period = 10000;

            //dynamic dyn = new ExpandoObject();
            //dyn.userName = "admin";
            //dyn.password = "1";
            //model.Args = dyn;

            //result = await RestClient.Instance.PostAsync<int>("Scan/SaveScan", model);


            //model = new ScanModel();
            //model.Asset = "Dollars";
            //model.Period = 5000;
            //model.ResourceName = "Dollars";
            ////model.MaxInstance = 1000000;
            //result = await RestClient.Instance.PostAsync<int>("Scan/SaveScan", model);

            //model = new ScanModel();
            //model.Asset = "-PS 127.0.0.1";
            //model.Period = 10000;
            //model.ResourceName = "nmap_windows";
            //result = await RestClient.Instance.PostAsync<int>("Scan/SaveScan", model);

            Assert.AreEqual(result, 1);
        }
    }
}
