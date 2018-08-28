namespace Kamaji.Data
{
    using Kamaji.Data.Models;
    using System.Threading.Tasks;

    public interface IScanPrerequisiteRepository
    {
        Task<IScanPrerequisiteModel> Save(IScanPrerequisiteModel scanPrerequisite);

        Task<IScanPrerequisiteModel> GetBy(string name);

        Task<object> GetScanPrerequisiteId(string name);

        Task<string> GetNameBy(object prerequisiteId);
    }
}
