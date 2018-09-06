namespace Kamaji.Data
{
    using Kamaji.Data.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IScanRepository
    {
        Task<IScanModel> Save(IScanModel scan);

        Task<IEnumerable<IScanModel>> GetListBy(bool enabled, params ScanState[] states);//postgres enu  flags i desteklemediğinden params yapıldı.

        Task<IScanModel> GetBy(string assset, object scanResourceId);

        Task<int> Edit(IScanModel model);//to edit stete to asiigned.

        Task<int> BatchSave(IEnumerable<IScanModel> scanList);

        Task<IEnumerable<IScanModel>> GetRecursivelyChildList(object parentId);//postgres de recuirsaive çağır

        Task<IScanModel> GetBy(object scanId);
    }
}
