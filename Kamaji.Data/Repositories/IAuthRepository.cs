namespace Kamaji.Data
{
    using Kamaji.Data.Models;
    using System;
    using System.Threading.Tasks;

    public interface IAuthRepository
    {
        Task<IAuthModel> GetBy(Guid token);

        Task<IAuthModel> GetBy(string address);

        Task<IAuthModel> Save(IAuthModel auth);

        Task<int> EditLoginCount(object authId, uint loginCount);
    }
}
