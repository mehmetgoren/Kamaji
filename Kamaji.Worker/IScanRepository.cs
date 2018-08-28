namespace Kamaji.Worker
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IScanRepository
    {
        Task AddChildScan(string parentAsset, IEnumerable<string> childAssetList);

        Task<IEnumerable<string>> GetResults(bool alsoGetParentResult, bool alsoGetChildsResults);
    }
}
