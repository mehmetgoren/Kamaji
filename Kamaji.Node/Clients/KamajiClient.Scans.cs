namespace Kamaji.Node
{
    using Kamaji.Common.Models;
    using Kamaji.Node.Offline;
    using System;
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

            public Task<IEnumerable<ScanInstanceModel>> GetScanInstanceListBy(string resourceName, string asset, bool alsoGetParent, bool alsoGetChilds)
               => RestClient.Instance.GetAsync<IEnumerable<ScanInstanceModel>>($"{KamajiScanActions.GetScanInstanceListBy}?resourceName=" + resourceName 
                   + "&asset=" + asset + "&alsoGetParent=" + alsoGetParent +"&alsoGetChilds=" + alsoGetChilds);


            public async Task<int> SaveScanInstance(ScanInstanceModel model)
            {
                try
                {
                    return await RestClient.Instance.PostAsync<int>($"{KamajiScanActions.SaveScanInstance}", model);
                }
                catch(Exception ex)
                {
                    await OfflineModel.From(model, nameof(SaveScanInstance), ex).SaveAsync();
                    return -1;
                }
            }

            public async Task<int> SaveScanInstanceOrEditResult(ScanInstanceModel model)
            {
                try
                {
                    return await RestClient.Instance.PostAsync<int>($"{KamajiScanActions.SaveScanInstanceOrEditResult}", model);
                }
                catch (Exception ex)
                {
                    await OfflineModel.From(model, nameof(SaveScanInstanceOrEditResult), ex).SaveAsync();
                    return -1;
                }
            }


            public async Task<int> EditScan(ScanModel model)
            {
                try
                {
                    return await RestClient.Instance.PostAsync<int>($"{KamajiScanActions.EditScan}", model);
                }
                catch(Exception ex)
                {
                    await OfflineModel.From(model, nameof(EditScan), ex).SaveAsync();
                    return -1;
                }
            }

            public async Task<int> AddChildScans(AddChildScansModel model)
            {
                try
                {
                    return await RestClient.Instance.PostAsync<int>($"{KamajiScanActions.AddChildScans}", model);
                }
                catch(Exception ex)
                {
                    await OfflineModel.From(model, nameof(EditScan), ex).SaveAsync();
                    return -1;
                }
            }
        }
    }
}