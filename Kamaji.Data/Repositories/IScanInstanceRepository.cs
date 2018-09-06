namespace Kamaji.Data
{
    using Kamaji.Data.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IScanInstanceRepository
    {
        Task<IScanInstanceModel> Save(IScanInstanceModel scanInstance);

        Task<IEnumerable<IScanInstanceModel>> GetListBy(IEnumerable<object> scanIds);

        //for ScanSaveType.Upsert
        Task<int> EditResult(object scanId, object nodeId, DateTime startTime, DateTime? endTime, string result);
        Task<IScanInstanceModel> GetFirstBy(object scanId);

        Task<int> TrimToSize(object scanId, int max);//IScanModel.MaxInstance
    }
}
