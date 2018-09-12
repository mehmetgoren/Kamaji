namespace Kamaji
{
    using ionix.Utils.Extensions;
    using ionix.Utils.Reflection;
    using Kamaji.Common.Models;
    using Kamaji.Data.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed class NodesClient
    {
        public static readonly NodesClient Instance = new NodesClient();
        private NodesClient()
        {

        }

        private static class NodesActions
        {
            internal const string AssignScan = "api/Kamaji/" + nameof(AssignScan);
            internal const string StopService = "api/Kamaji/" + nameof(StopService);

            internal const string GetOfflineData = "api/Kamaji/" + nameof(GetOfflineData);
            internal const string DeleteOfflineData = "api/Kamaji/" + nameof(DeleteOfflineData);
        }

        // işte şimdi node' a scan leri göndermemiz için bu çift raraflı dublex' i kullanacağız.

        public Task<int> AssignScan(INodeModel node, string prerequisiteName, string scanResourceName, IScanModel scan)
        {
            RestClient client = new RestClient(node.Address);

            ScanModel model = new ScanModel();
            model.CopyPropertiesFrom(scan);

            model.Type = scan.Type.Cast<int>().Cast<ScanModel.ScanType>();
            model.PrerequisiteName = prerequisiteName;
            model.ResourceName = scanResourceName;
            //model.State = scan.State.Cast<int>().Cast<ScanModel.ScanState>();
            model.SaveType = scan.SaveType.Cast<int>().Cast<ScanModel.ScanSaveType>();
            model.Args = scan.Args;
            model.SaveNullResult = scan.SaveNullResult;

            return client.PostAsync<int>(NodesActions.AssignScan, model);
        }


        public Task<bool> StopService(INodeModel node, string resourceName, string asset)
        {
            RestClient client = new RestClient(node.Address);

            return client.GetAsync<bool>(NodesActions.StopService + "?resourceName=" + resourceName + "&asset=" + asset);
        }


        public Task<IEnumerable<OfflineDataModel>> GetOfflineData(string nodeAddress)
            => new RestClient(nodeAddress).GetAsync<IEnumerable<OfflineDataModel>>(NodesActions.GetOfflineData);

        public Task<int> DeleteOfflineData(string nodeAddress, int id)
            => new RestClient(nodeAddress).GetAsync<int>(NodesActions.DeleteOfflineData + "?id=" + id);
    }
}
