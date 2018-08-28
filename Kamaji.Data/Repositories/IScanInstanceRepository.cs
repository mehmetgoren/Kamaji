namespace Kamaji.Data
{
    using Kamaji.Data.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IScanInstanceRepository
    {
        Task<IScanInstanceModel> Save(IScanInstanceModel scanInstance);

        Task<IEnumerable<IScanInstanceModel>> GetListBy(IEnumerable<object> scanIds);
    }
}
