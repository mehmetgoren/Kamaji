namespace Kamaji.Data
{
    using Kamaji.Data.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IScanRepository
    {
        Task<IScanModel> Save(IScanModel scan);

        Task<IEnumerable<IScanModel>> GetListBy(bool enabled, params ScanState[] states);

        Task<IEnumerable<IScanModel>> GetOnDemandListBy(bool enabled, params ScanState[] states);//postgres enu  flags i desteklemediğinden params yapıldı.

        Task<IEnumerable<IScanModel>> GetScheduledListBy(bool enabled, DateTime date, params ScanState[] states);//postgres enu  flags i desteklemediğinden params yapıldı.

        Task<IScanModel> GetBy(string assset, object scanResourceId);

        Task<int> Edit(IScanModel model);//to edit stete to asiigned.

        Task<IEnumerable<IScanModel>> GetRecursivelyChildList(object parentId);//postgres de recuirsaive çağır

        Task<IScanModel> GetBy(object scanId);

        Task<IEnumerable<IScanModel>> GetListByLastAssignedNodeId(bool enabled, object nodeId);
    }
}
