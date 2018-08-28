namespace Kamaji.Data
{
    using Kamaji.Data.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface INodeRepository
    {
        Task<INodeModel> Save(INodeModel node);

        Task<INodeModel> GetBy(string address);

        Task<IEnumerable<INodeModel>> GetAll();

        Task<object> GetIdBy(string nodeAddress);
    }
}
