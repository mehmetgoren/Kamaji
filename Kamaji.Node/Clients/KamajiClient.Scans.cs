namespace Kamaji.Node
{
    using Kamaji.Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;


    partial class KamajiClient
    {
        public Scan Scans { get; }

        public sealed class Scan
        {
            private static class KamajiScanActions
            {
                internal const string GetScanPrerequisiteBy = "api/Scan/" + nameof(GetScanPrerequisiteBy);

                internal const string GetScanResourceBy = "api/Scan/" + nameof(GetScanResourceBy);

                internal const string SaveScanInstance = "api/Scan/" + nameof(SaveScanInstance);

                internal const string EditScan = "api/Scan/" + nameof(EditScan);

                internal const string AddChildScans = "api/Scan/" + nameof(AddChildScans);

                internal const string GetScanInstanceListBy = "api/Scan/" + nameof(GetScanInstanceListBy);

                internal const string SaveScanInstanceOrEditResult = "api/Scan/" + nameof(SaveScanInstanceOrEditResult);
            }


            public Task<ScanPrerequisiteModel> GetScanPrerequisiteBy(string scanPrerequisiteName)
                => RestClient.Instance.GetAsync<ScanPrerequisiteModel>($"{KamajiScanActions.GetScanPrerequisiteBy}?name=" + scanPrerequisiteName);


            public Task<ScanResourceModel> GetScanResourceBy(string scanResourceName)
                => RestClient.Instance.GetAsync<ScanResourceModel>($"{KamajiScanActions.GetScanResourceBy}?name=" + scanResourceName);

            public Task<int> SaveScanInstance(ScanInstanceModel model)
                => RestClient.Instance.PostAsync<int>($"{KamajiScanActions.SaveScanInstance}", model);

            public Task<int> EditScan(ScanModel model)
                => RestClient.Instance.PostAsync<int>($"{KamajiScanActions.EditScan}", model);

            public Task<int> AddChildScans(AddChildScansModel model)
                => RestClient.Instance.PostAsync<int>($"{KamajiScanActions.AddChildScans}", model);

            public Task<IEnumerable<ScanInstanceModel>> GetScanInstanceListBy(string resourceName, string asset, bool alsoGetParent, bool alsoGetChilds)
               => RestClient.Instance.GetAsync<IEnumerable<ScanInstanceModel>>($"{KamajiScanActions.GetScanInstanceListBy}?resourceName=" + resourceName 
                   + "&asset=" + asset + "&alsoGetParent=" + alsoGetParent +"&alsoGetChilds=" + alsoGetChilds);

            public Task<int> SaveScanInstanceOrEditResult(ScanInstanceModel model)
                => RestClient.Instance.PostAsync<int>($"{KamajiScanActions.SaveScanInstanceOrEditResult}", model);
        }
    }
}