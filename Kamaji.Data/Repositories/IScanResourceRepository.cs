namespace Kamaji.Data
{
    using Kamaji.Data.Models;
    using System.Threading.Tasks;

    public interface IScanResourceRepository
    {
        Task<IScanResourceModel> Save(IScanResourceModel scanResource);

        Task<IScanResourceModel> GetBy(string name);

        Task<object> GetScanResourceIdBy(string name);

        Task<IScanResourceModel> GetBy(object scanResourceId);
    }
}
